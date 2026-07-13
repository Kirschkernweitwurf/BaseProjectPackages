using System.IO;

namespace Base.EmptyFoldersPackage
{
    /// <summary>
    /// One empty folder, plus how many folders get removed with it (itself and nested empties).
    /// </summary>
    public sealed class EmptyFolderEntry
    {
        /// <summary>Asset path of the folder, for example "Assets/Art/Unused".</summary>
        public string Path { get; }

        /// <summary>Total folders removed when this one is deleted, including nested empties.</summary>
        public int NestedFolderCount { get; }

        public EmptyFolderEntry(string path, int nestedFolderCount)
        {
            Path = path;
            NestedFolderCount = nestedFolderCount;
        }

        public string Name => System.IO.Path.GetFileName(Path);
    }
}
