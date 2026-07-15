#if UNITY_EDITOR
namespace Base.ToolPackage.Editor.AssetZoo.Generation
{
    /// <summary>
    /// Outcome of a single auto-generation run. Used to give the artist feedback in the UI.
    /// </summary>
    public readonly struct ZooGenerationResult
    {
        /// <summary>
        /// True when the scan ran and produced at least one category.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Number of categories in the config after the run.
        /// </summary>
        public int CategoryCount { get; }

        /// <summary>
        /// Number of entries added by this run.
        /// </summary>
        public int EntryCount { get; }

        /// <summary>
        /// Human readable summary or error text.
        /// </summary>
        public string Message { get; }

        public ZooGenerationResult(bool success, int categoryCount, int entryCount, string message)
        {
            Success = success;
            CategoryCount = categoryCount;
            EntryCount = entryCount;
            Message = message;
        }

        /// <summary>
        /// Creates a failed result with the given reason.
        /// </summary>
        public static ZooGenerationResult Failed(string message) => new(false, 0, 0, message);
    }
}
#endif
