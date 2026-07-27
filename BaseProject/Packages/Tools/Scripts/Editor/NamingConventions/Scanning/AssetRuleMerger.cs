using System.Collections.Generic;
using Base.ToolPackage.Editor.NamingConventions.Data;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Folds a detection run into the rule set. Anything a person created or changed by hand is
    /// left exactly as it is, field by field. Everything else belongs to the tool and may be
    /// refreshed or dropped, so a detection stays useful without ever undoing a decision.
    /// </summary>
    public static class AssetRuleMerger
    {
        /// <summary>Applies the detected rules and reports what changed.</summary>
        public static AssetRuleMergeResult Merge(AssetNamingRuleSet ruleSet, List<AssetNamingRule> detected)
        {
            AssetRuleMergeResult result = new();

            if (ruleSet == null)
                return result;

            Dictionary<string, AssetNamingRule> byType = IndexByType(detected);

            RefreshAndDrop(ruleSet, byType, result);
            AddMissing(ruleSet, detected, result);

            return result;
        }

        /// <summary>Runs the same merge on a throwaway copy, so a dialog can show the numbers first.</summary>
        public static AssetRuleMergeResult Preview(AssetNamingRuleSet ruleSet, List<AssetNamingRule> detected)
        {
            AssetRuleMergeResult result = new();

            if (ruleSet == null)
                return result;

            Dictionary<string, AssetNamingRule> byType = IndexByType(detected);
            HashSet<string> covered = new();

            foreach (AssetNamingRule rule in ruleSet.Rules)
            {
                covered.Add(rule.TypeName);

                if (byType.TryGetValue(rule.TypeName, out AssetNamingRule match))
                {
                    if (WouldChange(rule, match))
                        result.Updated++;

                    continue;
                }

                if (!rule.HasUserEdits)
                    result.Removed++;
            }

            foreach (AssetNamingRule rule in detected)
            {
                if (!covered.Add(rule.TypeName))
                    continue;

                result.Added++;
            }

            return result;
        }

        private static Dictionary<string, AssetNamingRule> IndexByType(List<AssetNamingRule> detected)
        {
            Dictionary<string, AssetNamingRule> byType = new();

            foreach (AssetNamingRule rule in detected)
                byType[rule.TypeName] = rule;

            return byType;
        }

        /// <summary>
        /// Refreshes the untouched fields of every rule the detection still knows, and removes the
        /// ones it does not, unless a person put work into them.
        /// </summary>
        private static void RefreshAndDrop(AssetNamingRuleSet ruleSet, Dictionary<string, AssetNamingRule> byType,
            AssetRuleMergeResult result)
        {
            // Walked back to front, so removing a rule leaves the earlier indices valid.
            for (int index = ruleSet.Rules.Count - 1; index >= 0; index--)
            {
                AssetNamingRule rule = ruleSet.Rules[index];

                if (byType.TryGetValue(rule.TypeName, out AssetNamingRule match))
                {
                    if (rule.ApplyDetected(match))
                        result.Updated++;

                    continue;
                }

                if (rule.HasUserEdits)
                    continue;

                ruleSet.RemoveRuleAt(index);
                result.Removed++;
            }
        }

        private static void AddMissing(AssetNamingRuleSet ruleSet, List<AssetNamingRule> detected,
            AssetRuleMergeResult result)
        {
            HashSet<string> covered = new();

            foreach (AssetNamingRule rule in ruleSet.Rules)
                covered.Add(rule.TypeName);

            foreach (AssetNamingRule rule in detected)
            {
                if (!covered.Add(rule.TypeName))
                    continue;

                ruleSet.AddRule(rule);
                result.Added++;
            }
        }

        /// <summary>
        /// True when at least one field of the rule would be refreshed. Mirrors the checks of
        /// <see cref="AssetNamingRule.ApplyDetected"/> without writing anything.
        /// </summary>
        private static bool WouldChange(AssetNamingRule rule, AssetNamingRule detected)
        {
            if (IsDifferent(rule, EAssetNamingField.Label, rule.Label, detected.Label))
                return true;

            if (IsDifferent(rule, EAssetNamingField.PathFilter, rule.PathFilter, detected.PathFilter))
                return true;

            if (IsDifferent(rule, EAssetNamingField.Pattern, rule.Naming.Pattern, detected.Naming.Pattern))
                return true;

            if (!rule.IsEdited(EAssetNamingField.Style)
                && rule.Naming.Style != detected.Naming.Style)
                return true;

            if (!rule.IsEdited(EAssetNamingField.Digits)
                && rule.EnumerationDigits != detected.EnumerationDigits)
                return true;

            if (!rule.IsEdited(EAssetNamingField.Enabled)
                && rule.Enabled != detected.Enabled)
                return true;

            if (!rule.IsEdited(EAssetNamingField.SuffixOptional)
                && rule.Naming.SuffixOptional != detected.Naming.SuffixOptional)
                return true;

            return HasListChange(rule, detected);
        }

        private static bool HasListChange(AssetNamingRule rule, AssetNamingRule detected)
        {
            if (!rule.IsEdited(EAssetNamingField.Prefixes)
                && !IsSameList(rule.Naming.Prefixes, detected.Naming.Prefixes))
                return true;

            if (!rule.IsEdited(EAssetNamingField.Suffixes)
                && !IsSameList(rule.Naming.Suffixes, detected.Naming.Suffixes))
                return true;

            return !rule.IsEdited(EAssetNamingField.Stripped)
                && !IsSameList(rule.Naming.Stripped, detected.Naming.Stripped);
        }

        private static bool IsDifferent(AssetNamingRule rule, EAssetNamingField field, string current,
            string value) => !rule.IsEdited(field) && current != value;

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
    }
}
