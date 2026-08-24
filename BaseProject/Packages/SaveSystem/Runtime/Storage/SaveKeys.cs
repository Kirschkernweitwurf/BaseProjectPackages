using System;
using System.Globalization;

namespace Base.SaveSystemPackage.Storage
{
    /// <summary>
    /// Builds and reads back the storage keys of a save. A live save is <c>slot/file</c>; a backup
    /// puts a timestamped folder in between, <c>slot/Backup_{ticks}/file</c>.
    /// <para>
    /// The spelling lives here rather than in the save system because two layers depend on it: one
    /// writes the keys, the other has to tell a live save apart from a backup of one while walking a
    /// flat key listing.
    /// </para>
    /// </summary>
    public static class SaveKeys
    {
        /// <summary>Marks the folder holding one backup generation inside a slot folder.</summary>
        public const string BackupFolderPrefix = "Backup_";

        private const string BackupSegment = Separator + BackupFolderPrefix;
        private const string DataFileName = "Save.dat";
        private const string MetaFileName = "Meta.dat";
        private const string MetaSuffix = Separator + MetaFileName;
        private const string ScreenshotFileName = "Screenshot.png";
        private const string Separator = "/";
        private const string TicksFormat = "D19";

        /// <summary>The key of one file of a live save.</summary>
        /// <param name="slotId">The slot the file belongs to.</param>
        /// <param name="file">Which part of the save is wanted.</param>
        /// <returns>The storage key.</returns>
        public static string Key(string slotId, ESaveFile file) => slotId + Separator + FileName(file);

        /// <summary>The key of one file inside a backup generation.</summary>
        /// <param name="slotId">The slot the backup belongs to.</param>
        /// <param name="backupId">The generation, as returned by <see cref="CreateBackupId"/>.</param>
        /// <param name="file">Which part of the save is wanted.</param>
        /// <returns>The storage key.</returns>
        public static string BackupKey(string slotId, string backupId, ESaveFile file)
            => slotId + BackupSegment + backupId + Separator + FileName(file);

        /// <summary>The prefix matching every backup key of a slot, for a storage listing.</summary>
        /// <param name="slotId">The slot to list the backups of.</param>
        /// <returns>The key prefix.</returns>
        public static string BackupPrefix(string slotId) => slotId + BackupSegment;

        /// <summary>
        /// A backup id for the given moment. Fixed width so ids sort chronologically as plain text.
        /// </summary>
        /// <param name="utc">The moment the backup is taken.</param>
        /// <returns>The backup id.</returns>
        public static string CreateBackupId(DateTime utc)
            => utc.Ticks.ToString(TicksFormat, CultureInfo.InvariantCulture);

        /// <summary>The moment a backup id stands for.</summary>
        /// <param name="backupId">The id to read.</param>
        /// <returns>
        /// The creation time, or <see cref="DateTime.MinValue"/> when the id is not a timestamp, which
        /// sorts it oldest and lets it be pruned away.
        /// </returns>
        public static DateTime ToCreationUtc(string backupId)
        {
            if (!long.TryParse(backupId, NumberStyles.None, CultureInfo.InvariantCulture, out long ticks)
                || ticks > DateTime.MaxValue.Ticks)
                return DateTime.MinValue;

            return new DateTime(ticks, DateTimeKind.Utc);
        }

        /// <summary>True when the key names the metadata of a save, live or backed up.</summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key ends in the metadata file name.</returns>
        public static bool IsMetaKey(string key) => key != null && key.EndsWith(MetaSuffix, StringComparison.Ordinal);

        /// <summary>
        /// Reads the slot a key belongs to, and only when the key is the commit marker of a live save
        /// rather than of a backup. This is what keeps kept generations out of a save listing, which
        /// walks a flat list of keys and would otherwise report each of them as a save of its own.
        /// </summary>
        /// <param name="key">The key to read.</param>
        /// <param name="slotId">The slot the key belongs to, or empty when it is not a live marker.</param>
        /// <returns>True when the key names the metadata of a live save.</returns>
        public static bool TryGetLiveSlotId(string key, out string slotId)
        {
            slotId = string.Empty;

            if (!IsMetaKey(key) || key.IndexOf(BackupSegment, StringComparison.Ordinal) >= 0)
                return false;

            slotId = key[..^MetaSuffix.Length];

            return slotId.Length > 0;
        }

        /// <summary>Reads which backup generation of a slot a key belongs to.</summary>
        /// <param name="key">The key to read.</param>
        /// <param name="slotId">The slot the key is expected to belong to.</param>
        /// <param name="backupId">The generation the key belongs to, or empty when it is not a backup.</param>
        /// <returns>True when the key names a file of a backup of that slot.</returns>
        public static bool TryGetBackupId(string key, string slotId, out string backupId)
        {
            backupId = string.Empty;

            if (key == null || slotId == null)
                return false;

            string prefix = BackupPrefix(slotId);

            if (!key.StartsWith(prefix, StringComparison.Ordinal))
                return false;

            int end = key.IndexOf(Separator, prefix.Length, StringComparison.Ordinal);

            if (end <= prefix.Length)
                return false;

            backupId = key[prefix.Length..end];

            return true;
        }

        private static string FileName(ESaveFile file) => file switch
        {
            ESaveFile.Data => DataFileName,
            ESaveFile.Meta => MetaFileName,
            ESaveFile.Screenshot => ScreenshotFileName,
            _ => throw new ArgumentOutOfRangeException(nameof(file), file, "Unknown save file.")
        };
    }
}