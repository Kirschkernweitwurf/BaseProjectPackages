using System;
using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Storage;
using Base.UtilityPackage.Async;
using UnityEngine;

namespace Base.SaveSystemPackage.Backup
{
    /// <summary>
    /// The do-nothing <see cref="ISaveBackups"/> used when a save system is built without one, so the
    /// save path can call it unconditionally instead of null checking at four call sites.
    /// </summary>
    internal sealed class NoSaveBackups : ISaveBackups
    {
        /// <summary>The shared instance. It holds no state, so one is enough.</summary>
        internal static readonly NoSaveBackups Instance = new();

        /// <inheritdoc/>
        public bool IsEnabled => false;

        private NoSaveBackups() { }

        /// <inheritdoc/>
        public Awaitable RotateAsync(string slotId, CancellationToken ct = default)
            => AwaitableUtility.Completed();

        /// <inheritdoc/>
        public Awaitable<IReadOnlyList<SaveBackupInfo>> ListAsync(string slotId, CancellationToken ct = default)
            => AwaitableUtility.FromResult<IReadOnlyList<SaveBackupInfo>>(Array.Empty<SaveBackupInfo>());

        /// <inheritdoc/>
        public Awaitable<byte[]> ReadAsync(string slotId, string backupId, ESaveFile file,
            CancellationToken ct = default)
            => AwaitableUtility.FromResult<byte[]>(null);

        /// <inheritdoc/>
        public Awaitable<bool> RestoreAsync(string slotId, string backupId, CancellationToken ct = default)
            => AwaitableUtility.FromResult(false);

        /// <inheritdoc/>
        public Awaitable DeleteAllAsync(string slotId, CancellationToken ct = default)
            => AwaitableUtility.Completed();
    }
}