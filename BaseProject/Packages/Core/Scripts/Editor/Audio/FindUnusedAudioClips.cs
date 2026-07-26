using System.Collections.Generic;
using System.Linq;
using Base.CorePackage.Audio;
using Base.ToolPackage.MenuManagerWindow;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.CorePackage.Editor.Audio
{
    /// <summary>
    /// Editor window that finds AudioClip assets which are not referenced
    /// anywhere in the project (scenes, prefabs, or AudioContainer assets).
    /// Also reports empty clip slots found in AudioContainer assets.
    /// </summary>
    public class FindUnusedAudioClips : EditorWindow
    {
        private const string ClipsFolder = "Assets/Audio";
        private const string ContainersFolder = "Assets/ScriptableObjects/AudioContainer";

        private readonly List<AudioClip> _unusedClips = new();
        private readonly List<NullClipReference> _nullClipReferences = new();

        private Vector2 _scroll;
        private bool _hasScanned;
        private bool _showNullClipReferences;

#region Unity Callbacks
        private void OnGUI()
        {
            if (GUILayout.Button(_hasScanned
                    ? "Rescan"
                    : "Scan for Unused Audio Clips"))
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

            DrawNullClipReferences();

            EditorGUILayout.Space();

            foreach (AudioClip clip in _unusedClips)
                EditorGUILayout.ObjectField(clip, typeof(AudioClip), false);

            EditorGUILayout.EndScrollView();
        }
#endregion

        [DynamicMenuItem("Tools/Base Packages/Assets/Audio/Unused Audio Clips")]
        public static void ShowWindow()
        {
            FindUnusedAudioClips window = GetWindow<FindUnusedAudioClips>("Unused Audio Clips Finder");
            window.ScanForUnusedAudioClips();
        }

        private static HashSet<AudioClip> LoadAllClips()
        {
            if (!AssetDatabase.IsValidFolder(ClipsFolder))
            {
                CustomLogger.LogWarning($"Clips folder '{ClipsFolder}' does not exist. No clips will be found.", null);
                return new HashSet<AudioClip>();
            }

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[]
            {
                ClipsFolder
            });

            return new HashSet<AudioClip>(guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(clip => clip != null));
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

        /// <summary>
        /// Collects every clip referenced by an AudioContainer and records the empty slots on the way.
        /// </summary>
        private static void CollectUsedClipsFromContainers(HashSet<AudioClip> usedClips,
            List<NullClipReference> nullClipReferences)
        {
            if (!AssetDatabase.IsValidFolder(ContainersFolder))
            {
                CustomLogger.LogWarning($"Containers folder '{ContainersFolder}' does not exist."
                    + " No clips will be found in containers.", null);

                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:AudioContainer", new[]
            {
                ContainersFolder
            });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioContainer container = AssetDatabase.LoadAssetAtPath<AudioContainer>(path);
                if (container == null)
                    continue;

                AudioClip[] clips = container.Clips;
                if (clips == null || clips.Length == 0)
                {
                    nullClipReferences.Add(new NullClipReference(container, NullClipReference.NoClipsIndex));
                    continue;
                }

                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] == null)
                    {
                        nullClipReferences.Add(new NullClipReference(container, i));
                        continue;
                    }

                    usedClips.Add(clips[i]);
                }
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
                    if (prop.propertyType == SerializedPropertyType.ObjectReference
                        && prop.objectReferenceValue is AudioClip clip)
                        usedClips.Add(clip);
                }
            }
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

                _nullClipReferences.Clear();

                CollectUsedClipsFromScenes(usedClips);
                CollectUsedClipsFromPrefabs(usedClips);
                CollectUsedClipsFromContainers(usedClips, _nullClipReferences);

                allClips.ExceptWith(usedClips);

                _unusedClips.Clear();
                _unusedClips.AddRange(allClips);
                _hasScanned = true;

                CustomLogger.Log($"Scan complete. {_unusedClips.Count} unused AudioClips found,"
                    + $" {_nullClipReferences.Count} empty clip slots in AudioContainers.", null);
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (originalSetup is
                    {
                        Length: > 0
                    })
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
            }
        }

        /// <summary>
        /// Draws the collapsed-by-default list of AudioContainers with empty clip slots.
        /// </summary>
        private void DrawNullClipReferences()
        {
            _showNullClipReferences = EditorGUILayout.Foldout(_showNullClipReferences,
                $"Empty clip slots ({_nullClipReferences.Count})", true);

            if (!_showNullClipReferences)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                if (_nullClipReferences.Count == 0)
                {
                    EditorGUILayout.HelpBox($"No empty {nameof(AudioContainer.Clips)} entries found.",
                        MessageType.Info);

                    return;
                }

                if (GUILayout.Button("Select Affected Containers"))
                    Selection.objects = _nullClipReferences
                        .Select(reference => (Object)reference.Container)
                        .Distinct()
                        .ToArray();

                foreach (NullClipReference reference in _nullClipReferences)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.ObjectField(reference.Container, typeof(AudioContainer), false);
                        EditorGUILayout.LabelField(reference.Describe());
                    }
                }
            }
        }

        private void DeleteUnusedClips()
        {
            bool confirmed = EditorUtility.DisplayDialog("Delete Unused Audio Clips",
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

        /// <summary>
        /// A single empty clip slot inside an AudioContainer.
        /// </summary>
        private readonly struct NullClipReference
        {
            /// <summary>
            /// Index used when the container holds no clips at all.
            /// </summary>
            public const int NoClipsIndex = -1;

            public readonly AudioContainer Container;
            private readonly int index;

            public NullClipReference(AudioContainer container, int index)
            {
                Container = container;
                this.index = index;
            }

            /// <summary>
            /// Returns a readable description of the empty slot.
            /// </summary>
            public string Describe() => index == NoClipsIndex
                ? $"{nameof(AudioContainer.Clips)} is empty"
                : $"{nameof(AudioContainer.Clips)}[{index}] is not assigned";
        }
    }
}