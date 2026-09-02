using System.Collections.Generic;
using Base.ToolsPackage.Editor.Shared;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// Collects every prefab asset in the project, classifies it, and optionally counts the overrides
    /// of each variant.
    /// </summary>
    internal static class PrefabScanner
    {
        private const string OverrideProgressTitle = "Analyzing Prefab Variants";
        private const string PrefabFilter = "t:Prefab";
        private const string ScanProgressTitle = "Scanning Prefabs";

        /// <summary>
        /// Scans the project for prefabs. Base prefabs outside the selected scope are pulled in as well,
        /// so that every variant chain stays complete.
        /// </summary>
        /// <param name="includePackages">True to also scan prefabs that live inside packages.</param>
        /// <param name="analyzeOverrides">True to count the overrides of every variant, which is slower.</param>
        /// <returns>All found prefabs.</returns>
        internal static List<PrefabEntry> Scan(bool includePackages, bool analyzeOverrides)
        {
            Dictionary<string, PrefabEntry> entries = new();

            try
            {
                Collect(includePackages, analyzeOverrides, entries);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return new List<PrefabEntry>(entries.Values);
        }

        private static void Collect(bool includePackages,
            bool analyzeOverrides,
            Dictionary<string, PrefabEntry> entries)
        {
            if (!CollectPrefabs(includePackages, entries))
                return;

            CollectMissingBases(entries);

            if (!analyzeOverrides)
                return;

            AnalyzeOverrides(entries);
        }

        // Returns false when the user canceled, so that the later steps are skipped.
        private static bool CollectPrefabs(bool includePackages, Dictionary<string, PrefabEntry> entries)
        {
            string[] guids = AssetDatabase.FindAssets(PrefabFilter);
            int unreadable = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!includePackages
                    && AssetOriginResolver.Classify(assetPath) != EAssetOrigin.Project)
                    continue;

                if (EditorUtility.DisplayCancelableProgressBar(ScanProgressTitle,
                        assetPath,
                        (float)i / Mathf.Max(1, guids.Length)))
                    return false;

                if (!TryCreateEntry(assetPath, entries))
                    unreadable++;
            }

            if (unreadable > 0)
                CustomLogger.LogWarning($"{unreadable} prefabs could not be loaded and were skipped.", null);

            return true;
        }

        // A base prefab can sit outside the scanned scope, for example inside a package. Without it the
        // variants below it would show up as unrelated roots.
        private static void CollectMissingBases(Dictionary<string, PrefabEntry> entries)
        {
            List<PrefabEntry> pending = new(entries.Values);

            while (pending.Count > 0)
            {
                List<PrefabEntry> created = new();

                foreach (PrefabEntry entry in pending)
                {
                    if (string.IsNullOrEmpty(entry.BaseGuid))
                        continue;

                    if (entries.ContainsKey(entry.BaseGuid))
                        continue;

                    string basePath = AssetDatabase.GUIDToAssetPath(entry.BaseGuid);

                    if (string.IsNullOrEmpty(basePath))
                        continue;

                    if (!TryCreateEntry(basePath, entries))
                        continue;

                    if (entries.TryGetValue(entry.BaseGuid, out PrefabEntry baseEntry))
                        created.Add(baseEntry);
                }

                pending = created;
            }
        }

        private static void AnalyzeOverrides(Dictionary<string, PrefabEntry> entries)
        {
            List<PrefabEntry> variants = new();

            foreach (PrefabEntry entry in entries.Values)
            {
                if (entry.Kind == EPrefabKind.Variant)
                    variants.Add(entry);
            }

            for (int i = 0; i < variants.Count; i++)
            {
                PrefabEntry variant = variants[i];

                if (EditorUtility.DisplayCancelableProgressBar(OverrideProgressTitle,
                        variant.AssetPath,
                        (float)i / Mathf.Max(1, variants.Count)))
                    return;

                variant.Overrides = PrefabOverrideCounter.Count(variant.AssetPath);
            }
        }

        private static bool TryCreateEntry(string assetPath, Dictionary<string, PrefabEntry> entries)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(guid))
                return false;

            if (entries.ContainsKey(guid))
                return true;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
                return false;

            int gameObjectCount = prefab.GetComponentsInChildren<Transform>(true).Length;
            int componentCount = prefab.GetComponentsInChildren<Component>(true).Length - gameObjectCount;

            entries.Add(guid, new PrefabEntry(guid,
                assetPath,
                ResolveKind(prefab),
                ResolveBaseGuid(prefab),
                gameObjectCount,
                Mathf.Max(0, componentCount)));

            return true;
        }

        private static EPrefabKind ResolveKind(GameObject prefab)
        {
            switch (PrefabUtility.GetPrefabAssetType(prefab))
            {
                case PrefabAssetType.Regular:
                    return EPrefabKind.Regular;

                case PrefabAssetType.Variant:
                    return EPrefabKind.Variant;

                case PrefabAssetType.Model:
                    return EPrefabKind.Model;

                default:
                    return EPrefabKind.Broken;
            }
        }

        private static string ResolveBaseGuid(GameObject prefab)
        {
            GameObject baseAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefab);

            if (baseAsset == null)
                return string.Empty;

            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(baseAsset));
        }
    }
}