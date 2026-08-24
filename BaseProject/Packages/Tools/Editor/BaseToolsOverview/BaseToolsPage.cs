namespace Base.ToolPackage.Editor.BaseToolsOverview
{
    /// <summary>
    /// One settings page listed on the Base Tools overview.
    /// </summary>
    internal readonly struct BaseToolsPage
    {
        /// <summary>The name the page carries in the settings tree.</summary>
        internal string Label { get; }

        /// <summary>The full settings path the page is opened by.</summary>
        internal string Path { get; }

        /// <summary>What the page is for, or an empty string when it says nothing about itself.</summary>
        internal string Summary { get; }

        /// <summary>Creates the entry for a single settings page.</summary>
        /// <param name="label">The name the page carries in the settings tree.</param>
        /// <param name="path">The full settings path the page is opened by.</param>
        /// <param name="summary">What the page is for.</param>
        internal BaseToolsPage(string label, string path, string summary)
        {
            Label = label;
            Path = path;
            Summary = summary;
        }
    }
}