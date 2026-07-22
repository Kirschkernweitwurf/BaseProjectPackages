using System.IO;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Reconstructs the link between a runtime instantiated object and the prefab it came from.
    /// Unity's own prefab APIs all return null in play mode, so the link has to be guessed from
    /// the "(Clone)" naming convention. The guess can be overridden by hand in the window.
    /// </summary>
    public static class PlayModePrefabResolver
    {
        private const string CloneSuffix = "(Clone)";
        private const string PrefabFilter = " t:Prefab";

        /// <summary>
        /// Finds the highest ancestor that looks like an instantiated prefab root, or null when there is none.
        /// A null result means the object most likely came with the scene rather than from Instantiate.
        /// </summary>
        public static Transform FindCloneRoot(Transform target)
        {
            Transform cursor = target;
            Transform highestClone = null;

            while (cursor != null)
            {
                if (cursor.name.EndsWith(CloneSuffix))
                    highestClone = cursor;

                cursor = cursor.parent;
            }

            return highestClone;
        }

        /// <summary>
        /// Guesses the prefab a root came from by name.
        /// Returns an empty string unless exactly one prefab matches, so an ambiguous name never
        /// silently writes to the wrong asset.
        /// </summary>
        public static string FindPrefabGuid(Transform root)
        {
            string rootName = StripCloneSuffix(root.name);
            string[] guids = AssetDatabase.FindAssets($"\"{rootName}\"{PrefabFilter}");
            string match = string.Empty;
            int matchCount = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(assetPath) != rootName)
                    continue;

                match = guid;
                matchCount++;
            }

            return matchCount == 1
                ? match
                : string.Empty;
        }

        /// <summary>Removes Unity's "(Clone)" suffix from an instantiated object name.</summary>
        public static string StripCloneSuffix(string name)
        {
            if (!name.EndsWith(CloneSuffix))
                return name;

            return name.Substring(0, name.Length - CloneSuffix.Length).TrimEnd();
        }
    }
}