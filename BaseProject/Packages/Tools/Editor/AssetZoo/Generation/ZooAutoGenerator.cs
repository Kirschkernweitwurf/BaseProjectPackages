using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Base.ToolsPackage.Editor.AssetZoo.Builder;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Generation
{
    /// <summary>
    /// Finds the art in the config's folders and sorts it into groups. A scan only ever adds: what an artist moved
    /// to another group, reordered or hid stays that way, and only assets deleted from the project disappear.
    /// </summary>
    internal static class ZooAutoGenerator
    {
        private const string AssetFilter = "t:Prefab t:Model";
        private const int HashFactor = 31;
        private const int HashSeed = 17;
        private const int HueSteps = 360;
        private const float LabelSaturation = 0.5f;
        private const float LabelValue = 1f;
        private const int MaxReportedTokens = 6;
        private const string NoConfigMessage = "No saved zoo selected.";
        private const string NoSourcesMessage = "Add a folder of art first.";
        private const int PartOffset = 1;
        private const string PrefabExtension = ".prefab";
        private const string ReportEllipsis = "...";
        private const string ReportSeparator = ", ";
        private const string UndoLabel = "Find Zoo Assets";

        /// <summary>
        /// Scans the config's sources and adds what it finds. Undoable and saves the asset.
        /// </summary>
        /// <param name="config">The saved zoo.</param>
        /// <param name="isStartingOver">True to forget all groups and hidden assets first.</param>
        /// <returns>What the scan did, in words for the artist.</returns>
        public static ZooGenerationResult Generate(ZooConfig config, bool isStartingOver = false)
        {
            if (config == null)
                return FailWithError(NoConfigMessage, null);

            AutoGenerateSettings settings = config.Generation;

            if (settings.Sources == null
                || settings.Sources.Count == 0)
                return ZooGenerationResult.Failed(NoSourcesMessage);

            List<string> paths = CollectPaths(settings, out List<string> missingSources);

            if (missingSources.Count > 0)
                return FailWithWarning($"Not found: {Describe(missingSources)}. Remove it or restore it.", config);

            List<FoundAsset> found = FindArt(paths, settings, out int nothingToSee, out int shownByPrefab,
                out PrefixSet prefixes);

            Undo.RecordObject(config, UndoLabel);

            if (isStartingOver)
                config.Categories.Clear();

            RemoveMissing(config);

            List<GameObject> added = AddNewAssets(config, found);

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);

            int assetCount = config.Categories.Sum(category => category.Entries.Count);

            if (assetCount == 0)
                return FailWithWarning($"Nothing to show in {Describe(settings.Sources)}.", config);

            string message = $"{assetCount} assets in {config.Categories.Count} groups.";
            string details = BuildDetails(prefixes, nothingToSee, shownByPrefab);

            return new ZooGenerationResult(true, message, details, added);
        }

        /// <summary>
        /// Adds a folder, prefab or model to what the zoo shows. Does not scan yet.
        /// </summary>
        /// <param name="config">The saved zoo.</param>
        /// <param name="path">The project path to add.</param>
        /// <returns>True when it was not listed yet.</returns>
        public static bool TryAddSource(ZooConfig config, string path)
        {
            if (config.Generation.Sources.Contains(path))
                return false;

            Undo.RecordObject(config, UndoLabel);
            config.Generation.Sources.Add(path);
            EditorUtility.SetDirty(config);

            return true;
        }

        /// <summary>
        /// Stops showing a folder, prefab or model, along with the assets only it brought in.
        /// </summary>
        /// <param name="config">The saved zoo.</param>
        /// <param name="path">The project path to remove.</param>
        public static void RemoveSource(ZooConfig config, string path)
        {
            Undo.RecordObject(config, UndoLabel);
            config.Generation.Sources.Remove(path);

            foreach (ZooCategory category in config.Categories)
            {
                category.Entries.RemoveAll(entry => IsOnlyFrom(entry, path, config.Generation.Sources));
                category.HiddenEntries?.RemoveAll(entry => IsOnlyFrom(entry, path, config.Generation.Sources));
            }

            config.Categories.RemoveAll(category => category.Entries.Count == 0);

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
        }

        /// <summary>
        /// The color of a group, derived from its name so it stays the same across scans and sessions.
        /// </summary>
        /// <param name="group">The group's name.</param>
        /// <returns>A soft color that reads on a dark label.</returns>
        public static Color GetCategoryColor(string group)
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

        private static bool IsOnlyFrom(GameObject entry, string removedSource, List<string> remainingSources)
        {
            if (entry == null)
                return true;

            string path = AssetDatabase.GetAssetPath(entry);

            return IsUnder(path, removedSource)
                && !remainingSources.Any(source => IsUnder(path, source));
        }

        private static bool IsUnder(string path, string source)
            => path == source || path.StartsWith(source + "/", StringComparison.Ordinal);

        private static List<string> CollectPaths(AutoGenerateSettings settings, out List<string> missingSources)
        {
            HashSet<string> paths = new();
            missingSources = new List<string>();

            foreach (string source in settings.Sources)
            {
                if (AssetDatabase.IsValidFolder(source))
                {
                    AddFolder(paths, source, settings.SearchDepth);
                    continue;
                }

                string guid = AssetDatabase.AssetPathToGUID(source, AssetPathToGUIDOptions.OnlyExistingAssets);

                if (string.IsNullOrEmpty(guid))
                {
                    missingSources.Add(source);
                    continue;
                }

                paths.Add(source);
            }

            return paths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void AddFolder(HashSet<string> paths, string folder, int maxDepth)
        {
            string[] folders =
            {
                folder
            };

            foreach (string guid in AssetDatabase.FindAssets(AssetFilter, folders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.IsNullOrEmpty(path)
                    && IsWithinDepth(path, folder, maxDepth))
                    paths.Add(path);
            }
        }

        private static bool IsWithinDepth(string assetPath, string rootFolder, int maxDepth)
        {
            if (maxDepth < 0)
                return true;

            int lastSlash = assetPath.LastIndexOf('/');
            if (lastSlash < 0)
                return true;

            string directory = assetPath[..lastSlash];
            if (directory.Length <= rootFolder.Length)
                return true;

            string relative = directory[(rootFolder.Length + 1)..];
            int depth = relative.Count(character => character == '/') + 1;

            return depth <= maxDepth;
        }

        private static List<FoundAsset> FindArt(List<string> paths, AutoGenerateSettings settings,
            out int nothingToSee, out int shownByPrefab, out PrefixSet prefixes)
        {
            nothingToSee = 0;
            shownByPrefab = 0;

            HashSet<string> modelsWithPrefab = settings.HasModelsWithPrefab
                ? new HashSet<string>()
                : FindModelsWithPrefab(paths);

            List<ScannedAsset> scanned = new();

            foreach (string path in paths)
            {
                if (modelsWithPrefab.Contains(path))
                {
                    shownByPrefab++;
                    continue;
                }

                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (asset == null
                    || !ZooVisuals.HasVisuals(asset))
                {
                    nothingToSee++;
                    continue;
                }

                scanned.Add(new ScannedAsset(asset, path));
            }

            prefixes = settings.GroupBy == EGroupBy.Name
                ? BuildPrefixes(scanned, settings)
                : null;

            return SortIntoGroups(scanned, settings, prefixes);
        }

        // A prefab made from a model shows that model, so listing both would put every piece of art in the zoo
        // twice, once with its materials and once on the import defaults.
        private static HashSet<string> FindModelsWithPrefab(List<string> paths)
        {
            HashSet<string> scannedModels = new(paths.Where(path => !IsPrefab(path)));
            HashSet<string> models = new();

            foreach (string path in paths)
            {
                if (!IsPrefab(path))
                    continue;

                foreach (string dependency in AssetDatabase.GetDependencies(path, true))
                {
                    if (scannedModels.Contains(dependency))
                        models.Add(dependency);
                }
            }

            return models;
        }

        private static bool IsPrefab(string path)
            => path.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase);

        private static PrefixSet BuildPrefixes(List<ScannedAsset> scanned, AutoGenerateSettings settings)
        {
            List<string> known = settings.Prefixes == null
                ? new List<string>()
                : settings.Prefixes.Where(prefix => !string.IsNullOrWhiteSpace(prefix)).ToList();

            StringComparer comparer = settings.IsPrefixCaseIgnored
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            string separator = GetSeparator(settings);

            List<string> firstTokens = scanned
                .Select(asset => SplitName(asset.Path, separator)[0])
                .ToList();

            return PrefixDetector.Build(known, firstTokens, settings.CanDetectPrefixes,
                settings.MinPrefixOccurrences, settings.MaxPrefixLength, comparer);
        }

        private static string GetSeparator(AutoGenerateSettings settings) => string.IsNullOrEmpty(settings.Separator)
            ? AutoGenerateSettings.DefaultSeparator
            : settings.Separator;

        private static string[] SplitName(string path, string separator)
        {
            string assetName = Path.GetFileNameWithoutExtension(path);
            string[] parts = assetName.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            return parts.Length > 0
                ? parts
                : new[]
                {
                    assetName
                };
        }

        private static List<FoundAsset> SortIntoGroups(List<ScannedAsset> scanned, AutoGenerateSettings settings,
            PrefixSet prefixes)
        {
            List<FoundAsset> found = new();
            string separator = GetSeparator(settings);

            foreach (ScannedAsset asset in scanned)
            {
                string name = Path.GetFileNameWithoutExtension(asset.Path);

                if (prefixes == null)
                {
                    found.Add(new FoundAsset(asset.Asset, ZooFolderGrouping.GetGroupName(asset.Path), name, 0));
                    continue;
                }

                ParseName(SplitName(asset.Path, separator), separator, prefixes, out string group,
                    out string sortKey, out int prefixOrder);

                found.Add(new FoundAsset(asset.Asset, group, sortKey, prefixOrder));
            }

            // Variants of the same asset (P_Garden_Rock_01, SM_Garden_Rock_01) end up next to each other.
            found.Sort((first, second) =>
            {
                int byName = StringComparer.OrdinalIgnoreCase.Compare(first.SortKey, second.SortKey);

                return byName != 0
                    ? byName
                    : first.PrefixOrder.CompareTo(second.PrefixOrder);
            });

            return found;
        }

        private static void ParseName(string[] parts, string separator, PrefixSet prefixes, out string group,
            out string sortKey, out int prefixOrder)
        {
            bool hasPrefix = prefixes.TryGetOrder(parts[0], out prefixOrder);
            if (!hasPrefix)
                prefixOrder = prefixes.NoPrefixOrder;

            // A name that is only a prefix has no word behind it, so the prefix has to serve as its group.
            int groupIndex = hasPrefix && parts.Length > PartOffset
                ? PartOffset
                : 0;

            group = parts[groupIndex];

            int nameStart = groupIndex + PartOffset;
            sortKey = parts.Length > nameStart
                ? string.Join(separator, parts, nameStart, parts.Length - nameStart)
                : string.Empty;
        }

        private static void RemoveMissing(ZooConfig config)
        {
            // Only a group that just lost its last asset goes. One a person added by hand and has not filled
            // yet is still waiting for its first prefab.
            List<ZooCategory> emptied = new();

            foreach (ZooCategory category in config.Categories)
            {
                if (category.TryRemoveMissing()
                    && category.Entries.Count == 0)
                    emptied.Add(category);
            }

            foreach (ZooCategory category in emptied)
                config.Categories.Remove(category);
        }

        private static List<GameObject> AddNewAssets(ZooConfig config, List<FoundAsset> found)
        {
            HashSet<GameObject> known = new();

            foreach (ZooCategory category in config.Categories)
                known.UnionWith(category.Entries);

            List<GameObject> added = new();
            List<ZooCategory> newGroups = new();

            foreach (FoundAsset asset in found)
            {
                if (known.Contains(asset.Asset))
                    continue;

                ZooCategory category = FindCategory(config.Categories, asset.Group)
                    ?? FindCategory(newGroups, asset.Group);

                if (category == null)
                {
                    category = new ZooCategory(asset.Group, GetCategoryColor(asset.Group), Array.Empty<GameObject>());
                    newGroups.Add(category);
                }

                category.TryAddEntry(asset.Asset);
                added.Add(asset.Asset);
            }

            config.Categories.AddRange(newGroups.OrderBy(group => group.Name, StringComparer.OrdinalIgnoreCase));

            return added;
        }

        private static ZooCategory FindCategory(List<ZooCategory> categories, string group)
        {
            foreach (ZooCategory category in categories)
            {
                if (category != null
                    && string.Equals(category.Name, group, StringComparison.OrdinalIgnoreCase))
                    return category;
            }

            return null;
        }

        private static string BuildDetails(PrefixSet prefixes, int nothingToSee, int shownByPrefab)
        {
            List<string> leftOut = new();

            if (shownByPrefab > 0)
                leftOut.Add($"{shownByPrefab} models that already have a prefab");

            if (nothingToSee > 0)
                leftOut.Add($"{nothingToSee} with nothing to see, like UI and managers");

            List<string> lines = new();

            if (leftOut.Count > 0)
                lines.Add($"Left out: {string.Join(ReportSeparator, leftOut)}.");

            if (prefixes == null)
                return string.Join(Environment.NewLine, lines);

            if (prefixes.Detected.Count > 0)
                lines.Add($"Detected prefixes: {Describe(prefixes.Detected)}.");

            if (prefixes.Suspects.Count > 0)
                lines.Add("Look like prefixes but were used as group names, add them to Prefixes if that "
                    + $"is wrong: {Describe(prefixes.Suspects)}.");

            return string.Join(Environment.NewLine, lines);
        }

        private static string Describe(IReadOnlyList<string> tokens)
        {
            if (tokens.Count <= MaxReportedTokens)
                return string.Join(ReportSeparator, tokens);

            return string.Join(ReportSeparator, tokens.Take(MaxReportedTokens)) + ReportEllipsis;
        }

        private readonly struct ScannedAsset
        {
            /// <summary>The loaded prefab or model.</summary>
            public GameObject Asset { get; }

            /// <summary>Where it is in the project.</summary>
            public string Path { get; }

            /// <summary>Records one asset the scan kept.</summary>
            /// <param name="asset">The loaded prefab or model.</param>
            /// <param name="path">Where it is in the project.</param>
            public ScannedAsset(GameObject asset, string path)
            {
                Asset = asset;
                Path = path;
            }
        }

        private readonly struct FoundAsset
        {
            /// <summary>The prefab the zoo places.</summary>
            public GameObject Asset { get; }

            /// <summary>The group it belongs in.</summary>
            public string Group { get; }

            /// <summary>The name the assets of a group are ordered by.</summary>
            public string SortKey { get; }

            /// <summary>Which naming prefix it carries, so variants come in the order the convention lists.</summary>
            public int PrefixOrder { get; }

            /// <summary>Records where one asset belongs.</summary>
            /// <param name="asset">The prefab the zoo places.</param>
            /// <param name="group">The group it belongs in.</param>
            /// <param name="sortKey">The name it is ordered by.</param>
            /// <param name="prefixOrder">Which naming prefix it carries.</param>
            public FoundAsset(GameObject asset, string group, string sortKey, int prefixOrder)
            {
                Asset = asset;
                Group = group;
                SortKey = sortKey;
                PrefixOrder = prefixOrder;
            }
        }
    }
}