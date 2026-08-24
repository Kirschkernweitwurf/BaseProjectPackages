using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Storage;
using UnityEngine;

namespace Base.SaveSystemPackage.Backup
{
    /// <summary>
    /// Keeps a number of previous versions of each slot around, so a save that turns out to be
    /// unreadable is not the end of a playthrough.
    /// <para>
    /// A backup is taken from the files that are already on disk, right before they are overwritten,
    /// which is the only moment a known-good copy is guaranteed to exist.
    /// </para>
    /// </summary>
    public interface ISaveBackups
    {
        /// <summary>False when no generations are kept, so callers can skip the work entirely.</summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Copies the slot's current save aside as a new generation and prunes the oldest beyond the
        /// configured count. Does nothing when the slot holds no completed save yet.
        /// </summary>
        Awaitable RotateAsync(string slotId, CancellationToken ct = default);

        /// <summary>Every complete backup of a slot, newest first.</summary>
        Awaitable<IReadOnlyList<SaveBackupInfo>> ListAsync(string slotId, CancellationToken ct = default);

        /// <summary>
        /// The raw bytes of one file of a backup, or <c>null</c> when it does not exist. Still encoded,
        /// so the caller decides what to do with them.
        /// </summary>
        Awaitable<byte[]> ReadAsync(string slotId, string backupId, ESaveFile file, CancellationToken ct = default);

        /// <summary>
        /// Copies a backup back over the live save. Wait for any in-flight write to finish first, since
        /// this writes the same files a save does.
        /// </summary>
        /// <returns>True when the live save was replaced.</returns>
        Awaitable<bool> RestoreAsync(string slotId, string backupId, CancellationToken ct = default);

        /// <summary>Removes every backup of a slot, for example when the slot itself is deleted.</summary>
        Awaitable DeleteAllAsync(string slotId, CancellationToken ct = default);
    }
}