using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage.Editor
{
    /// <summary>
    /// Helpers for the repetitive parts of querying the <see cref="AssetDatabase"/>.
    /// </summary>
    public static class AssetDatabaseUtility
    {
        private const string TypeFilterFormat = "t:{0}";

        /// <summary>
        /// Returns the asset paths that match a search filter.
        /// </summary>
        /// <param name="filter">The search filter. Defaults to an empty filter, which matches everything.</param>
        /// <param name="folders">Optional folders to restrict the search to. Pass null to search everything.</param>
        /// <returns>The matching asset paths.</returns>
        /// <remarks>
        /// Validate folder paths with <see cref="AssetDatabase.IsValidFolder"/> beforehand, since Unity logs
        /// an error of its own for folders that do not exist.
        /// </remarks>
        public static List<string> FindAssetPaths(string filter = null, string[] folders = null)
        {
            filter ??= string.Empty;

            string[] guids = folders == null
                ? AssetDatabase.FindAssets(filter)
                : AssetDatabase.FindAssets(filter, folders);

            List<string> paths = new(guids.Length);

            foreach (string guid in guids)
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));

            return paths;
        }

        /// <summary>
        /// Loads every asset of type <typeparamref name="T"/> that matches a search filter.
        /// </summary>
        /// <typeparam name="T">The asset type to load.</typeparam>
        /// <param name="filter">
        /// The search filter. Defaults to a type filter built from <typeparamref name="T"/>.
        /// </param>
        /// <param name="folders">Optional folders to restrict the search to. Pass null to search everything.</param>
        /// <returns>The loaded assets. Assets that fail to load are skipped, so the list never contains null.</returns>
        public static List<T> LoadAll<T>(string filter = null, string[] folders = null) where T : Object
        {
            filter ??= string.Format(TypeFilterFormat, typeof(T).Name);

            List<string> paths = FindAssetPaths(filter, folders);
            List<T> assets = new(paths.Count);

            foreach (string path in paths)
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset != null)
                    assets.Add(asset);
            }

            return assets;
        }

        /// <summary>
        /// Returns only the folders that actually exist, so one missing path does not abort a whole scan.
        /// </summary>
        /// <param name="folders">The folder paths to check.</param>
        /// <returns>The existing folders, or null if none of them exist.</returns>
        public static string[] GetExistingFolders(params string[] folders)
        {
            if (folders == null)
                return null;

            List<string> existing = new(folders.Length);

            foreach (string folder in folders)
            {
                if (AssetDatabase.IsValidFolder(folder))
                    existing.Add(folder);
            }

            return existing.Count == 0
                ? null
                : existing.ToArray();
        }
    }
}