using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolsPackage.Editor.NamingConventions.Data;
using Base.ToolsPackage.Editor.Shared;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolsPackage.Editor.NamingConventions.Scanning
{
    /// <summary>Walks the assets of the project and collects the ones that break a rule.</summary>
    internal static class AssetNamingScanner
    {
        private const string AssetsRoot = "Assets/";
        private const string MissingIndexMessage =
            "Collecting assets needs an asset index to read the project through.";
        private const string PackagesRoot = "Packages/";
        private const int ProgressStep = 200;
        private const string ProgressTitle = "Asset Naming Conventions";

        /// <summary>Returns every asset that breaks the first rule matching it.</summary>
        public static List<AssetNamingViolation> Scan(AssetNamingRuleSet ruleSet)
        {
            List<AssetNamingViolation> violations = new();

            if (ruleSet == null)
            {
                CustomLogger.LogError($"Scanning needs an {nameof(AssetNamingRuleSet)}.", null);
                return violations;
            }

            List<string> paths = CollectAssetPaths(ruleSet, AssetDatabaseIndex.Default);

            try
            {
                for (int index = 0; index < paths.Count; index++)
                {
                    ReportProgress(index, paths.Count);
                    AssetNamingViolation violation = Inspect(ruleSet, paths[index]);

                    if (violation == null)
                        continue;

                    violations.Add(violation);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return violations;
        }

        /// <summary>Returns every asset path that takes part in a scan.</summary>
        /// <param name="ruleSet">The rules that say what is in scope.</param>
        /// <param name="index">
        /// The project to read. Pass <see cref="AssetDatabaseIndex.Default"/> for the live one.
        /// </param>
        /// <returns>Every path that takes part in a scan, in the order the project reports them.</returns>
        public static List<string> CollectAssetPaths(AssetNamingRuleSet ruleSet, IAssetIndex index)
        {
            List<string> paths = new();

            if (ruleSet == null)
            {
                CustomLogger.LogError($"Collecting assets needs an {nameof(AssetNamingRuleSet)}.", null);
                return paths;
            }

            if (index == null)
            {
                CustomLogger.LogError(MissingIndexMessage, ruleSet);
                return paths;
            }

            foreach (string path in index.GetAllAssetPaths())
            {
                if (!IsScannable(ruleSet, index, path))
                    continue;

                paths.Add(path);
            }

            return paths;
        }

        private static bool IsScannable(AssetNamingRuleSet ruleSet, IAssetIndex index, string path)
        {
            bool isInProject = path.StartsWith(AssetsRoot, StringComparison.Ordinal)
                || ruleSet.IncludePackages && path.StartsWith(PackagesRoot, StringComparison.Ordinal);

            if (!isInProject)
                return false;

            if (index.IsValidFolder(path))
                return false;

            return !ruleSet.IsIgnoredPath(path);
        }

        private static void ReportProgress(int index, int total)
        {
            if (index % ProgressStep != 0)
                return;

            EditorUtility.DisplayProgressBar(ProgressTitle, $"Checking {index} of {total}", (float)index / total);
        }

        private static AssetNamingViolation Inspect(AssetNamingRuleSet ruleSet, string assetPath)
        {
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            string assetKind = AssetKindResolver.Resolve(assetPath, assetType);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            foreach (AssetNamingRule rule in ruleSet.Rules)
            {
                if (!rule.AppliesTo(assetPath, assetKind, assetType))
                    continue;

                string requiredSuffix = AssetSuffixResolver.Resolve(assetPath, rule.Naming);

                if (AssetNameEvaluator.IsValid(rule, fileName, requiredSuffix))
                    return null;

                return Build(rule, assetPath, fileName, requiredSuffix);
            }

            return null;
        }

        private static AssetNamingViolation Build(AssetNamingRule rule, string assetPath, string fileName,
            string requiredSuffix)
        {
            string suggestion = AssetNameEvaluator.Suggest(rule, fileName, requiredSuffix);

            // A violation the tool cannot improve is noise, so a suggestion equal to the current
            // name means the rule and the name simply disagree in a way nothing can fix.
            if (suggestion == fileName)
                return null;

            string unique = AssetNameUniquifier.MakeUnique(assetPath, suggestion, rule.EnumerationDigits);
            string reason = AssetNameEvaluator.Reason(rule, fileName, requiredSuffix);

            // A bumped number looks arbitrary next to assets that keep theirs, so the row says why.
            if (unique != suggestion)
                reason += ", " + suggestion + " is taken";

            return new AssetNamingViolation(assetPath, AssetDatabase.AssetPathToGUID(assetPath), fileName,
                rule.Label, reason, unique);
        }
    }
}