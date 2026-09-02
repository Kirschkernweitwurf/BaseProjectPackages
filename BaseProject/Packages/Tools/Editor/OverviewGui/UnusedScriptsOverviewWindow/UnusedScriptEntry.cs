namespace Base.ToolsPackage.Editor.OverviewGui.UnusedScriptsOverviewWindow
{
    /// <summary>
    /// One script that looks dead, with its folder for grouping and GUID for stable reference.
    /// </summary>
    internal sealed class UnusedScriptEntry
    {
        /// <summary>Asset path, for example "Assets/Runtime/OldThing.cs".</summary>
        internal string Path { get; }

        /// <summary>Stable GUID.</summary>
        internal string Guid { get; }

        /// <summary>Containing folder, used to group the list.</summary>
        internal string Folder { get; }

        /// <summary>File name of the script, which is what the list shows.</summary>
        internal string Name => System.IO.Path.GetFileName(Path);

        /// <summary>Creates an entry for one script that nothing appears to reference.</summary>
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