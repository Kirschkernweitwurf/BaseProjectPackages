#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Base.ToolPackage.Editor.AssetZoo.Config;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Generation
{
    /// <summary>
    /// Scans a project folder and fills a <see cref="ZooConfig"/> with categories,
    /// derived from asset names like "P_Garden_Rock_01". The first word after the
    /// prefix is the group, so "P_Garden_Rock_01" and "SM_Garden_Rock_01" both end
    /// up in the group "Garden".
    /// </summary>
    public static class ZooAutoGenerator
    {
        private const string AssetFilter = "t:Prefab t:Model";
        private const string AssetsRoot = "Assets";
        private const int GroupIndex = 1;
        private const int HashFactor = 31;
        private const int HashSeed = 17;
        private const int HueSteps = 360;
        private const float LabelSaturation = 0.5f;
        private const float LabelValue = 1f;
        private const int MinNameParts = 2;
        private const int NameStartIndex = 2;
        private const string UndoLabel = "Auto Generate Zoo";

        /// <summary>
        /// Scans the folder in <see cref="ZooConfig.Generation"/> and writes the resulting
        /// categories into the config. Undoable and saves the asset.
        /// </summary>
        public static ZooGenerationResult Generate(ZooConfig config)
        {
            if (config == null)
            {
                CustomLogger.LogError("No config provided.", null);
                return ZooGenerationResult.Failed("No config provided.");
            }

            AutoGenerateSettings settings = config.Generation;

            string folder = NormalizeFolder(settings.SearchFolder);
            if (!AssetDatabase.IsValidFolder(folder))
            {
                CustomLogger.LogError($"Search folder \"{folder}\" does not exist.", config);
                return ZooGenerationResult.Failed($"Search folder \"{folder}\" does not exist.");
            }

            List<string> prefixes = settings.Prefixes == null
                ? new List<string>()
                : settings.Prefixes.Where(prefix => !string.IsNullOrWhiteSpace(prefix)).ToList();

            if (prefixes.Count == 0)
            {
                CustomLogger.LogError("No prefixes defined.", config);
                return ZooGenerationResult.Failed("No prefixes defined.");
            }

            string separator = string.IsNullOrEmpty(settings.Separator)
                ? AutoGenerateSettings.DefaultSeparator
                : settings.Separator;

            StringComparison comparison = settings.IgnorePrefixCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            Dictionary<string, List<ScannedAsset>> groups =
                Scan(folder, separator, prefixes, comparison, settings.SearchDepth);

            if (groups.Count == 0)
            {
                CustomLogger.LogWarning($"No matching assets found in \"{folder}\".", config);
                return ZooGenerationResult.Failed($"No matching assets found in \"{folder}\".");
            }

            Undo.RecordObject(config, UndoLabel);

            if (!settings.MergeWithExisting)
                config.Categories.Clear();

            int added = Apply(config, groups, settings.ColorizeCategories);

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);

            string message = $"{groups.Count} groups found, {added} entries added.";
            return new ZooGenerationResult(true, config.Categories.Count, added, message);
        }

        private static Dictionary<string, List<ScannedAsset>> Scan(string folder, string separator,
            IReadOnlyList<string> prefixes, StringComparison comparison, int maxDepth)
        {
            Dictionary<string, List<ScannedAsset>> groups = new(StringComparer.OrdinalIgnoreCase);
            string[] guids = AssetDatabase.FindAssets(AssetFilter, new[]
            {
                folder
            });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !IsWithinDepth(path, folder, maxDepth))
                    continue;

                string assetName = Path.GetFileNameWithoutExtension(path);
                if (!TryParseName(assetName, separator, prefixes, comparison,
                        out string group, out string sortKey, out int prefixOrder))
                    continue;

                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                    continue;

                if (!groups.TryGetValue(group, out List<ScannedAsset> entries))
                {
                    entries = new List<ScannedAsset>();
                    groups.Add(group, entries);
                }

                entries.Add(new ScannedAsset(asset, sortKey, prefixOrder));
            }

            return groups;
        }

        private static int Apply(ZooConfig config, Dictionary<string, List<ScannedAsset>> groups, bool colorize)
        {
            int added = 0;

            foreach (string group in groups.Keys.OrderBy(keySelector: name => name, StringComparer.OrdinalIgnoreCase))
            {
                // Sort by the name behind the group first, so variants of the same asset
                // (P_Garden_Rock_01, SM_Garden_Rock_01) end up next to each other.
                List<GameObject> sorted = groups[group]
                    .OrderBy(keySelector: asset => asset.SortKey, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(asset => asset.PrefixOrder)
                    .Select(asset => asset.Asset)
                    .ToList();

                ZooCategory existing = config.Categories.FirstOrDefault(category => category != null
                    && string.Equals(category.Name, group, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    Color color = colorize
                        ? GetCategoryColor(group)
                        : Color.cyan;

                    config.Categories.Add(new ZooCategory(group, color, sorted));
                    added += sorted.Count;

                    continue;
                }

                if (colorize)
                    existing.SetLabelColor(GetCategoryColor(group));

                foreach (GameObject asset in sorted)
                {
                    if (existing.TryAddEntry(asset))
                        added++;
                }
            }

            return added;
        }

        private static bool TryParseName(string assetName, string separator, IReadOnlyList<string> prefixes,
            StringComparison comparison, out string group, out string sortKey, out int prefixOrder)
        {
            group = string.Empty;
            sortKey = string.Empty;
            prefixOrder = 0;

            string[] parts = assetName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < MinNameParts)
                return false;

            prefixOrder = IndexOfPrefix(prefixes, parts[0], comparison);
            if (prefixOrder < 0)
                return false;

            group = parts[GroupIndex];
            sortKey = parts.Length > NameStartIndex
                ? string.Join(separator, parts, NameStartIndex, parts.Length - NameStartIndex)
                : string.Empty;

            return true;
        }

        private static int IndexOfPrefix(IReadOnlyList<string> prefixes, string candidate, StringComparison comparison)
        {
            for (int i = 0; i < prefixes.Count; i++)
            {
                if (string.Equals(prefixes[i].Trim(), candidate, comparison))
                    return i;
            }

            return -1;
        }

        private static bool IsWithinDepth(string assetPath, string rootFolder, int maxDepth)
        {
            if (maxDepth < 0)
                return true;

            int lastSlash = assetPath.LastIndexOf('/');
            if (lastSlash < 0)
                return true;

            string directory = assetPath.Substring(0, lastSlash);
            if (directory.Length <= rootFolder.Length)
                return true;

            string relative = directory.Substring(rootFolder.Length + 1);
            int depth = relative.Count(character => character == '/') + 1;

            return depth <= maxDepth;
        }

        private static string NormalizeFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                return AssetsRoot;

            string path = folder.Replace('\\', '/').TrimEnd('/');
            string dataPath = Application.dataPath;

            // The FolderPath attribute can store absolute paths, map those back into the project.
            if (path.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
                path = AssetsRoot + path.Substring(dataPath.Length);

            return path.Length == 0
                ? AssetsRoot
                : path;
        }

        private static Color GetCategoryColor(string group)
        {
            // Own hash instead of string.GetHashCode, so colors stay stable across sessions.
            int hash = HashSeed;
            unchecked
            {
                foreach (char character in group)
                    hash = hash * HashFactor + char.ToLowerInvariant(character);
            }

            float hue = Mathf.Abs(hash % HueSteps) / (float)HueSteps;
            return Color.HSVToRGB(hue, LabelSaturation, LabelValue);
        }

        private readonly struct ScannedAsset
        {
            public GameObject Asset { get; }

            public string SortKey { get; }

            public int PrefixOrder { get; }

            public ScannedAsset(GameObject asset, string sortKey, int prefixOrder)
            {
                Asset = asset;
                SortKey = sortKey;
                PrefixOrder = prefixOrder;
            }
        }
    }
}
#endif