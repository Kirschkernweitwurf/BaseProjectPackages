namespace Base.ToolPackage.Editor.OverviewGui.UnusedAssetsOverviewWindow
{
    /// <summary>
    /// One asset that looks unreferenced, with its type and size for display.
    /// </summary>
    internal sealed class UnusedAssetEntry
    {
        /// <summary>Asset path, for example "Assets/Art/Unused.png".</summary>
        internal string Path { get; }

        /// <summary>Stable GUID, used to remember dismissals across moves and rescans.</summary>
        internal string Guid { get; }

        /// <summary>Main asset type name, used to group the list.</summary>
        internal string TypeName { get; }

        /// <summary>File size in bytes.</summary>
        internal long SizeBytes { get; }

        /// <summary>Creates an entry for one asset that nothing appears to reference.</summary>
        public UnusedAssetEntry(string path, string guid, string typeName, long sizeBytes)
        {
            Path = path;
            Guid = guid;
            TypeName = typeName;
            SizeBytes = sizeBytes;
        }
    }
}