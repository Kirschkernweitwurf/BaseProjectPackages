using System;
using System.Collections.Generic;
using Base.AttributesPackage;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// All settings related to scanning a project folder and turning matching assets into categories.
    /// Expected naming: Prefix, Separator, Group, Separator, Name. For example "P_Garden_Rock_01"
    /// and "SM_Garden_Rock_01" both land in the group "Garden". Prefixes are recognized on their own,
    /// the list below is only for the ones detection cannot be sure about.
    /// </summary>
    [Serializable]
    internal class AutoGenerateSettings
    {
        /// <summary>
        /// Separator used when none is set.
        /// </summary>
        public const string DefaultSeparator = "_";
        /// <summary>
        /// Depth value that disables the depth limit.
        /// </summary>
        public const int UnlimitedDepth = -1;
        /// <summary>
        /// Longest a name part may be to still be considered a prefix by detection.
        /// </summary>
        private const int DefaultMaxPrefixLength = 4;
        /// <summary>
        /// Number of assets a token has to appear on before detection accepts it as a prefix.
        /// </summary>
        private const int DefaultMinPrefixOccurrences = 2;
        /// <summary>
        /// Smallest value the prefix detection limits accept.
        /// </summary>
        private const int MinDetectionLimit = 1;

        [field: Tooltip("Folder to scan. Subfolders are included up to the search depth.")]
        [field: FolderPath]
        [field: SerializeField] public string SearchFolder { get; private set; } = "Assets";

        [field: Tooltip("Prefixes that are always stripped instead of becoming a group. Only needed "
            + "for prefixes a single asset carries, detection cannot spot those on its own.")]
        [field: SerializeField] public List<string> Prefixes { get; private set; } = new()
        {
            "P",
            "SM"
        };

        [field: Tooltip("Also recognize prefixes that are not listed above, based on their shape and "
            + "how many assets share them. Off = only the list above counts as a prefix.")]
        [field: SerializeField] public bool AutoDetectPrefixes { get; private set; } = true;

        [field: Tooltip("How many assets have to share a name part before detection calls it a prefix.")]
        [field: Min(MinDetectionLimit)]
        [field: SerializeField] public int MinPrefixOccurrences { get; private set; } = DefaultMinPrefixOccurrences;

        [field: Tooltip("Longest a name part may be to still be detected as a prefix.")]
        [field: Min(MinDetectionLimit)]
        [field: SerializeField] public int MaxPrefixLength { get; private set; } = DefaultMaxPrefixLength;

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