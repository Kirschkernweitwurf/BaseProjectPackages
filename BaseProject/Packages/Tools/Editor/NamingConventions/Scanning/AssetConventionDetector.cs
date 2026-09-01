using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.NamingConventions.Data;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Reads the asset naming conventions a project already follows. Assets are grouped by their
    /// kind, so prefabs and models get their own rules instead of sharing one GameObject rule.
    /// Enumerations like "_01" are split off before the analysis. Every prefix or suffix that a
    /// meaningful share of the group uses is collected, so prefabs named P_ and SM_ end up with
    /// both prefixes allowed. The result is a starting point that stays fully editable in the
    /// rule table.
    /// </summary>
    internal static class AssetConventionDetector
    {
        private const float DominanceThreshold = 0.6f;
        private const float MinimumPrefixShare = 0.2f;
        private const int MinimumSamples = 5;

        // Suffix conventions are usually uniform, so a rarer token is more likely to be a category
        // like "_Lamp" than a real suffix. The higher bar keeps those out of the rule.
        private const float MinimumSuffixShare = 0.35f;
        private const int MinimumSuffixTokens = 2;
        private const char TokenSeparator = '_';

        /// <summary>Returns one rule per asset kind that shows a clear convention.</summary>
        public static List<AssetNamingRule> Detect(AssetNamingRuleSet ruleSet)
        {
            List<AssetNamingRule> rules = new();

            if (ruleSet == null)
            {
                CustomLogger.LogError($"Detection needs an {nameof(AssetNamingRuleSet)}.", null);
                return rules;
            }

            Dictionary<string, List<string>> namesByKind = GroupNamesByKind(ruleSet);

            foreach (KeyValuePair<string, List<string>> group in namesByKind)
            {
                if (group.Value.Count < MinimumSamples)
                    continue;

                rules.Add(BuildRule(group.Key, group.Value));
            }

            ApplyCreateMenuPrefixes(rules);
            rules.Sort(CompareRules);

            return rules;
        }

        /// <summary>
        /// Adds the prefixes the asset creation entries already declare. A type created as
        /// "ANRS_AssetNamingRuleSet" states its prefix in code, which beats guessing it from the
        /// handful of assets that happen to exist.
        /// </summary>
        private static void ApplyCreateMenuPrefixes(List<AssetNamingRule> rules)
        {
            foreach (KeyValuePair<Type, string> pair in CreateAssetMenuScanner.CollectPrefixes())
            {
                AssetNamingRule rule = FindRule(rules, pair.Key.FullName);

                if (rule == null)
                {
                    rule = new AssetNamingRule(pair.Key.Name, pair.Key.FullName, ENamingStyle.PascalSnakeCase);
                    rules.Add(rule);
                }

                if (!rule.Naming.Prefixes.Contains(pair.Value))
                    rule.Naming.Prefixes.Add(pair.Value);
            }
        }

        private static AssetNamingRule FindRule(List<AssetNamingRule> rules, string typeName)
        {
            foreach (AssetNamingRule rule in rules)
            {
                if (rule.TypeName == typeName)
                    return rule;
            }

            return null;
        }

        /// <summary>
        /// Orders the rules so the ones for an imported kind come first. Rules are checked in
        /// order, so a Sprite rule has to sit above the Texture2D rule that would swallow it.
        /// </summary>
        private static int CompareRules(AssetNamingRule first, AssetNamingRule second)
        {
            bool isFirstImported = AssetKindResolver.IsImportedKind(first.TypeName);
            bool isSecondImported = AssetKindResolver.IsImportedKind(second.TypeName);

            if (isFirstImported != isSecondImported)
                return isFirstImported
                    ? -1
                    : 1;

            return string.Compare(first.Label, second.Label, StringComparison.Ordinal);
        }

        private static Dictionary<string, List<string>> GroupNamesByKind(AssetNamingRuleSet ruleSet)
        {
            Dictionary<string, List<string>> groups = new();

            foreach (string path in AssetNamingScanner.CollectAssetPaths(ruleSet))
            {
                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);

                if (assetType == null)
                    continue;

                string key = AssetKindResolver.ResolveRuleTypeName(path, assetType);

                if (!groups.TryGetValue(key, out List<string> names))
                {
                    names = new List<string>();
                    groups[key] = names;
                }

                names.Add(Path.GetFileNameWithoutExtension(path));
            }

            return groups;
        }

        private static AssetNamingRule BuildRule(string typeName, List<string> names)
        {
            List<string> cores = SplitEnumerations(names, out int enumerationDigits);
            List<string> prefixes = CollectTokens(cores, true, MinimumPrefixShare);
            List<string> suffixes = CollectTokens(cores, false, MinimumSuffixShare);

            AssetNamingRule rule = new(LabelOf(typeName), typeName,
                FindDominantStyle(cores, prefixes, suffixes));

            rule.Naming.Prefixes.AddRange(prefixes);
            rule.Naming.Suffixes.AddRange(suffixes);
            rule.EnumerationDigits = enumerationDigits;

            return rule;
        }

        private static string LabelOf(string typeName)
        {
            int separator = typeName.LastIndexOf('.');

            return separator < 0
                ? typeName
                : typeName[(separator + 1)..];
        }

        /// <summary>
        /// Strips trailing enumerations off every name and returns the dominant digit count, or 0
        /// when the group does not enumerate consistently.
        /// </summary>
        private static List<string> SplitEnumerations(List<string> names, out int enumerationDigits)
        {
            List<string> cores = new(names.Count);
            Dictionary<int, int> digitCounts = new();
            int enumerated = 0;

            foreach (string name in names)
            {
                if (!AssetNameEvaluator.TrySplitEnumeration(name, out string core, out string number))
                {
                    cores.Add(name);
                    continue;
                }

                cores.Add(core);
                enumerated++;
                digitCounts.TryGetValue(number.Length, out int current);
                digitCounts[number.Length] = current + 1;
            }

            enumerationDigits = 0;

            if (enumerated < names.Count * DominanceThreshold)
                return cores;

            int bestCount = 0;

            foreach (KeyValuePair<int, int> pair in digitCounts)
            {
                if (pair.Value <= bestCount)
                    continue;

                enumerationDigits = pair.Key;
                bestCount = pair.Value;
            }

            if (bestCount < enumerated * DominanceThreshold)
                enumerationDigits = 0;

            return cores;
        }

        /// <summary>
        /// Collects every leading or trailing underscore token that a meaningful share of the
        /// group uses, most common first. Returns an empty list when the tokens together do not
        /// cover enough of the group to count as a convention.
        /// </summary>
        private static List<string> CollectTokens(List<string> names, bool fromStart, float minimumShare)
        {
            Dictionary<string, int> counts = new();

            foreach (string name in names)
            {
                string token = ExtractToken(name, fromStart);

                if (token.Length == 0)
                    continue;

                counts.TryGetValue(token, out int current);
                counts[token] = current + 1;
            }

            List<KeyValuePair<string, int>> ordered = new(counts);
            ordered.Sort((first, second) => second.Value.CompareTo(first.Value));

            List<string> kept = new();
            int covered = 0;

            foreach (KeyValuePair<string, int> pair in ordered)
            {
                if (pair.Value < names.Count * minimumShare)
                    break;

                kept.Add(pair.Key);
                covered += pair.Value;
            }

            return covered >= names.Count * DominanceThreshold
                ? kept
                : new List<string>();
        }

        private static string ExtractToken(string name, bool fromStart)
        {
            int separators = CountSeparators(name);

            if (separators == 0)
                return string.Empty;

            if (fromStart)
            {
                int first = name.IndexOf(TokenSeparator);

                return first <= 0
                    ? string.Empty
                    : name[..(first + 1)];
            }

            // The first separator already belongs to the prefix, so a suffix needs a second one.
            if (separators < MinimumSuffixTokens)
                return string.Empty;

            int last = name.LastIndexOf(TokenSeparator);

            return last >= name.Length - 1
                ? string.Empty
                : name[last..];
        }

        private static int CountSeparators(string name)
        {
            int count = 0;

            foreach (char symbol in name)
            {
                if (symbol != TokenSeparator)
                    continue;

                count++;
            }

            return count;
        }

        private static ENamingStyle FindDominantStyle(List<string> names, List<string> prefixes,
            List<string> suffixes)
        {
            Dictionary<ENamingStyle, int> counts = new();
            int samples = 0;

            foreach (string name in names)
            {
                string core = StripAffixes(name, prefixes, suffixes);

                if (core.Length == 0)
                    continue;

                ENamingStyle style = NameStyleUtility.Detect(core);

                if (style == ENamingStyle.Any)
                    continue;

                samples++;
                counts.TryGetValue(style, out int current);
                counts[style] = current + 1;
            }

            MergePascalStyles(counts);

            ENamingStyle best = ENamingStyle.Any;
            int bestCount = 0;

            foreach (KeyValuePair<ENamingStyle, int> pair in counts)
            {
                if (pair.Value <= bestCount)
                    continue;

                best = pair.Key;
                bestCount = pair.Value;
            }

            return bestCount >= samples * DominanceThreshold
                ? best
                : ENamingStyle.Any;
        }

        /// <summary>
        /// Pascal case is a special case of the mixed snake style, so a group that uses both gets
        /// the wider one instead of reporting half of its names as violations.
        /// </summary>
        private static void MergePascalStyles(Dictionary<ENamingStyle, int> counts)
        {
            if (!counts.TryGetValue(ENamingStyle.PascalSnakeCase, out int pascalSnake))
                return;

            counts.TryGetValue(ENamingStyle.PascalCase, out int pascal);
            counts[ENamingStyle.PascalSnakeCase] = pascalSnake + pascal;
            counts.Remove(ENamingStyle.PascalCase);
        }

        private static string StripAffixes(string name, List<string> prefixes, List<string> suffixes)
        {
            string core = name;

            foreach (string prefix in prefixes)
            {
                if (!core.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                core = core[prefix.Length..];
                break;
            }

            foreach (string suffix in suffixes)
            {
                if (!core.EndsWith(suffix, StringComparison.Ordinal))
                    continue;

                core = core[..^suffix.Length];
                break;
            }

            return core.Trim(TokenSeparator);
        }
    }
}