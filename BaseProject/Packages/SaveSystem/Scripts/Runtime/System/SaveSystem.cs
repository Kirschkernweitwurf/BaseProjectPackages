using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Base.SaveSystemPackage.Encryption;
using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Serialization;
using Base.SaveSystemPackage.Serialization.Wire;
using Base.SaveSystemPackage.Storage;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.System
{
    /// <summary>
    /// The default <see cref="ISaveSystem"/>. Uses an <see cref="ISaveStorage"/> for bytes, an
    /// <see cref="ISaveCodec"/> for serialize/encrypt, and an injected <see cref="ISavableRegistry"/>
    /// for the objects to collect from (no global statics).
    ///
    /// Each slot is a folder holding up to three files: the data, the screenshot, and the metadata.
    /// The metadata is written LAST and acts as the commit marker: if it is present, the save is
    /// complete. A crash mid-save therefore never looks like a finished save.
    ///
    /// Writes are serialized through a gate so two saves cannot interleave; <see cref="FlushAsync"/>
    /// waits for the current one.
    /// </summary>
    public sealed class SaveSystem : ISaveSystem
    {
        private const string DataSuffix = "/Save.dat";
        private const string MetaSuffix = "/Meta.dat";
        private const string ShotSuffix = "/Screenshot.png";

        private readonly int _saveVersion;
        private readonly ISaveCodec _codec;
        private readonly ISaveStorage _storage;
        private readonly ISavableRegistry _registry;
        private readonly IReadOnlyList<ISaveMigration> _migrations;
        private readonly SemaphoreSlim _writeGate = new(1, 1);

        public SaveSystem(ISaveStorage storage, ISaveCodec codec, ISavableRegistry registry, int saveVersion = 1,
            IReadOnlyList<ISaveMigration> migrations = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _saveVersion = saveVersion;
            _migrations = (migrations ?? Array.Empty<ISaveMigration>())
                .OrderBy(m => m.FromVersion)
                .ToList();
        }

        public async Awaitable SaveAsync(SaveRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.SlotId))
                throw new ArgumentException("SaveRequest.SlotId must be set.", nameof(request));

            await _writeGate.WaitAsync(ct);
            try
            {
                await Awaitable.MainThreadAsync();

                SaveBlob blob = new();
                foreach (ISavable s in _registry.GetOrdered())
                    blob.Add(s.PersistentKey.Value, s.Serialize() ?? string.Empty);

                SaveMetadata meta = BuildMetadata(await LoadMetadataAsync(request.SlotId, ct), request);

                byte[] dataBytes = _codec.Encode(blob);
                byte[] metaBytes = _codec.Encode(SaveMetadataDto.From(meta));

                await _storage.WriteAsync(DataKey(request.SlotId), dataBytes, ct);

                if (request.Screenshot is { Png: { Length: > 0 } } shot)
                    await _storage.WriteAsync(ShotKey(request.SlotId), shot.Png, ct);

                await _storage.WriteAsync(MetaKey(request.SlotId), metaBytes, ct);
            }
            finally
            {
                _writeGate.Release();
            }
        }

        public async Awaitable<ESaveLoadResult> LoadAsync(string slotId, CancellationToken ct = default)
        {
            byte[] metaBytes = await _storage.ReadAsync(MetaKey(slotId), ct);
            if (metaBytes == null)
                return ESaveLoadResult.NotFound;

            byte[] dataBytes = await _storage.ReadAsync(DataKey(slotId), ct);
            if (dataBytes == null)
            {
                CustomLogger.LogWarning($"Slot '{slotId}' has metadata but no data; treating as corrupt.", null);
                return ESaveLoadResult.Corrupt;
            }

            SaveMetadataDto metaDto;
            SaveBlob blob;
            try
            {
                metaDto = _codec.Decode<SaveMetadataDto>(metaBytes);
                blob = _codec.Decode<SaveBlob>(dataBytes);
            }
            catch (Exception e)
            {
                CustomLogger.LogWarning($"Failed to decode slot '{slotId}': {e.Message}", null);
                return ESaveLoadResult.Corrupt;
            }

            int storedVersion = metaDto.saveVersion;
            if (storedVersion > _saveVersion)
            {
                CustomLogger.LogWarning($"Slot '{slotId}' was saved at version {storedVersion}, " +
                                        $"newer than supported version {_saveVersion}.", null);
                return ESaveLoadResult.VersionTooNew;
            }

            Dictionary<string, string> states = blob.ToLookup();

            if (storedVersion < _saveVersion && !TryMigrate(slotId, states, storedVersion))
                return ESaveLoadResult.Corrupt;

            foreach (ISavable s in _registry.GetOrdered())
                s.Deserialize(states.GetValueOrDefault(s.PersistentKey.Value));

            return ESaveLoadResult.Success;
        }

        public async Awaitable<bool> ExistsAsync(string slotId, CancellationToken ct = default)
        {
            return await _storage.ExistsAsync(MetaKey(slotId), ct);
        }

        public async Awaitable DeleteAsync(string slotId, CancellationToken ct = default)
        {
            await _writeGate.WaitAsync(ct);
            try
            {
                await _storage.DeleteAsync(MetaKey(slotId), ct);
                await _storage.DeleteAsync(DataKey(slotId), ct);
                await _storage.DeleteAsync(ShotKey(slotId), ct);
            }
            finally
            {
                _writeGate.Release();
            }
        }

        public async Awaitable<SaveMetadata> LoadMetadataAsync(string slotId, CancellationToken ct = default)
        {
            byte[] bytes = await _storage.ReadAsync(MetaKey(slotId), ct);
            if (bytes == null)
                return null;

            try
            {
                return _codec.Decode<SaveMetadataDto>(bytes).ToDomain();
            }
            catch (Exception e)
            {
                CustomLogger.LogWarning($"Failed to decode metadata for slot '{slotId}': {e.Message}", null);
                return null;
            }
        }

        public async Awaitable<byte[]> LoadScreenshotPngAsync(string slotId, CancellationToken ct = default)
        {
            return await _storage.ReadAsync(ShotKey(slotId), ct);
        }

        public async Awaitable<IReadOnlyList<SaveMetadata>> ListSavesAsync(CancellationToken ct = default)
        {
            IReadOnlyList<string> keys = await _storage.ListKeysAsync(null, ct);

            List<SaveMetadata> result = new();
            foreach (string key in keys)
            {
                if (!key.EndsWith(MetaSuffix))
                    continue;

                byte[] bytes = await _storage.ReadAsync(key, ct);
                if (bytes == null)
                    continue;

                try
                {
                    result.Add(_codec.Decode<SaveMetadataDto>(bytes).ToDomain());
                }
                catch
                {
                    // Skip but name it, so a corrupt save is diagnosable rather than silently gone.
                    CustomLogger.LogWarning($"Skipping unreadable save metadata for key '{key}'.", null);
                }
            }

            return result;
        }

        public async Awaitable FlushAsync(CancellationToken ct = default)
        {
            await _writeGate.WaitAsync(ct);
            _writeGate.Release();
        }

        private SaveMetadata BuildMetadata(SaveMetadata existing, SaveRequest request)
        {
            DateTime nowUtc = DateTime.UtcNow;

            SaveMetadata meta = existing ?? SaveMetadata.CreateNew(request.SlotId, _saveVersion, Application.version, nowUtc);

            meta = meta.With(
                saveVersion: _saveVersion,
                appVersion: Application.version,
                lastSavedUtc: nowUtc,
                displayName: request.DisplayName,
                totalPlayTime: request.PlaytimeSeconds.HasValue
                    ? TimeSpan.FromSeconds(request.PlaytimeSeconds.Value)
                    : null);

            if (request.Screenshot is { Png: { Length: > 0 } } shot)
                meta = meta.With(hasScreenshot: true, screenshotWidth: shot.Width, screenshotHeight: shot.Height);

            return meta;
        }

        private bool TryMigrate(string slotId, IDictionary<string, string> states, int fromVersion)
        {
            try
            {
                for (int v = fromVersion; v < _saveVersion; v++)
                {
                    ISaveMigration step = _migrations.FirstOrDefault(m => m.FromVersion == v);
                    if (step == null)
                    {
                        CustomLogger.LogError($"No migration from version {v} for slot '{slotId}'." +
                                              " Cannot upgrade save.", null);
                        return false;
                    }

                    step.Migrate(states);
                }

                return true;
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Migration failed for slot '{slotId}': {e.Message}", null);
                return false;
            }
        }

        private static string DataKey(string slotId) => slotId + DataSuffix;
        private static string MetaKey(string slotId) => slotId + MetaSuffix;
        private static string ShotKey(string slotId) => slotId + ShotSuffix;
    }
}