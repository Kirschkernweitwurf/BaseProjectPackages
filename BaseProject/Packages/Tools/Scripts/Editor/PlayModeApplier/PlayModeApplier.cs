using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Writes a captured payload back onto an edit mode object or a prefab asset.
    /// Nothing runs automatically, every apply is triggered by the user from the Play Mode Saver window.
    /// </summary>
    public static class PlayModeApplier
    {
        private const string UndoName = "Apply Play Mode Changes";

        /// <summary>Returns true when the payload has everything its chosen destination needs.</summary>
        public static bool CanApply(PlayModeSavePayload payload)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling)
                return false;

            if (payload.applyTarget == EPlayModeApplyTarget.PrefabAsset)
                return !string.IsNullOrEmpty(payload.sourcePrefabGuid);

            return SceneManager.GetSceneByPath(payload.scenePath).isLoaded;
        }

        /// <summary>Applies one payload. Returns false and logs a warning when the destination is gone.</summary>
        public static bool TryApply(PlayModeSavePayload payload)
        {
            if (payload.applyTarget == EPlayModeApplyTarget.PrefabAsset)
                return TryApplyToPrefabAsset(payload);

            return TryApplyToSceneObject(payload);
        }

        private static bool TryApplyToSceneObject(PlayModeSavePayload payload)
        {
            Scene scene = SceneManager.GetSceneByPath(payload.scenePath);
            if (!scene.IsValid()
                || !scene.isLoaded)
            {
                CustomLogger.LogWarning(
                    $"Scene '{payload.scenePath}' must be open to restore '{payload.displayName}'.", null);

                return false;
            }

            Transform owner = PlayModeObjectLocator.ResolveInScene(scene, payload.sceneNamePath,
                payload.sceneIndexPath);

            if (owner == null)
            {
                CustomLogger.LogWarning($"Could not find '{payload.sceneNamePath}' in '{payload.scenePath}'. "
                    + "If it was spawned at runtime, switch this entry to Prefab Asset.", null);

                return false;
            }

            Component target = PlayModeObjectLocator.ResolveComponent(owner, payload.componentTypeName,
                payload.componentIndex);

            if (target == null)
            {
                CustomLogger.LogWarning(
                    $"Could not find '{payload.componentTypeName}' on '{payload.sceneNamePath}'.", null);

                return false;
            }

            Undo.RecordObject(target, UndoName);
            PlayModeSerializationUtility.ApplyJson(target, payload.json);
            PlayModeSerializationUtility.ApplyObjectReferences(target, payload.objectReferences, null);
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(scene);

            if (payload.applyTarget == EPlayModeApplyTarget.PrefabOverride)
                PushOverrideToPrefab(target, payload.displayName);

            return true;
        }

        private static bool TryApplyToPrefabAsset(PlayModeSavePayload payload)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(payload.sourcePrefabGuid);
            if (string.IsNullOrEmpty(assetPath))
            {
                CustomLogger.LogWarning($"No destination prefab for '{payload.displayName}'.", null);
                return false;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(assetPath);
            if (contents == null)
                return false;

            try
            {
                Transform owner = PlayModeObjectLocator.ResolveFromRoot(contents.transform, payload.prefabNamePath,
                    payload.prefabIndexPath);

                if (owner == null)
                {
                    CustomLogger.LogWarning($"Could not find '{payload.prefabNamePath}' in '{assetPath}'.", null);
                    return false;
                }

                Component target = PlayModeObjectLocator.ResolveComponent(owner, payload.componentTypeName,
                    payload.componentIndex);

                if (target == null)
                {
                    CustomLogger.LogWarning($"Could not find '{payload.componentTypeName}' in '{assetPath}'.", null);
                    return false;
                }

                PlayModeSerializationUtility.ApplyJson(target, payload.json);
                PlayModeSerializationUtility.ApplyObjectReferences(target, payload.objectReferences,
                    contents.transform);

                PrefabUtility.SaveAsPrefabAsset(contents, assetPath);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void PushOverrideToPrefab(Object target, string displayName)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(target))
            {
                CustomLogger.LogWarning($"Kept '{displayName}' in the scene because it is not a prefab.", target);
                return;
            }

            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(target);
            if (root == null)
                return;

            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
            if (string.IsNullOrEmpty(assetPath))
                return;

            PrefabUtility.ApplyObjectOverride(target, assetPath, InteractionMode.AutomatedAction);
        }
    }
}