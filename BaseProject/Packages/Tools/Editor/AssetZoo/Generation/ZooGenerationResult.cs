namespace Base.ToolPackage.Editor.AssetZoo.Generation
{
    /// <summary>
    /// Outcome of a single auto-generation run. Used to give the artist feedback in the UI.
    /// </summary>
    internal readonly struct ZooGenerationResult
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
        /// Human readable summary or error text. Kept to a single short line for the status bar.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// What the scan decided about naming prefixes and what it left out, one item per line.
        /// Empty when there is nothing worth reporting.
        /// </summary>
        public string Details { get; }

        /// <summary>
        /// True when <see cref="Details"/> holds something to show.
        /// </summary>
        public bool HasDetails => !string.IsNullOrEmpty(Details);

        /// <summary>Creates a result describing what a generation run produced.</summary>
        /// <param name="success">Whether the run produced at least one category.</param>
        /// <param name="categoryCount">Number of categories in the config afterwards.</param>
        /// <param name="entryCount">Number of entries this run added.</param>
        /// <param name="message">Single line summary or error text.</param>
        /// <param name="details">Optional multi line report about prefixes and skipped assets.</param>
        public ZooGenerationResult(bool success, int categoryCount, int entryCount, string message,
            string details = null)
        {
            Success = success;
            CategoryCount = categoryCount;
            EntryCount = entryCount;
            Message = message;
            Details = details;
        }

        /// <summary>
        /// Creates a failed result with the given reason.
        /// </summary>
        public static ZooGenerationResult Failed(string message) => new(false, 0, 0, message);
    }
}