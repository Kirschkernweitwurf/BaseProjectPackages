#if UNITY_EDITOR
using Base.ToolPackage.Identification;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.Identification
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
        private const string UniqueIdValidatorRanOnce = "UniqueIdValidator_RanOnce";

        static UniqueIdProjectValidator() => EditorApplication.delayCall += () =>
        {
            if (!UniqueIdSettings.Enabled)
                return;

            if (SessionState.GetBool(UniqueIdValidatorRanOnce, false))
                return;

            SessionState.SetBool(UniqueIdValidatorRanOnce, true);
            RebuildAndFixAll();
        };

        /// <summary>
        /// Rebuilds the UniqueId registry from scratch and ensures all
        /// <see cref="IUniquelyIdentifiable"/> assets have valid UniqueIds.
        /// </summary>
        internal static void RebuildAndFixAll()
        {
            UniqueIdRegistry.Reset();

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

            if (guids.Length == 0)
                return;

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
                    CustomLogger.LogWarning(
                        $"Duplicate or invalid UniqueId found on {LogTextFormatter.Bold(asset.name)}"
                        + $"at {LogTextFormatter.Italic(path)}. Regenerated to {finalId}.", asset);

                UniqueIdSerializer.SetId(asset, finalId);
                EditorUtility.SetDirty(asset);
                fixedCount++;
            }

            if (totalChecked == 0)
                return;

            if (fixedCount > 0)
                CustomLogger.Log($"Checked {totalChecked} identifiable assets and fixed"
                    + $" {fixedCount} duplicate or missing IDs.", null);
            else
                CustomLogger.Log($"All {totalChecked} identifiable assets have valid unique IDs.", null);
        }
    }
}
#endif