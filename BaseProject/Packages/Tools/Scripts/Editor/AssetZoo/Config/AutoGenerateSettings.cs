using System;
using System.Collections.Generic;
using Base.AttributePackage;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// All settings related to scanning a project folder and turning matching assets into categories.
    /// Expected naming: Prefix, Separator, Group, Separator, Name. For example "P_Garden_Rock_01"
    /// and "SM_Garden_Rock_01" both land in the group "Garden".
    /// </summary>
    [Serializable]
    public class AutoGenerateSettings
    {
        /// <summary>
        /// Depth value that disables the depth limit.
        /// </summary>
        public const int UnlimitedDepth = -1;

        /// <summary>
        /// Separator used when none is set.
        /// </summary>
        public const string DefaultSeparator = "_";

        [field: Tooltip("Folder to scan. Subfolders are included up to the search depth.")]
        [field: FolderPath]
        [field: SerializeField] public string SearchFolder { get; private set; } = "Assets";

        [field: Tooltip("Name prefixes to scan for. The first word after the prefix becomes the group.")]
        [field: SerializeField] public List<string> Prefixes { get; private set; } = new() { "P", "SM" };

        [field: Tooltip("Separator between the name parts.")]
        [field: SerializeField] public string Separator { get; private set; } = DefaultSeparator;

        [field: Tooltip("Subfolder levels to scan. 0 = search folder only, -1 = unlimited.")]
        [field: Min(UnlimitedDepth)]
        [field: SerializeField] public int SearchDepth { get; private set; } = UnlimitedDepth;

        [field: Tooltip("Ignore casing when matching prefixes.")]
        [field: SerializeField] public bool IgnorePrefixCase { get; private set; } = true;

        [field: Tooltip("Keep existing categories and only add new assets. Off = replace all categories.")]
        [field: SerializeField] public bool MergeWithExisting { get; private set; }

        [field: Tooltip("Give every generated category its own label color, derived from its name.")]
        [field: SerializeField] public bool ColorizeCategories { get; private set; } = true;
    }
}
