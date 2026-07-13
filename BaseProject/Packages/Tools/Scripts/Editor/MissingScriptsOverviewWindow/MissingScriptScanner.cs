using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.ToolPackage.Editor.MissingScriptsOverviewWindow
{
    /// <summary>
    /// Finds GameObjects and assets that hold missing scripts anywhere in the project.
    /// </summary>
    public static class MissingScriptScanner
    {
        public static List<MissingScriptEntry> Scan(bool scanScenes,
            bool scanAllScenes,
            bool scanPrefabs,
            bool scanScriptableObjects)
        {
            List<MissingScriptEntry> results = new();

            if (scanScenes)
                ScanScenes(scanAllScenes, results);

            if (scanPrefabs)
                ScanPrefabs(results);

            if (scanScriptableObjects)
                ScanScriptableObjects(results);

            return results;
        }

        private static void ScanScenes(bool scanAllScenes, List<MissingScriptEntry> results)
        {
            if (!scanAllScenes)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);

                    if (scene.isLoaded)
                        ScanLoadedScene(scene, results);
                }

                return;
            }

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();

            // Give the user a chance to save before we start opening scenes.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string[] guids = AssetDatabase.FindAssets("t:Scene");

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    if (!path.StartsWith("Assets/"))
                        continue;

                    EditorUtility.DisplayProgressBar("Scanning Scenes",
                        path,
                        (float)i / Mathf.Max(1, guids.Length));

                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    ScanLoadedScene(scene, results);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (setup != null && setup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }

        private static void ScanLoadedScene(Scene scene, List<MissingScriptEntry> results)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                ScanHierarchy(root, EMissingScriptSource.Scene, scene.path, results);
        }

        private static void ScanPrefabs(List<MissingScriptEntry> results)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    EditorUtility.DisplayProgressBar("Scanning Prefabs",
                        path,
                        (float)i / Mathf.Max(1, guids.Length));

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (prefab == null)
                        continue;

                    ScanHierarchy(prefab, EMissingScriptSource.Prefab, path, results);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void ScanScriptableObjects(List<MissingScriptEntry> results)
        {
            string[] allPaths = AssetDatabase.GetAllAssetPaths();

            try
            {
                for (int i = 0; i < allPaths.Length; i++)
                {
                    string path = allPaths[i];

                    if (!path.StartsWith("Assets/") || !path.EndsWith(".asset"))
                        continue;

                    EditorUtility.DisplayProgressBar("Scanning Assets",
                        path,
                        (float)i / Mathf.Max(1, allPaths.Length));

                    Object asset = AssetDatabase.LoadMainAssetAtPath(path);

                    if (asset == null)
                    {
                        // The asset could not be loaded at all, likely a broken script.
                        results.Add(new MissingScriptEntry(EMissingScriptSource.ScriptableObject,
                            path,
                            null,
                            Path.GetFileNameWithoutExtension(path),
                            1,
                            true));

                        continue;
                    }

                    SerializedObject serialized = new(asset);
                    SerializedProperty scriptProperty = serialized.FindProperty("m_Script");

                    if (scriptProperty != null && scriptProperty.objectReferenceValue == null)
                        results.Add(new MissingScriptEntry(EMissingScriptSource.ScriptableObject,
                            path,
                            null,
                            asset.name,
                            1));

                    serialized.Dispose();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void ScanHierarchy(GameObject root,
            EMissingScriptSource source,
            string assetPath,
            List<MissingScriptEntry> results)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform current in transforms)
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(current.gameObject);

                if (count <= 0)
                    continue;

                results.Add(new MissingScriptEntry(source,
                    assetPath,
                    BuildSiblingPath(current),
                    BuildDisplayPath(current),
                    count));
            }
        }

        private static int[] BuildSiblingPath(Transform target)
        {
            List<int> indices = new();
            Transform current = target;

            while (current != null)
            {
                indices.Add(current.GetSiblingIndex());
                current = current.parent;
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private static string BuildDisplayPath(Transform target)
        {
            List<string> names = new();
            Transform current = target;

            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }
    }
}