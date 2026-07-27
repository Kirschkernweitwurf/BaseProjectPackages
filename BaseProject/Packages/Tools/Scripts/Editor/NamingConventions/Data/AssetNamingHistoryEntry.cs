using System;

namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>One thing the tool did. Serialized into the history file, so fields are public.</summary>
    [Serializable]
    public sealed class AssetNamingHistoryEntry
    {
        /// <summary>What happened to the asset.</summary>
        public EAssetNamingAction action;

        /// <summary>File name before the action, without the extension.</summary>
        public string oldName;

        /// <summary>File name after a rename, without the extension. Empty for other actions.</summary>
        public string newName;

        /// <summary>Project relative path of the asset after the action.</summary>
        public string assetPath;

        /// <summary>When it happened, already formatted in the local culture.</summary>
        public string time;
    }
}