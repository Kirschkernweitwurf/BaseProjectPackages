namespace Base.ToolPackage.Editor.UnusedScriptsOverviewWindow
{
    /// <summary>
    /// One script that looks dead, with its folder for grouping and GUID for stable reference.
    /// </summary>
    public sealed class UnusedScriptEntry
    {
        /// <summary>Asset path, for example "Assets/Runtime/OldThing.cs".</summary>
        public string Path { get; }

        /// <summary>Stable GUID.</summary>
        public string Guid { get; }

        /// <summary>Containing folder, used to group the list.</summary>
        public string Folder { get; }

        public string Name => System.IO.Path.GetFileName(Path);

        public UnusedScriptEntry(string path, string guid)
        {
            Path = path;
            Guid = guid;

            string folder = System.IO.Path.GetDirectoryName(path);
            Folder = string.IsNullOrEmpty(folder)
                ? "Assets"
                : folder.Replace('\\', '/');
        }
    }
}