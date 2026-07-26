using System;

namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>One rename the tool applied. Serialized into the history file, so fields are public.</summary>
    [Serializable]
    public sealed class AssetNamingHistoryEntry
    {
        /// <summary>File name before the rename, without the extension.</summary>
        public string oldName;

        /// <summary>File name after the rename, without the extension.</summary>
        public string newName;

        /// <summary>Project relative path of the asset after the rename.</summary>
        public string assetPath;

        /// <summary>When the rename happened, formatted for display.</summary>
        public string time;
    }
}
