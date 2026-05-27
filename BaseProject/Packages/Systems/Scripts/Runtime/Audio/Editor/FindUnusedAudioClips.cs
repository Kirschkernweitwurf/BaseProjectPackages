#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.SystemsCorePackage.Audio.Editor
{
    /// <summary>
    /// Editor window that finds AudioClip assets which are not referenced
    /// anywhere in the project (scenes, prefabs, or AudioContainer assets).
    /// </summary>
    public class FindUnusedAudioClips : EditorWindow
    {
        private const string ClipsFolder = "Assets/Audio";
        private const string ContainersFolder = "Assets/ScriptableObjects/AudioContainer";

        private readonly List<AudioClip> _unusedClips = new();
        private Vector2 _scroll;
        private bool _hasScanned;

        [MenuItem("Tools/Base Packages/Audio/Find Unused Audio Clips", priority = 2)]
        public static void ShowWindow()
        {
            FindUnusedAudioClips window = GetWindow<FindUnusedAudioClips>("Unused Audio Clips Finder");
            window.ScanForUnusedAudioClips();
        }

        private void OnGUI()
        {
            if (GUILayout.Button(_hasScanned ? "Rescan" : "Scan for Unused Audio Clips"))
                ScanForUnusedAudioClips();

            if (!_hasScanned)
            {
                EditorGUILayout.HelpBox("No scan results yet. Press Rescan to start.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            GUILayout.Label($"Found {_unusedClips.Count} unused clips.", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(_unusedClips.Count == 0))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select All in Project"))
                        Selection.objects = _unusedClips.Cast<Object>().ToArray();

                    if (GUILayout.Button("Delete All"))
                        DeleteUnusedClips();
                }
            }

            EditorGUILayout.Space();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (AudioClip clip in _unusedClips)
                EditorGUILayout.ObjectField(clip, typeof(AudioClip), false);
            EditorGUILayout.EndScrollView();
        }

        private void ScanForUnusedAudioClips()
        {
            // Ask to save first, since we open scenes during the scan.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            SceneSetup[] originalSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                HashSet<AudioClip> allClips = LoadAllClips();
                HashSet<AudioClip> usedClips = new();

                CollectUsedClipsFromScenes(usedClips);
                CollectUsedClipsFromPrefabs(usedClips);
                CollectUsedClipsFromContainers(usedClips);

                allClips.ExceptWith(usedClips);

                _unusedClips.Clear();
                _unusedClips.AddRange(allClips);
                _hasScanned = true;

                CustomLogger.Log($"Scan complete. {_unusedClips.Count} unused AudioClips found.", null);
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (originalSetup is { Length: > 0 })
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
            }
        }

        private static HashSet<AudioClip> LoadAllClips()
        {
            if (!AssetDatabase.IsValidFolder(ClipsFolder))
            {
                CustomLogger.LogWarning($"Clips folder '{ClipsFolder}' does not exist. No clips will be found.", null);
                return new HashSet<AudioClip>();
            }

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { ClipsFolder });
            return new HashSet<AudioClip>(
                guids.Select(guid => AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid)))
                     .Where(clip => clip != null)
            );
        }

        private static void CollectUsedClipsFromScenes(HashSet<AudioClip> usedClips)
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                EditorUtility.DisplayProgressBar("Scanning Scenes", path, (float)i / sceneCount);

                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (GameObject root in scene.GetRootGameObjects())
                    CollectClipsFromHierarchy(root, usedClips);
            }
        }

        private static void CollectUsedClipsFromPrefabs(HashSet<AudioClip> usedClips)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Scanning Prefabs", path, (float)i / guids.Length);

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                    CollectClipsFromHierarchy(prefab, usedClips);
            }
        }

        private static void CollectUsedClipsFromContainers(HashSet<AudioClip> usedClips)
        {
            if (!AssetDatabase.IsValidFolder(ContainersFolder))
            {
                CustomLogger.LogWarning($"Containers folder '{ContainersFolder}' does not exist." +
                                        " No clips will be found in containers.", null);
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:AudioContainer", new[] { ContainersFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioContainer container = AssetDatabase.LoadAssetAtPath<AudioContainer>(path);
                if (container == null || container.clips == null)
                    continue;

                foreach (AudioClip clip in container.clips)
                    if (clip != null)
                        usedClips.Add(clip);
            }
        }

        private static void CollectClipsFromHierarchy(GameObject root, HashSet<AudioClip> usedClips)
        {
            foreach (Component component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null) // Missing script.
                    continue;

                SerializedObject serialized = new(component);
                SerializedProperty prop = serialized.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.objectReferenceValue is AudioClip clip)
                    {
                        usedClips.Add(clip);
                    }
                }
            }
        }

        private void DeleteUnusedClips()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Unused Audio Clips",
                $"Delete {_unusedClips.Count} clips permanently? This cannot be undone.",
                "Delete", "Cancel");

            if (!confirmed)
                return;

            foreach (AudioClip clip in _unusedClips)
            {
                string path = AssetDatabase.GetAssetPath(clip);
                if (!string.IsNullOrEmpty(path))
                    AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.Refresh();
            _unusedClips.Clear();
            CustomLogger.Log("Deleted unused AudioClips.", null);
        }
    }
}
#endif