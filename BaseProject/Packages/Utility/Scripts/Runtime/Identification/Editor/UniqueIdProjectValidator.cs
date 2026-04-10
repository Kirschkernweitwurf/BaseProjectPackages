#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Utility.Logging;

namespace Utility.Identification.Editor
{
    /// <summary>
    /// Responsible for building a clean UniqueId registry
    /// and fixing any collisions/missing IDs across the whole project.
    /// <para/>
    /// Runs automatically on editor/domain reload via static constructor.
    /// <para/>
    /// Can also be reused from build preprocess step.
    /// </summary>
    [InitializeOnLoad]
    internal static class UniqueIdProjectValidator
    {
        static UniqueIdProjectValidator()
        {
            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool("UniqueIdValidator_RanOnce", false))
                    return;

                SessionState.SetBool("UniqueIdValidator_RanOnce", true);
                RebuildAndFixAll();
            };
        }

        /// <summary>
        /// Rebuilds the UniqueId registry from scratch and ensures all
        /// <see cref="IUniquelyIdentifiable"/> assets have valid UniqueIds.
        /// </summary>
        internal static void RebuildAndFixAll()
        {
            UniqueIdRegistry.Reset();

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            int fixedCount = 0;
            int totalChecked = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset is not IUniquelyIdentifiable)
                    continue;

                totalChecked++;

                string currentId = UniqueIdSerializer.GetId(asset);
                string finalId = UniqueIdRegistry.EnsureUnique(asset, currentId);

                if (finalId == currentId)
                    continue;

                if (!string.IsNullOrEmpty(currentId))
                {
                    CustomLogger.LogWarning($"Duplicate or invalid UniqueId found on {LogTextFormatter.Bold(asset.name)}" +
                                            $"at {LogTextFormatter.Italic(path)}. Regenerated to {finalId}.", asset);
                }

                UniqueIdSerializer.SetId(asset, finalId);
                EditorUtility.SetDirty(asset);
                fixedCount++;
            }

            if (fixedCount > 0)
                CustomLogger.Log($"Checked {totalChecked} identifiable assets and fixed" +
                                 $" {fixedCount} duplicate or missing IDs.", null);
            else
                CustomLogger.Log($"All {totalChecked} identifiable assets have valid unique IDs.", null);
        }
    }
}
#endif