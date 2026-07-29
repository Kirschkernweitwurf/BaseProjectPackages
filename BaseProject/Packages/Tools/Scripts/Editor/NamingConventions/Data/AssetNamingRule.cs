using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>
    /// One naming rule for a group of assets. The name check itself is reused from
    /// <see cref="NamingRule"/>, so assets and code share the same casing, prefix and pattern
    /// logic. This type adds the filters that decide which assets the rule is responsible for, the
    /// digit count for enumerated names like "Rock_01", and the record of which fields were
    /// changed by hand so the auto detection can refresh the rest without undoing a decision.
    /// </summary>
    [Serializable]
    public sealed class AssetNamingRule
    {
        private const string AnyTypeLabel = "Any Asset";
        private const char TypeSeparator = '.';

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

        [Tooltip("True when this rule was added by hand. Such a rule is never removed automatically.")]
        [field: SerializeField] public bool UserCreated { get; set; }

        [Tooltip("Fields that were changed by hand. The auto detection leaves exactly these alone.")]
        [SerializeField] private List<EAssetNamingField> editedFields = new();

        /// <summary>True when the rule was created or changed by hand and belongs to the user.</summary>
        public bool HasUserEdits => UserCreated || editedFields.Count > 0;

        /// <summary>Short type label for the rule table and the scan results.</summary>
        public string TypeLabel
        {
            get
            {
                if (string.IsNullOrEmpty(TypeName))
                    return AnyTypeLabel;

                int separator = TypeName.LastIndexOf(TypeSeparator);

                return separator < 0
                    ? TypeName
                    : TypeName[(separator + 1)..];
            }
        }

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

            // Kinds like Prefab or Sprite are resolved from the file and its importer, because
            // their asset type alone cannot tell them apart.
            if (TypeName == assetKind)
                return true;

            return HasMatchingType(assetType);
        }

        /// <summary>True when the field was changed by hand and has to survive a detection.</summary>
        public bool IsEdited(EAssetNamingField field) => editedFields.Contains(field);

        /// <summary>Remembers that a field was changed by hand.</summary>
        public void MarkEdited(EAssetNamingField field)
        {
            if (editedFields.Contains(field))
                return;

            editedFields.Add(field);
        }

        /// <summary>Forgets every edit, so the whole rule is refreshed by the next detection.</summary>
        public void ClearEdits()
        {
            editedFields.Clear();
            UserCreated = false;
        }

        /// <summary>
        /// Takes over everything the detection found, field by field, but only where the field was
        /// not changed by hand. Returns true when something actually changed.
        /// </summary>
        public bool ApplyDetected(AssetNamingRule detected)
        {
            bool isChanged = false;

            isChanged |= ApplyLabel(detected);
            isChanged |= ApplyEnabled(detected);
            isChanged |= ApplyPathFilter(detected);
            isChanged |= ApplyStyle(detected);
            isChanged |= ApplyDigits(detected);
            isChanged |= ApplySuffixOptional(detected);
            isChanged |= ApplyList(EAssetNamingField.Prefixes, Naming.Prefixes, detected.Naming.Prefixes);
            isChanged |= ApplyList(EAssetNamingField.Suffixes, Naming.Suffixes, detected.Naming.Suffixes);
            isChanged |= ApplyList(EAssetNamingField.Stripped, Naming.Stripped, detected.Naming.Stripped);
            isChanged |= ApplyPattern(detected);

            return isChanged;
        }

        private static bool IsSameList(List<string> first, List<string> second)
        {
            if (first.Count != second.Count)
                return false;

            for (int index = 0; index < first.Count; index++)
            {
                if (first[index] != second[index])
                    return false;
            }

            return true;
        }

        private bool ApplyLabel(AssetNamingRule detected)
        {
            if (IsEdited(EAssetNamingField.Label)
                || Label == detected.Label)
                return false;

            Label = detected.Label;

            return true;
        }

        private bool ApplyEnabled(AssetNamingRule detected)
        {
            if (IsEdited(EAssetNamingField.Enabled)
                || Enabled == detected.Enabled)
                return false;

            Enabled = detected.Enabled;

            return true;
        }

        private bool ApplyPathFilter(AssetNamingRule detected)
        {
            if (IsEdited(EAssetNamingField.PathFilter)
                || PathFilter == detected.PathFilter)
                return false;

            PathFilter = detected.PathFilter;

            return true;
        }

        private bool ApplyStyle(AssetNamingRule detected)
        {
            if (IsEdited(EAssetNamingField.Style)
                || Naming.Style == detected.Naming.Style)
                return false;

            Naming.Style = detected.Naming.Style;

            return true;
        }

        private bool ApplyDigits(AssetNamingRule detected)
        {
            if (IsEdited(EAssetNamingField.Digits)
                || EnumerationDigits == detected.EnumerationDigits)
                return false;

            EnumerationDigits = detected.EnumerationDigits;

            return true;
        }

        private bool ApplySuffixOptional(AssetNamingRule detected)
        {
            if (IsEdited(EAssetNamingField.SuffixOptional)
                || Naming.SuffixOptional == detected.Naming.SuffixOptional)
                return false;

            Naming.SuffixOptional = detected.Naming.SuffixOptional;

            return true;
        }

        private bool ApplyPattern(AssetNamingRule detected)
        {
            if (IsEdited(EAssetNamingField.Pattern)
                || Naming.Pattern == detected.Naming.Pattern)
                return false;

            Naming.Pattern = detected.Naming.Pattern;

            return true;
        }

        private bool ApplyList(EAssetNamingField field, List<string> target, List<string> detected)
        {
            if (IsEdited(field)
                || IsSameList(target, detected))
                return false;

            target.Clear();
            target.AddRange(detected);

            return true;
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