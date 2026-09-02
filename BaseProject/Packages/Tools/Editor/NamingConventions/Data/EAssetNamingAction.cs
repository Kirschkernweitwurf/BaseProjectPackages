namespace Base.ToolsPackage.Editor.NamingConventions.Data
{
    /// <summary>What the tool did to an asset, remembered in the history.</summary>
    internal enum EAssetNamingAction : byte
    {
        /// <summary>The asset was renamed.</summary>
        Renamed = 0,

        /// <summary>The asset was taken out of the scan.</summary>
        Dismissed = 1,

        /// <summary>The asset was brought back into the scan.</summary>
        Restored = 2
    }
}