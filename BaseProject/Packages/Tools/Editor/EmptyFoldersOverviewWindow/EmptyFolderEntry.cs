namespace Base.ToolsPackage.Editor.EmptyFoldersOverviewWindow
{
    /// <summary>
    /// One empty folder, plus how many folders get removed with it (itself and nested empties).
    /// </summary>
    internal sealed class EmptyFolderEntry
    {
        /// <summary>Asset path of the folder, for example "Assets/Art/Unused".</summary>
        internal string Path { get; }

        /// <summary>Total folders removed when this one is deleted, including nested empties.</summary>
        internal int NestedFolderCount { get; }

        /// <summary>Creates an entry for one empty folder found by the scanner.</summary>
        public EmptyFolderEntry(string path, int nestedFolderCount)
        {
            Path = path;
            NestedFolderCount = nestedFolderCount;
        }
    }
}