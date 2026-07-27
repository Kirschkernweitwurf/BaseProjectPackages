using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// One naming rule for a group of assets. The name check itself is reused from
    /// <see cref="NamingRule"/>, so assets and code share the same casing, prefix and pattern
    /// logic. This type adds the filters that decide which assets the rule is responsible for and
    /// the digit count for enumerated names like "Rock_01".
    /// </summary>
    [Serializable]
    public sealed class AssetNamingRule
    {
        [Tooltip("Shown in the rule table and in the scan results.")]
        [field: SerializeField] public string Label { get; set; } = "New Rule";

        [Tooltip("Turns the rule off without deleting it.")]
        [field: SerializeField] public bool Enabled { get; set; } = true;

        [Tooltip("A kind like Prefab, or a full type name like UnityEngine.Texture2D. "
            + "Empty means every asset.")]
        [field: SerializeField] public string TypeName { get; set; } = string.Empty;

        [Tooltip("Only check assets whose path contains this text, for example /Art/. "
            + "Empty checks everywhere.")]
        [field: SerializeField] public string PathFilter { get; set; } = string.Empty;

        [Tooltip("Casing, prefixes, suffixes and pattern the file name has to follow.")]
        [field: SerializeField] public NamingRule Naming { get; private set; } = new();

        [Tooltip("Length of the number at the end of a name: 2 means _01, 3 means _001. "
            + "0 allows any length. Numbers are always recognized and kept.")]
        [field: SerializeField] public int EnumerationDigits { get; set; }

        /// <summary>Creates an empty rule. Needed by the serializer.</summary>
        public AssetNamingRule() { }

        /// <summary>Creates a rule for one asset kind.</summary>
        public AssetNamingRule(string label, string typeName, ENamingStyle style)
        {
            Label = label;
            TypeName = typeName;
            Naming.Style = style;
        }

        /// <summary>True when this rule is responsible for the given asset.</summary>
        public bool AppliesTo(string assetPath, string assetKind, Type assetType)
        {
            if (!Enabled)
                return false;

            if (string.IsNullOrEmpty(assetPath))
                return false;

            if (PathFilter.Length > 0
                && !assetPath.Contains(PathFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (TypeName.Length == 0)
                return true;

            // Kinds like Prefab or Model are resolved from the file extension, because their main
            // asset type is a plain GameObject and would otherwise be indistinguishable.
            if (TypeName == assetKind)
                return true;

            return HasMatchingType(assetType);
        }

        /// <summary>True when the type or one of its base types is the type of this rule.</summary>
        private bool HasMatchingType(Type assetType)
        {
            Type current = assetType;

            while (current != null)
            {
                if (current.FullName == TypeName)
                    return true;

                current = current.BaseType;
            }

            return false;
        }
    }
}