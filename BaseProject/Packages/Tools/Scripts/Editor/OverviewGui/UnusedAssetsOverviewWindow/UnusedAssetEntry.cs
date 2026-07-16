namespace Base.ToolPackage.Editor.OverviewGui.UnusedAssetsOverviewWindow
{
    /// <summary>
    /// One asset that looks unreferenced, with its type and size for display.
    /// </summary>
    public sealed class UnusedAssetEntry
    {
        /// <summary>Asset path, for example "Assets/Art/Unused.png".</summary>
        public string Path { get; }

        /// <summary>Stable GUID, used to remember dismissals across moves and rescans.</summary>
        public string Guid { get; }

        /// <summary>Main asset type name, used to group the list.</summary>
        public string TypeName { get; }

        /// <summary>File size in bytes.</summary>
        public long SizeBytes { get; }

        public string Name => System.IO.Path.GetFileName(Path);

        public UnusedAssetEntry(string path, string guid, string typeName, long sizeBytes)
        {
            Path = path;
            Guid = guid;
            TypeName = typeName;
            SizeBytes = sizeBytes;
        }
    }
}