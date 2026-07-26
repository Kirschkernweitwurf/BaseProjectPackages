using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.NamingConventions.Data;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>Walks the assets of the project and collects the ones that break a rule.</summary>
    public static class AssetNamingScanner
    {
        private const string AssetsRoot = "Assets/";
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

            List<string> paths = CollectAssetPaths(ruleSet);

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
        public static List<string> CollectAssetPaths(AssetNamingRuleSet ruleSet)
        {
            List<string> paths = new();

            if (ruleSet == null)
            {
                CustomLogger.LogError($"Collecting assets needs an {nameof(AssetNamingRuleSet)}.", null);
                return paths;
            }

            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (!IsScannable(ruleSet, path))
                    continue;

                paths.Add(path);
            }

            return paths;
        }

        private static bool IsScannable(AssetNamingRuleSet ruleSet, string path)
        {
            bool isInProject = path.StartsWith(AssetsRoot, StringComparison.Ordinal)
                || (ruleSet.IncludePackages && path.StartsWith(PackagesRoot, StringComparison.Ordinal));

            if (!isInProject)
                return false;

            if (AssetDatabase.IsValidFolder(path))
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

                if (AssetNameEvaluator.IsValid(rule, fileName))
                    return null;

                return new AssetNamingViolation(assetPath, AssetDatabase.AssetPathToGUID(assetPath), fileName,
                    rule.Label, AssetNameEvaluator.Reason(rule, fileName), AssetNameEvaluator.Suggest(rule, fileName));
            }

            return null;
        }
    }
}
