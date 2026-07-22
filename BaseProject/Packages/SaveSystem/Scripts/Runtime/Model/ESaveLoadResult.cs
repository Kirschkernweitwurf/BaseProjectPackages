namespace Base.SaveSystemPackage.Model
{
    /// <summary>
    /// Outcome of a load. One channel for all results.
    /// </summary>
    public enum ESaveLoadResult : byte
    {
        /// <summary>Loaded and handed back to all savables.</summary>
        Success = 0,

        /// <summary>No save exists in this slot.</summary>
        NotFound = 1,

        /// <summary>The save exists but could not be read (bad header, decrypt/parse failure, missing data).</summary>
        Corrupt = 2,

        /// <summary>The save was written by a newer build than this one and has no migration path down.</summary>
        VersionTooNew = 3
    }
}