using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Enumerates the ScriptableObject assets of the project. Shared by the play-mode validator and the
    /// overview window, so both look at exactly the same set.
    /// </summary>
    internal static class ScriptableObjectAssets
    {
        private const string AssetFilter = "t:ScriptableObject";

        /// <summary>Loads every ScriptableObject asset, skipping the ones that fail to load.</summary>
        public static IEnumerable<ScriptableObject> LoadAll()
        {
            foreach (string guid in AssetDatabase.FindAssets(AssetFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset != null)
                    yield return asset;
            }
        }
    }
}