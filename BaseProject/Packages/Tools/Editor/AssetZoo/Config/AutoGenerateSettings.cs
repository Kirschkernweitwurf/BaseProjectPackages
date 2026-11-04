using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// Where the art comes from and how it is sorted into groups.
    /// </summary>
    [Serializable]
    internal class AutoGenerateSettings : ISerializationCallbackReceiver
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

        // Configs from before several folders were possible hold their one folder here.
        [FormerlySerializedAs("<SearchFolder>k__BackingField")]
        [HideInInspector]
        [SerializeField] private string legacySearchFolder;

        [field: Tooltip("The folders, prefabs and models the zoo shows. Subfolders are included.")]
        [field: SerializeField] public List<string> Sources { get; private set; } = new();

        [field: Tooltip("What decides which group an asset lands in.")]
        [field: SerializeField] public EGroupBy GroupBy { get; private set; } = EGroupBy.Folder;

        [field: Tooltip("Also show a model when a prefab already shows it. Off keeps every piece of art once.")]
        [field: SerializeField] public bool HasModelsWithPrefab { get; private set; }

        [field: Tooltip("Subfolder levels to scan. 0 = the folder only, -1 = all of them.")]
        [field: Min(UnlimitedDepth)]
        [field: SerializeField] public int SearchDepth { get; private set; } = UnlimitedDepth;

        [field: Tooltip("Prefixes that are always stripped instead of becoming a group. Only needed "
            + "for prefixes a single asset carries, detection cannot spot those on its own.")]
        [field: SerializeField] public List<string> Prefixes { get; private set; } = new()
        {
            "P",
            "SM"
        };

        [field: Tooltip("Also recognize prefixes that are not listed above, based on their shape and "
            + "how many assets share them. Off = only the list above counts as a prefix.")]
        [field: FormerlySerializedAs("<AutoDetectPrefixes>k__BackingField")]
        [field: SerializeField] public bool CanDetectPrefixes { get; private set; } = true;

        [field: Tooltip("How many assets have to share a name part before detection calls it a prefix.")]
        [field: Min(MinDetectionLimit)]
        [field: SerializeField] public int MinPrefixOccurrences { get; private set; } = DefaultMinPrefixOccurrences;

        [field: Tooltip("Longest a name part may be to still be detected as a prefix.")]
        [field: Min(MinDetectionLimit)]
        [field: SerializeField] public int MaxPrefixLength { get; private set; } = DefaultMaxPrefixLength;

        [field: Tooltip("Separator between the name parts.")]
        [field: SerializeField] public string Separator { get; private set; } = DefaultSeparator;

        [field: Tooltip("Ignore casing when matching prefixes.")]
        [field: FormerlySerializedAs("<IgnorePrefixCase>k__BackingField")]
        [field: SerializeField] public bool IsPrefixCaseIgnored { get; private set; } = true;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(legacySearchFolder))
                return;

            Sources ??= new List<string>();

            if (Sources.Count == 0)
                Sources.Add(legacySearchFolder);

            legacySearchFolder = null;
        }
    }
}