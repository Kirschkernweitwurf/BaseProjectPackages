using System;
using System.IO;
using Base.ToolsPackage.Editor.NamingConventions.Data;
using Base.ToolsPackage.Editor.NamingConventions.Scanning;
using Base.UtilityPackage.Logging;

namespace Base.ToolsPackage.Editor.NamingConventions
{
    /// <summary>
    /// Asks the project's naming rules what an asset should be called, so a tool that creates or
    /// renames assets names them right the first time instead of leaving work for the naming window.
    /// </summary>
    /// <remarks>
    /// Without a rule set, or without a rule for the asset, the name comes back unchanged. A project
    /// that never set up naming conventions is not an error.
    /// </remarks>
    public static class AssetNamingConventions
    {
        /// <summary>
        /// The name an existing asset should carry, free of collisions in its folder.
        /// </summary>
        /// <param name="assetPath">Project path of the asset, including the extension.</param>
        /// <param name="assetType">Type of the main asset.</param>
        /// <param name="fileName">The name the user wants, without extension.</param>
        /// <returns>The file name without extension.</returns>
        public static string SuggestName(string assetPath, Type assetType, string fileName)
            => Suggest(assetPath, assetType, fileName, true);

        /// <summary>
        /// The name a new asset should carry, free of collisions in its folder.
        /// </summary>
        /// <param name="plannedPath">The project path the asset would be created at, including the extension.</param>
        /// <param name="assetType">Type of the asset to create.</param>
        /// <returns>The file name without extension.</returns>
        public static string SuggestNewName(string plannedPath, Type assetType)
            => Suggest(plannedPath, assetType, Path.GetFileNameWithoutExtension(plannedPath), false);

        private static string Suggest(string assetPath, Type assetType, string fileName, bool isExisting)
        {
            if (string.IsNullOrEmpty(assetPath)
                || string.IsNullOrEmpty(fileName))
            {
                CustomLogger.LogWarning("Cannot suggest an asset name without a path and a name.", null);
                return fileName;
            }

            AssetNamingRuleSet ruleSet = AssetNamingRuleSet.Load();

            AssetNamingRule rule = ruleSet == null
                ? null
                : FindRule(ruleSet, assetPath, assetType);

            if (rule == null)
                return AssetNameUniquifier.MakeUnique(assetPath, fileName, 0, isExisting);

            string requiredSuffix = AssetSuffixResolver.Resolve(assetPath, rule.Naming);

            string suggestion = AssetNameEvaluator.IsValid(rule, fileName, requiredSuffix)
                ? fileName
                : AssetNameEvaluator.Suggest(rule, fileName, requiredSuffix);

            return AssetNameUniquifier.MakeUnique(assetPath, suggestion, rule.EnumerationDigits, isExisting);
        }

        // The first rule wins, the same way the scanner decides which rule an asset breaks.
        private static AssetNamingRule FindRule(AssetNamingRuleSet ruleSet, string assetPath, Type assetType)
        {
            string assetKind = AssetKindResolver.Resolve(assetPath, assetType);

            foreach (AssetNamingRule rule in ruleSet.Rules)
            {
                if (rule.AppliesTo(assetPath, assetKind, assetType))
                    return rule;
            }

            return null;
        }
    }
}