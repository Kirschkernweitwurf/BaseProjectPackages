using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.ToolPackage.Editor.MissingScriptsOverviewWindow
{
    /// <summary>
    /// Opens, selects, and frames the object behind a missing script entry.
    /// </summary>
    public static class MissingScriptNavigator
    {
        public static void Navigate(MissingScriptEntry entry)
        {
            switch (entry.Source)
            {
                case EMissingScriptSource.Scene:
                    NavigateToScene(entry);
                    break;

                case EMissingScriptSource.Prefab:
                    NavigateToPrefab(entry);
                    break;

                case EMissingScriptSource.ScriptableObject:
                    NavigateToAsset(entry);
                    break;
            }
        }

        private static void NavigateToScene(MissingScriptEntry entry)
        {
            Scene scene = FindLoadedScene(entry.AssetPath);

            if (!scene.IsValid())
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;

                // Open additive so the user does not lose their current scenes.
                scene = EditorSceneManager.OpenScene(entry.AssetPath, OpenSceneMode.Additive);
            }

            Transform target = ResolveInScene(scene, entry.SiblingPath);
            SelectAndFrame(target);
        }

        private static void NavigateToPrefab(MissingScriptEntry entry)
        {
            PrefabStage stage = PrefabStageUtility.OpenPrefab(entry.AssetPath);

            if (stage == null)
                return;

            Transform target = ResolveInPrefab(stage, entry.SiblingPath);
            SelectAndFrame(target);
        }

        private static void NavigateToAsset(MissingScriptEntry entry)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(entry.AssetPath);

            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
                return;
            }

            // Asset cannot be loaded, reveal its folder in the Project window instead.
            EditorUtility.FocusProjectWindow();

            Object folder = AssetDatabase.LoadAssetAtPath<Object>(Path.GetDirectoryName(entry.AssetPath));

            if (folder != null)
                EditorGUIUtility.PingObject(folder);
        }

        private static Scene FindLoadedScene(string path)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.isLoaded && scene.path == path)
                    return scene;
            }

            return default(Scene);
        }

        private static Transform ResolveInScene(Scene scene, int[] siblingPath)
        {
            if (!scene.IsValid() || siblingPath == null || siblingPath.Length == 0)
                return null;

            Transform root = null;

            foreach (GameObject candidate in scene.GetRootGameObjects())
            {
                if (candidate.transform.GetSiblingIndex() == siblingPath[0])
                {
                    root = candidate.transform;
                    break;
                }
            }

            return WalkChildren(root, siblingPath);
        }

        private static Transform ResolveInPrefab(PrefabStage stage, int[] siblingPath)
        {
            Transform root = stage.prefabContentsRoot.transform;

            if (siblingPath == null || siblingPath.Length == 0)
                return root;

            return WalkChildren(root, siblingPath);
        }

        private static Transform WalkChildren(Transform root, int[] siblingPath)
        {
            Transform current = root;

            for (int i = 1; i < siblingPath.Length; i++)
            {
                if (current == null)
                    return null;

                int index = siblingPath[i];

                if (index < 0 || index >= current.childCount)
                    return null;

                current = current.GetChild(index);
            }

            return current;
        }

        private static void SelectAndFrame(Transform target)
        {
            if (target == null)
                return;

            Selection.activeGameObject = target.gameObject;
            EditorGUIUtility.PingObject(target.gameObject);

            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();
        }
    }
}