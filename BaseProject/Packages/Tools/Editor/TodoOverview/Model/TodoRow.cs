namespace Base.ToolsPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// One drawn line of the list, either a section header or an item inside a section. Headers and
    /// items are flattened into a single list of equally tall rows, which is what lets the window draw
    /// only the rows the scroll view actually shows.
    /// </summary>
    internal readonly struct TodoRow
    {
        private const int HeaderIndex = -1;

        /// <summary>Index of the section this row belongs to.</summary>
        internal int Group { get; }

        /// <summary>Index of the item inside the section, or -1 when this row is the header.</summary>
        internal int Entry { get; }

        /// <summary>Whether this row is a section header rather than an item.</summary>
        internal bool IsHeader => Entry == HeaderIndex;

        /// <summary>Creates a row.</summary>
        /// <param name="group">Index of the section.</param>
        /// <param name="entry">Index of the item, or -1 for the header.</param>
        internal TodoRow(int group, int entry)
        {
            Group = group;
            Entry = entry;
        }

        /// <summary>Creates the header row of a section.</summary>
        /// <param name="group">Index of the section.</param>
        /// <returns>The header row.</returns>
        internal static TodoRow Header(int group) => new(group, HeaderIndex);
    }
}