using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine.Serialization;

namespace Base.ToolPackage.Editor.AssetReserializer
{
    /// <summary>
    /// Rewrites assets on disk using the current serializer. This is what makes a
    /// <see cref="FormerlySerializedAsAttribute"/> rename stick: the attribute only redirects the
    /// value while the asset is loaded, so the file keeps the old field name until something writes
    /// it back out.
    /// </summary>
    /// <remarks>
    /// The rewrite uses the serializer of the running editor, so the diff is usually larger than the
    /// rename alone. Commit before running a batch.
    /// </remarks>
    public static class ReserializeRunner
    {
        private const string PrefabFilter = "t:Prefab";
        private const string SceneFilter = "t:Scene";
        private const string ScriptableObjectFilter = "t:ScriptableObject";

        /// <summary>
        /// Collects the asset paths a run would touch, without changing anything.
        /// </summary>
        /// <param name="folderPaths">
        /// Project relative folders to search. Pass an empty list to search the whole project.
        /// </param>
        /// <param name="kinds">Which asset kinds to include.</param>
        /// <returns>The matching asset paths, sorted and free of duplicates.</returns>
        public static IReadOnlyList<string> CollectPaths(IReadOnlyList<string> folderPaths,
            EReserializeAssetKinds kinds)
        {
            if (kinds == EReserializeAssetKinds.None)
            {
                CustomLogger.LogWarning("No asset kinds selected, so there is nothing to collect.", null);
                return Array.Empty<string>();
            }

            string[] searchFolders = BuildSearchFolders(folderPaths);
            SortedSet<string> paths = new(StringComparer.Ordinal);

            Collect(paths, kinds, EReserializeAssetKinds.Prefabs, PrefabFilter, searchFolders);
            Collect(paths, kinds, EReserializeAssetKinds.Scenes, SceneFilter, searchFolders);
            Collect(paths, kinds, EReserializeAssetKinds.ScriptableObjects, ScriptableObjectFilter, searchFolders);

            return new List<string>(paths);
        }

        /// <summary>
        /// Reserializes the given assets and saves the result.
        /// </summary>
        /// <param name="assetPaths">The asset paths to rewrite.</param>
        /// <returns>How many assets were handed to the asset database.</returns>
        public static int Run(IReadOnlyList<string> assetPaths)
        {
            if (assetPaths == null
                || assetPaths.Count == 0)
            {
                CustomLogger.LogWarning("Nothing to reserialize, the path list is empty.", null);
                return 0;
            }

            // Reserializes both the asset and its meta file, which is what a field rename needs.
            AssetDatabase.ForceReserializeAssets(assetPaths);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CustomLogger.Log($"Reserialized {assetPaths.Count} asset(s).", null);

            return assetPaths.Count;
        }

        /// <summary>
        /// Returns the folders to search, or <c>null</c> to let the asset database search everything.
        /// </summary>
        private static string[] BuildSearchFolders(IReadOnlyList<string> folderPaths)
        {
            if (folderPaths == null
                || folderPaths.Count == 0)
                return null;

            List<string> valid = new(folderPaths.Count);

            foreach (string path in folderPaths)
            {
                if (AssetDatabase.IsValidFolder(path))
                    valid.Add(path);
            }

            return valid.Count > 0
                ? valid.ToArray()
                : null;
        }

        private static void Collect(ISet<string> paths, EReserializeAssetKinds kinds, EReserializeAssetKinds kind,
            string filter, string[] searchFolders)
        {
            if ((kinds & kind) == 0)
                return;

            string[] guids = searchFolders == null
                ? AssetDatabase.FindAssets(filter)
                : AssetDatabase.FindAssets(filter, searchFolders);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.IsNullOrEmpty(path))
                    paths.Add(path);
            }
        }
    }
}