using System;
using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Encryption;
using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.Savable;
using Base.SaveSystemPackage.Serialization;
using Base.SaveSystemPackage.Serialization.Wire;
using Base.SaveSystemPackage.Storage;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Core
{
    /// <summary>
    /// The default <see cref="ISaveSystem"/>. Uses an <see cref="ISaveStorage"/> for bytes, an
    /// <see cref="ISaveCodec"/> for serialize and encrypt, and an injected <see cref="ISavableRegistry"/>
    /// for the objects to collect from, so there are no global statics.
    /// Each slot is a folder holding up to three files: the data, the screenshot and the metadata.
    /// The metadata is written last and acts as the commit marker: if it is present, the save is
    /// complete. A crash mid-save therefore never looks like a finished save.
    /// Writes are serialized through a gate so two saves cannot interleave; <see cref="FlushAsync"/>
    /// waits for the current one.
    /// State is collected and applied on the main thread, while encode and decrypt work runs on a
    /// background thread so large saves do not hitch the frame.
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
        private readonly Dictionary<int, ISaveMigration> _migrations = new();
        private readonly SemaphoreSlim _writeGate = new(1, 1);

        /// <param name="storage">Where the raw bytes live. Swap this layer for a console save API.</param>
        /// <param name="codec">Turns objects into bytes, including the header and encryption.</param>
        /// <param name="registry">The savables to collect state from and hand state back to.</param>
        /// <param name="saveVersion">The schema version written into new saves.</param>
        /// <param name="migrations">Steps that upgrade an older save one version at a time.</param>
        /// <exception cref="ArgumentNullException">When storage, codec or registry is null.</exception>
        public SaveSystem(ISaveStorage storage, ISaveCodec codec, ISavableRegistry registry, int saveVersion = 1,
            IReadOnlyList<ISaveMigration> migrations = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _saveVersion = saveVersion;

            if (migrations == null)
                return;

            foreach (ISaveMigration migration in migrations)
            {
                if (migration != null)
                    _migrations[migration.FromVersion] = migration;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">When the request carries no slot id.</exception>
        public async Awaitable SaveAsync(SaveRequest request, CancellationToken ct = default)
        {
            // Throwing rather than logging: a save that silently did nothing is worse than a loud stop,
            // and the caller must not report success. SaveSlotButtonBase already catches and logs.
            if (string.IsNullOrWhiteSpace(request.SlotId))
                throw new ArgumentException($"{nameof(SaveRequest)}.{nameof(SaveRequest.SlotId)} must be set.",
                    nameof(request));

            await _writeGate.WaitAsync(ct);
            try
            {
                await Awaitable.MainThreadAsync();

                SaveBlob blob = new();
                foreach (ISavable savable in _registry.GetOrdered())
                    blob.Add(savable.PersistentKey.Value, savable.Serialize() ?? string.Empty);

                SaveMetadata metadata = BuildMetadata(await LoadMetadataAsync(request.SlotId, ct), request);
                SaveMetadataDto metadataDto = SaveMetadataDto.From(metadata);

                // Encode (serialize and encrypt) off the main thread; it is pure CPU work.
                await Awaitable.BackgroundThreadAsync();
                byte[] dataBytes = _codec.Encode(blob);
                byte[] metaBytes = _codec.Encode(metadataDto);
                await Awaitable.MainThreadAsync();

                await _storage.WriteAsync(DataKey(request.SlotId), dataBytes, ct);

                if (TryGetScreenshot(request, out ScreenshotData screenshot))
                    await _storage.WriteAsync(ShotKey(request.SlotId), screenshot.Png, ct);

                // The metadata is the commit marker, so it has to be the last thing written.
                await _storage.WriteAsync(MetaKey(request.SlotId), metaBytes, ct);
            }
            finally
            {
                _writeGate.Release();
            }
        }

        /// <inheritdoc/>
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

            // Decode (decrypt and deserialize) off the main thread; it is pure CPU work.
            SaveMetadataDto metadataDto = null;
            SaveBlob blob = null;
            Exception decodeError = null;

            await Awaitable.BackgroundThreadAsync();
            try
            {
                metadataDto = _codec.Decode<SaveMetadataDto>(metaBytes);
                blob = _codec.Decode<SaveBlob>(dataBytes);
            }
            catch (Exception exception)
            {
                decodeError = exception;
            }

            await Awaitable.MainThreadAsync();

            if (decodeError != null)
            {
                CustomLogger.LogWarning($"Failed to decode slot '{slotId}': {decodeError.Message}", null);
                return ESaveLoadResult.Corrupt;
            }

            int storedVersion = metadataDto.saveVersion;
            if (storedVersion > _saveVersion)
            {
                CustomLogger.LogWarning($"Slot '{slotId}' was saved at version {storedVersion}, "
                    + $"newer than supported version {_saveVersion}.", null);

                return ESaveLoadResult.VersionTooNew;
            }

            Dictionary<string, string> states = blob.ToLookup();

            if (storedVersion < _saveVersion && !TryMigrate(slotId, states, storedVersion))
                return ESaveLoadResult.Corrupt;

            foreach (ISavable savable in _registry.GetOrdered())
                savable.Deserialize(states.GetValueOrDefault(savable.PersistentKey.Value));

            return ESaveLoadResult.Success;
        }

        /// <inheritdoc/>
        public async Awaitable<bool> ExistsAsync(string slotId, CancellationToken ct = default)
            => await _storage.ExistsAsync(MetaKey(slotId), ct);

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Awaitable<SaveMetadata> LoadMetadataAsync(string slotId, CancellationToken ct = default)
        {
            byte[] bytes = await _storage.ReadAsync(MetaKey(slotId), ct);
            if (bytes == null)
                return null;

            try
            {
                return _codec.Decode<SaveMetadataDto>(bytes).ToDomain();
            }
            catch (Exception exception)
            {
                CustomLogger.LogWarning($"Failed to decode metadata for slot '{slotId}': {exception.Message}", null);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Awaitable<byte[]> LoadScreenshotPngAsync(string slotId, CancellationToken ct = default)
            => await _storage.ReadAsync(ShotKey(slotId), ct);

        /// <inheritdoc/>
        public async Awaitable<IReadOnlyList<SaveMetadata>> ListSavesAsync(CancellationToken ct = default)
        {
            IReadOnlyList<string> keys = await _storage.ListKeysAsync(null, ct);

            List<SaveMetadata> result = new();
            foreach (string key in keys)
            {
                if (!key.EndsWith(MetaSuffix, StringComparison.Ordinal))
                    continue;

                byte[] bytes = await _storage.ReadAsync(key, ct);
                if (bytes == null)
                    continue;

                try
                {
                    result.Add(_codec.Decode<SaveMetadataDto>(bytes).ToDomain());
                }
                catch (Exception)
                {
                    // Skip but name it, so a corrupt save is diagnosable rather than silently gone.
                    CustomLogger.LogWarning($"Skipping unreadable save metadata for key '{key}'.", null);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Awaitable FlushAsync(CancellationToken ct = default)
        {
            await _writeGate.WaitAsync(ct);
            _writeGate.Release();
        }

        private static string DataKey(string slotId) => slotId + DataSuffix;

        private static string MetaKey(string slotId) => slotId + MetaSuffix;

        private static string ShotKey(string slotId) => slotId + ShotSuffix;

        private static bool TryGetScreenshot(SaveRequest request, out ScreenshotData screenshot)
        {
            screenshot = request.Screenshot ?? default(ScreenshotData);
            return request.Screenshot.HasValue && screenshot.IsValid;
        }

        private SaveMetadata BuildMetadata(SaveMetadata existing, SaveRequest request)
        {
            DateTime nowUtc = DateTime.UtcNow;

            SaveMetadata metadata = existing
                ?? SaveMetadata.CreateNew(request.SlotId, _saveVersion, Application.version, nowUtc);

            metadata = metadata.With(request.DisplayName,
                _saveVersion,
                Application.version,
                nowUtc,
                request.PlaytimeSeconds.HasValue
                    ? TimeSpan.FromSeconds(request.PlaytimeSeconds.Value)
                    : null);

            if (!TryGetScreenshot(request, out ScreenshotData screenshot))
                return metadata;

            return metadata.With(hasScreenshot: true,
                screenshotWidth: screenshot.Width,
                screenshotHeight: screenshot.Height);
        }

        private bool TryMigrate(string slotId, IDictionary<string, string> states, int fromVersion)
        {
            try
            {
                for (int version = fromVersion; version < _saveVersion; version++)
                {
                    if (!_migrations.TryGetValue(version, out ISaveMigration step))
                    {
                        CustomLogger.LogError($"No migration from version {version} for slot '{slotId}'. "
                            + "Cannot upgrade save.", null);

                        return false;
                    }

                    step.Migrate(states);
                }

                return true;
            }
            catch (Exception exception)
            {
                CustomLogger.LogError($"Migration failed for slot '{slotId}': {exception.Message}", null);
                return false;
            }
        }
    }
}