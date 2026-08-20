namespace Base.ToolPackage.Editor.NamingConventions.Data
{
    /// <summary>An asset that breaks its rule, together with the file name that would fix it.</summary>
    public sealed class AssetNamingViolation
    {
        /// <summary>Project relative path of the asset.</summary>
        public string AssetPath { get; }

        /// <summary>GUID of the asset, used by the dismiss store so ignores survive renames.</summary>
        public string Guid { get; }

        /// <summary>Current file name without the extension.</summary>
        public string CurrentName { get; }

        /// <summary>Label of the rule that was broken.</summary>
        public string RuleLabel { get; }

        /// <summary>Short explanation of why the name was rejected.</summary>
        public string Reason { get; }

        /// <summary>
        /// Replacement file name without the extension. Stays writable because the window lets the
        /// user adjust the suggestion before the rename is applied.
        /// </summary>
        public string Suggestion { get; set; }

        /// <summary>Creates a violation.</summary>
        public AssetNamingViolation(string assetPath, string guid, string currentName, string ruleLabel,
            string reason, string suggestion)
        {
            AssetPath = assetPath;
            Guid = guid;
            CurrentName = currentName;
            RuleLabel = ruleLabel;
            Reason = reason;
            Suggestion = suggestion;
        }
    }
}