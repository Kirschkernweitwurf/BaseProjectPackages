using System;

namespace Base.SaveSystemPackage.Backup
{
    /// <summary>
    /// One kept generation of a slot: the id its files are stored under and when it was taken.
    /// </summary>
    public readonly struct SaveBackupInfo
    {
        /// <summary>The id the backup's files are stored under.</summary>
        public string Id { get; }

        /// <summary>
        /// When the backup was taken, or <see cref="DateTime.MinValue"/> when the id cannot be read as
        /// a timestamp.
        /// </summary>
        public DateTime CreatedUtc { get; }

        /// <param name="id">The id the backup's files are stored under.</param>
        /// <param name="createdUtc">When the backup was taken.</param>
        public SaveBackupInfo(string id, DateTime createdUtc)
        {
            Id = id;
            CreatedUtc = createdUtc;
        }
    }
}