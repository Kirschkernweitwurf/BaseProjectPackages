using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Generation
{
    /// <summary>
    /// Scans a project folder and fills a <see cref="ZooConfig"/> with categories, derived from asset
    /// names like "SM_Kitchen_Table_01". Naming prefixes are recognized and stripped, so the first word
    /// behind the prefix becomes the group and "P_Garden_Rock_01" and "SM_Garden_Rock_01" both end up
    /// in "Garden". Names without a prefix are grouped by their first word instead of being dropped.
    /// </summary>
    internal static class ZooAutoGenerator
    {
        private const string AssetFilter = "t:Prefab t:Model";
        private const string AssetsRoot = "Assets";
        private const int HashFactor = 31;
        private const int HashSeed = 17;
        private const int HueSteps = 360;
        private const float LabelSaturation = 0.5f;
        private const float LabelValue = 1f;
        private const int MaxReportedTokens = 6;
        private const int MinNameParts = 2;
        private const string NoConfigMessage = "No config provided.";
        private const string NoPrefixesMessage = "No prefixes defined and prefix detection is off.";
        private const int PartOffset = 1;
        private const string ReportEllipsis = "...";
        private const string ReportSeparator = ", ";
        private const string UndoLabel = "Auto Generate Zoo";

        /// <summary>
        /// Scans the folder in <see cref="ZooConfig.Generation"/> and writes the resulting
        /// categories into the config. Undoable and saves the asset.
        /// </summary>
        public static ZooGenerationResult Generate(ZooConfig config)
        {
            if (config == null)
                return FailWithError(NoConfigMessage, null);

            AutoGenerateSettings settings = config.Generation;

            string folder = NormalizeFolder(settings.SearchFolder);
            if (!AssetDatabase.IsValidFolder(folder))
                return FailWithError($"Search folder \"{folder}\" does not exist.", config);

            List<string> knownPrefixes = settings.Prefixes == null
                ? new List<string>()
                : settings.Prefixes.Where(prefix => !string.IsNullOrWhiteSpace(prefix)).ToList();

            if (knownPrefixes.Count == 0
                && !settings.AutoDetectPrefixes)
                return FailWithError(NoPrefixesMessage, config);

            string separator = string.IsNullOrEmpty(settings.Separator)
                ? AutoGenerateSettings.DefaultSeparator
                : settings.Separator;

            StringComparer comparer = settings.IgnorePrefixCase
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            string emptyMessage = $"No matching assets found in \"{folder}\".";

            List<ScannedName> names = CollectNames(folder, separator, settings.SearchDepth, out int skipped);
            if (names.Count == 0)
                return FailWithWarning(emptyMessage, config);

            List<string> firstTokens = names.Select(name => name.Parts[0]).ToList();
            PrefixSet prefixes = PrefixDetector.Build(knownPrefixes, firstTokens, settings.AutoDetectPrefixes,
                minOccurrences: settings.MinPrefixOccurrences, maxLength: settings.MaxPrefixLength, comparer);

            Dictionary<string, List<ScannedAsset>> groups = BuildGroups(names, separator, prefixes,
                out int unloadable);

            if (groups.Count == 0)
                return FailWithWarning(emptyMessage, config);

            Undo.RecordObject(config, UndoLabel);

            if (!settings.MergeWithExisting)
                config.Categories.Clear();

            int added = Apply(config, groups, settings.ColorizeCategories);

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);

            string message = $"{groups.Count} groups found, {added} entries added.";
            string details = BuildDetails(prefixes, skipped, unloadable);

            return new ZooGenerationResult(true, config.Categories.Count, added, message, details);
        }

        private static ZooGenerationResult FailWithError(string message, ZooConfig config)
        {
            CustomLogger.LogError(message, config);
            return ZooGenerationResult.Failed(message);
        }

        private static ZooGenerationResult FailWithWarning(string message, ZooConfig config)
        {
            CustomLogger.LogWarning(message, config);
            return ZooGenerationResult.Failed(message);
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

        private static List<ScannedName> CollectNames(string folder, string separator, int maxDepth,
            out int skipped)
        {
            skipped = 0;
            List<ScannedName> names = new();

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
                string[] parts = assetName.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                // A single word carries no group, there is nothing sensible to sort it under.
                if (parts.Length < MinNameParts)
                {
                    skipped++;

                    continue;
                }

                names.Add(new ScannedName(path, parts));
            }

            return names;
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

        private static Dictionary<string, List<ScannedAsset>> BuildGroups(IReadOnlyList<ScannedName> names,
            string separator, PrefixSet prefixes, out int unloadable)
        {
            unloadable = 0;
            Dictionary<string, List<ScannedAsset>> groups = new(StringComparer.OrdinalIgnoreCase);

            foreach (ScannedName name in names)
            {
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(name.Path);
                if (asset == null)
                {
                    unloadable++;

                    continue;
                }

                ParseName(name.Parts, separator, prefixes, out string group, out string sortKey,
                    out int prefixOrder);

                if (!groups.TryGetValue(group, out List<ScannedAsset> entries))
                {
                    entries = new List<ScannedAsset>();
                    groups.Add(group, entries);
                }

                entries.Add(new ScannedAsset(asset, sortKey, prefixOrder));
            }

            return groups;
        }

        private static void ParseName(string[] parts, string separator, PrefixSet prefixes, out string group,
            out string sortKey, out int prefixOrder)
        {
            bool hasPrefix = prefixes.TryGetOrder(parts[0], out prefixOrder);
            if (!hasPrefix)
                prefixOrder = prefixes.NoPrefixOrder;

            int groupIndex = hasPrefix
                ? PartOffset
                : 0;

            group = parts[groupIndex];

            int nameStart = groupIndex + PartOffset;
            sortKey = parts.Length > nameStart
                ? string.Join(separator, parts, nameStart, parts.Length - nameStart)
                : string.Empty;
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

        private static string BuildDetails(PrefixSet prefixes, int skipped, int unloadable)
        {
            List<string> lines = new();

            if (prefixes.Detected.Count > 0)
                lines.Add($"Detected prefixes: {Describe(prefixes.Detected)}.");

            if (prefixes.Suspects.Count > 0)
                lines.Add("Look like prefixes but were used as group names, add them to Prefixes if that "
                    + $"is wrong: {Describe(prefixes.Suspects)}.");

            if (skipped > 0)
                lines.Add($"{skipped} assets skipped, their name has no part behind the first word.");

            if (unloadable > 0)
                lines.Add($"{unloadable} assets could not be loaded.");

            return string.Join(Environment.NewLine, lines);
        }

        private static string Describe(IReadOnlyList<string> tokens)
        {
            if (tokens.Count <= MaxReportedTokens)
                return string.Join(ReportSeparator, tokens);

            return string.Join(ReportSeparator, tokens.Take(MaxReportedTokens)) + ReportEllipsis;
        }

        private readonly struct ScannedName
        {
            /// <summary>Asset path the name was taken from.</summary>
            public string Path { get; }

            /// <summary>The name split by the separator, at least <see cref="MinNameParts"/> long.</summary>
            public string[] Parts { get; }

            /// <summary>Records one asset name before it is known what its prefix is.</summary>
            /// <param name="path">Asset path the name was taken from.</param>
            /// <param name="parts">The name split by the separator.</param>
            public ScannedName(string path, string[] parts)
            {
                Path = path;
                Parts = parts;
            }
        }

        private readonly struct ScannedAsset
        {
            /// <summary>The prefab the zoo places.</summary>
            public GameObject Asset { get; }

            /// <summary>The name the assets are ordered by within one prefix group.</summary>
            public string SortKey { get; }

            /// <summary>
            /// Which naming prefix the asset carries, so the groups come out in the order the
            /// convention lists them rather than alphabetically.
            /// </summary>
            public int PrefixOrder { get; }

            /// <summary>Records one asset the scan found.</summary>
            /// <param name="asset">The prefab the zoo places.</param>
            /// <param name="sortKey">The name it is ordered by within its group.</param>
            /// <param name="prefixOrder">Which naming prefix group it belongs to.</param>
            public ScannedAsset(GameObject asset, string sortKey, int prefixOrder)
            {
                Asset = asset;
                SortKey = sortKey;
                PrefixOrder = prefixOrder;
            }
        }
    }
}