using System;
using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Backup;
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
    /// <para>
    /// A save that cannot be read is not the end of the road: the previous versions kept by
    /// <see cref="ISaveBackups"/> are tried in turn, and a load that had to fall back reports
    /// <see cref="ESaveLoadResult.RecoveredFromBackup"/> so the game can say so. Metadata falls back
    /// the same way, so a slot whose marker went bad still shows up in a menu instead of quietly
    /// disappearing from one while remaining perfectly loadable.
    /// </para>
    /// </summary>
    public sealed class SaveSystem : ISaveSystem
    {
        private readonly int _saveVersion;
        private readonly ISaveCodec _codec;
        private readonly ISaveStorage _storage;
        private readonly ISavableRegistry _registry;
        private readonly ISaveBackups _backups;
        private readonly SaveMigrationChain _migrations;
        private readonly SemaphoreSlim _writeGate = new(1, 1);

        /// <param name="storage">Where the raw bytes live. Swap this layer for a console save API.</param>
        /// <param name="codec">Turns objects into bytes, including the header and encryption.</param>
        /// <param name="registry">The savables to collect state from and hand state back to.</param>
        /// <param name="saveVersion">The schema version written into new saves.</param>
        /// <param name="migrations">Steps that upgrade an older save one version at a time.</param>
        /// <param name="backups">
        /// Keeps previous versions of each slot. Null means no backups are kept and a damaged save
        /// cannot be recovered from one.
        /// </param>
        /// <exception cref="ArgumentNullException">When storage, codec or registry is null.</exception>
        public SaveSystem(ISaveStorage storage, ISaveCodec codec, ISavableRegistry registry, int saveVersion = 1,
            IReadOnlyList<ISaveMigration> migrations = null, ISaveBackups backups = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _saveVersion = saveVersion;
            _backups = backups ?? NoSaveBackups.Instance;

            _migrations = new SaveMigrationChain(migrations);
            _migrations.Validate(saveVersion);
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

            string slotId = request.SlotId;

            await _writeGate.WaitAsync(ct);
            try
            {
                await Awaitable.MainThreadAsync();

                SaveBlob blob = new();
                foreach (ISavable savable in _registry.GetOrdered())
                    blob.Add(savable.PersistentKey.Value, savable.Serialize() ?? string.Empty);

                SaveMetadata metadata = BuildMetadata(await LoadMetadataAsync(slotId, ct), request);
                SaveMetadataDto metadataDto = SaveMetadataDto.From(metadata);

                // Encode (serialize and encrypt) off the main thread; it is pure CPU work.
                await Awaitable.BackgroundThreadAsync();
                byte[] dataBytes = _codec.Encode(blob);
                byte[] metaBytes = _codec.Encode(metadataDto);
                await Awaitable.MainThreadAsync();

                // The last moment the previous save is still on disk in one piece.
                await _backups.RotateAsync(slotId, ct);

                await _storage.WriteAsync(SaveKeys.Key(slotId, ESaveFile.Data), dataBytes, ct);

                if (TryGetScreenshot(request, out ScreenshotData screenshot))
                    await _storage.WriteAsync(SaveKeys.Key(slotId, ESaveFile.Screenshot), screenshot.Png, ct);

                // The metadata is the commit marker, so it has to be the last thing written.
                await _storage.WriteAsync(SaveKeys.Key(slotId, ESaveFile.Meta), metaBytes, ct);
            }
            finally
            {
                _writeGate.Release();
            }
        }

        /// <inheritdoc/>
        public async Awaitable<ESaveLoadResult> LoadAsync(string slotId, CancellationToken ct = default)
        {
            DecodedSave save = await ReadSaveAsync(SaveKeys.Key(slotId, ESaveFile.Meta),
                SaveKeys.Key(slotId, ESaveFile.Data), Describe(slotId), ct);

            bool recovered = false;

            if (!save.IsComplete)
            {
                DecodedSave fromBackup = await ReadNewestBackupAsync(slotId, ct);

                if (fromBackup.IsComplete)
                {
                    CustomLogger.LogWarning($"{Describe(slotId)} could not be read and was loaded from a backup "
                        + "instead. The live save is still on disk and is replaced by the next save.", null);

                    save = fromBackup;
                    recovered = true;
                }
            }

            if (!save.IsComplete)
                return save.Result;

            int storedVersion = save.Metadata.saveVersion;

            if (storedVersion > _saveVersion)
            {
                CustomLogger.LogWarning($"{Describe(slotId)} was saved at version {storedVersion}, "
                    + $"newer than supported version {_saveVersion}.", null);

                return ESaveLoadResult.VersionTooNew;
            }

            Dictionary<string, string> states = save.Blob.ToLookup();

            if (storedVersion < _saveVersion
                && !_migrations.TryMigrate(slotId, states, storedVersion, _saveVersion))
                return ESaveLoadResult.Corrupt;

            foreach (ISavable savable in _registry.GetOrdered())
                savable.Deserialize(states.GetValueOrDefault(savable.PersistentKey.Value));

            return recovered
                ? ESaveLoadResult.RecoveredFromBackup
                : ESaveLoadResult.Success;
        }

        /// <inheritdoc/>
        public async Awaitable<bool> ExistsAsync(string slotId, CancellationToken ct = default)
            => await _storage.ExistsAsync(SaveKeys.Key(slotId, ESaveFile.Meta), ct);

        /// <inheritdoc/>
        public async Awaitable DeleteAsync(string slotId, CancellationToken ct = default)
        {
            await _writeGate.WaitAsync(ct);
            try
            {
                await Awaitable.MainThreadAsync();

                await _storage.DeleteAsync(SaveKeys.Key(slotId, ESaveFile.Meta), ct);
                await _storage.DeleteAsync(SaveKeys.Key(slotId, ESaveFile.Data), ct);
                await _storage.DeleteAsync(SaveKeys.Key(slotId, ESaveFile.Screenshot), ct);

                // Otherwise the next load of a freshly deleted slot would resurrect it from a backup.
                await _backups.DeleteAllAsync(slotId, ct);
            }
            finally
            {
                _writeGate.Release();
            }
        }

        /// <inheritdoc/>
        public async Awaitable<SaveMetadata> LoadMetadataAsync(string slotId, CancellationToken ct = default)
        {
            byte[] bytes = await _storage.ReadAsync(SaveKeys.Key(slotId, ESaveFile.Meta), ct);

            // No marker means no save, so there is nothing to fall back for. Checked before the
            // backups are listed, since an empty fixed slot asks this question on every menu open.
            if (bytes == null)
                return null;

            if (TryDecodeMetadata(bytes, Describe(slotId), out SaveMetadata metadata))
                return metadata;

            // The slot does hold a save, its marker just cannot be read. Falling back keeps the slot
            // visible in a menu, which matters because a load recovers it from the very same backups.
            return await ReadNewestBackupMetadataAsync(slotId, ct);
        }

        /// <inheritdoc/>
        public async Awaitable<byte[]> LoadScreenshotPngAsync(string slotId, CancellationToken ct = default)
            => await _storage.ReadAsync(SaveKeys.Key(slotId, ESaveFile.Screenshot), ct);

        /// <inheritdoc/>
        public async Awaitable<IReadOnlyList<SaveMetadata>> ListSavesAsync(CancellationToken ct = default)
        {
            IReadOnlyList<string> keys = await _storage.ListKeysAsync(null, ct);

            List<SaveMetadata> result = new();
            foreach (string key in keys)
            {
                // Backups sit under the slot they belong to and carry a marker of their own, so a bare
                // suffix match would list every kept generation as a save in its own right.
                if (!SaveKeys.TryGetLiveSlotId(key, out string slotId))
                    continue;

                SaveMetadata metadata = await LoadMetadataAsync(slotId, ct);

                if (metadata != null)
                    result.Add(metadata);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Awaitable FlushAsync(CancellationToken ct = default)
        {
            await _writeGate.WaitAsync(ct);
            _writeGate.Release();
        }

        private static string Describe(string slotId) => $"Slot '{slotId}'";

        private static string Describe(string slotId, string backupId) => $"Backup '{backupId}' of slot '{slotId}'";

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

        private bool TryDecodeMetadata(byte[] bytes, string description, out SaveMetadata metadata)
        {
            metadata = null;

            try
            {
                metadata = _codec.Decode<SaveMetadataDto>(bytes)?.ToDomain();
            }
            catch (Exception exception)
            {
                CustomLogger.LogWarning($"{description} has metadata that cannot be read: {exception.Message}",
                    null);

                return false;
            }

            return metadata != null;
        }

        // Reads and decodes one metadata and data pair. Everything that can go wrong with a file on
        // disk ends here, so the live save and every backup are judged by exactly the same rules.
        private async Awaitable<DecodedSave> ReadSaveAsync(string metaKey, string dataKey, string description,
            CancellationToken ct)
        {
            byte[] metaBytes = await _storage.ReadAsync(metaKey, ct);
            if (metaBytes == null)
                return DecodedSave.Failed(ESaveLoadResult.NotFound);

            byte[] dataBytes = await _storage.ReadAsync(dataKey, ct);
            if (dataBytes == null)
            {
                CustomLogger.LogWarning($"{description} has metadata but no data; treating as corrupt.", null);
                return DecodedSave.Failed(ESaveLoadResult.Corrupt);
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
                CustomLogger.LogWarning($"{description} could not be decoded: {decodeError.Message}", null);
                return DecodedSave.Failed(ESaveLoadResult.Corrupt);
            }

            // An empty file decodes without throwing and yields nothing, which is still not a save.
            if (metadataDto == null || blob == null)
            {
                CustomLogger.LogWarning($"{description} decoded to nothing; treating as corrupt.", null);
                return DecodedSave.Failed(ESaveLoadResult.Corrupt);
            }

            return new DecodedSave(metadataDto, blob);
        }

        private async Awaitable<DecodedSave> ReadNewestBackupAsync(string slotId, CancellationToken ct)
        {
            IReadOnlyList<SaveBackupInfo> backups = await _backups.ListAsync(slotId, ct);

            foreach (SaveBackupInfo backup in backups)
            {
                DecodedSave save = await ReadSaveAsync(SaveKeys.BackupKey(slotId, backup.Id, ESaveFile.Meta),
                    SaveKeys.BackupKey(slotId, backup.Id, ESaveFile.Data), Describe(slotId, backup.Id), ct);

                if (save.IsComplete)
                    return save;
            }

            return DecodedSave.Failed(ESaveLoadResult.NotFound);
        }

        private async Awaitable<SaveMetadata> ReadNewestBackupMetadataAsync(string slotId, CancellationToken ct)
        {
            IReadOnlyList<SaveBackupInfo> backups = await _backups.ListAsync(slotId, ct);

            foreach (SaveBackupInfo backup in backups)
            {
                byte[] bytes = await _backups.ReadAsync(slotId, backup.Id, ESaveFile.Meta, ct);

                if (bytes != null && TryDecodeMetadata(bytes, Describe(slotId, backup.Id), out SaveMetadata found))
                    return found;
            }

            return null;
        }
    }
}