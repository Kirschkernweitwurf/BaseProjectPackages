using System;
using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Storage;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Backup
{
    /// <summary>
    /// Default <see cref="ISaveBackups"/>. Stores each generation in its own timestamped folder next to
    /// the live save, so a rotation copies the current files once instead of shifting every generation
    /// along, and the number of kept generations costs nothing extra per save.
    /// <para>
    /// Built on <see cref="ISaveStorage"/> alone, so it works on whatever layer a platform uses.
    /// </para>
    /// </summary>
    public sealed class SaveBackups : ISaveBackups
    {
        private const int NoGenerations = 0;

        /// <inheritdoc/>
        public bool IsEnabled => _keptGenerations > NoGenerations;

        private readonly ISaveStorage _storage;
        private readonly int _keptGenerations;

        /// <param name="storage">The storage the live saves and the backups share.</param>
        /// <param name="keptGenerations">How many previous saves to keep per slot. Zero turns backups off.</param>
        /// <exception cref="ArgumentNullException">When the storage is null.</exception>
        public SaveBackups(ISaveStorage storage, int keptGenerations)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _keptGenerations = Mathf.Max(NoGenerations, keptGenerations);
        }

        /// <inheritdoc/>
        public async Awaitable RotateAsync(string slotId, CancellationToken ct = default)
        {
            if (!IsEnabled || string.IsNullOrEmpty(slotId))
                return;

            // Read rather than probed: the marker has to be copied anyway, and its absence is the same
            // answer as "this slot holds no completed save yet".
            byte[] metaBytes = await _storage.ReadAsync(SaveKeys.Key(slotId, ESaveFile.Meta), ct);

            if (metaBytes == null)
                return;

            string backupId = SaveKeys.CreateBackupId(DateTime.UtcNow);

            await CopyAsync(slotId, backupId, ESaveFile.Data, ct);
            await CopyAsync(slotId, backupId, ESaveFile.Screenshot, ct);

            // Written last for the same reason a save writes it last: it is what marks the set
            // complete, so a rotation interrupted halfway is skipped rather than trusted.
            await _storage.WriteAsync(SaveKeys.BackupKey(slotId, backupId, ESaveFile.Meta), metaBytes, ct);

            await PruneAsync(slotId, ct);
        }

        /// <inheritdoc/>
        public async Awaitable<IReadOnlyList<SaveBackupInfo>> ListAsync(string slotId,
            CancellationToken ct = default)
        {
            List<SaveBackupInfo> backups = new();

            if (!IsEnabled || string.IsNullOrEmpty(slotId))
                return backups;

            IReadOnlyList<string> keys = await _storage.ListKeysAsync(SaveKeys.BackupPrefix(slotId), ct);

            foreach (string key in keys)
            {
                if (!SaveKeys.IsMetaKey(key) || !SaveKeys.TryGetBackupId(key, slotId, out string backupId))
                    continue;

                backups.Add(new SaveBackupInfo(backupId, SaveKeys.ToCreationUtc(backupId)));
            }

            backups.Sort(comparison: static (first, second) => second.CreatedUtc.CompareTo(first.CreatedUtc));

            return backups;
        }

        /// <inheritdoc/>
        public async Awaitable<byte[]> ReadAsync(string slotId, string backupId, ESaveFile file,
            CancellationToken ct = default)
            => await _storage.ReadAsync(SaveKeys.BackupKey(slotId, backupId, file), ct);

        /// <inheritdoc/>
        public async Awaitable<bool> RestoreAsync(string slotId, string backupId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(slotId) || string.IsNullOrEmpty(backupId))
                return false;

            byte[] metaBytes = await ReadAsync(slotId, backupId, ESaveFile.Meta, ct);
            byte[] dataBytes = await ReadAsync(slotId, backupId, ESaveFile.Data, ct);

            if (metaBytes == null || dataBytes == null)
            {
                CustomLogger.LogWarning($"Backup '{backupId}' of slot '{slotId}' is incomplete and was not "
                    + "restored.", null);

                return false;
            }

            byte[] screenshotBytes = await ReadAsync(slotId, backupId, ESaveFile.Screenshot, ct);

            await _storage.WriteAsync(SaveKeys.Key(slotId, ESaveFile.Data), dataBytes, ct);

            // The live screenshot belongs to the save being replaced, so it goes when there is no
            // backed up one to put in its place.
            if (screenshotBytes != null)
                await _storage.WriteAsync(SaveKeys.Key(slotId, ESaveFile.Screenshot), screenshotBytes, ct);
            else
                await _storage.DeleteAsync(SaveKeys.Key(slotId, ESaveFile.Screenshot), ct);

            await _storage.WriteAsync(SaveKeys.Key(slotId, ESaveFile.Meta), metaBytes, ct);

            return true;
        }

        /// <inheritdoc/>
        public async Awaitable DeleteAllAsync(string slotId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(slotId))
                return;

            // Deliberately not guarded by IsEnabled: turning backups off has to be able to clean up
            // the ones taken while they were on.
            IReadOnlyList<string> keys = await _storage.ListKeysAsync(SaveKeys.BackupPrefix(slotId), ct);

            foreach (string key in keys)
                await _storage.DeleteAsync(key, ct);
        }

        private async Awaitable CopyAsync(string slotId, string backupId, ESaveFile file, CancellationToken ct)
        {
            byte[] bytes = await _storage.ReadAsync(SaveKeys.Key(slotId, file), ct);

            if (bytes == null)
                return;

            await _storage.WriteAsync(SaveKeys.BackupKey(slotId, backupId, file), bytes, ct);
        }

        private async Awaitable PruneAsync(string slotId, CancellationToken ct)
        {
            IReadOnlyList<SaveBackupInfo> backups = await ListAsync(slotId, ct);

            for (int i = _keptGenerations; i < backups.Count; i++)
                await DeleteAsync(slotId, backups[i].Id, ct);
        }

        private async Awaitable DeleteAsync(string slotId, string backupId, CancellationToken ct)
        {
            // Metadata first, so a delete that is interrupted leaves a generation that no longer
            // counts as complete instead of a headless one that still looks readable.
            await _storage.DeleteAsync(SaveKeys.BackupKey(slotId, backupId, ESaveFile.Meta), ct);
            await _storage.DeleteAsync(SaveKeys.BackupKey(slotId, backupId, ESaveFile.Data), ct);
            await _storage.DeleteAsync(SaveKeys.BackupKey(slotId, backupId, ESaveFile.Screenshot), ct);
        }
    }
}