using System.Collections.Generic;
using System.Linq;
using Base.CorePackage.Audio;
using Base.EditorUiPackage;
using Base.UtilityPackage.Editor;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
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
        private const string ClipsHeaderFormat = "Unused Clips ({0})";
        private const string ContainersFolder = "Assets/ScriptableObjects/AudioContainer";
        private const string DeleteAllLabel = "Delete All";
        private const string Description = "Lists every AudioClip under the audio folder that nothing in "
            + "the build scenes, the prefabs or an AudioContainer refers to, and the container slots that "
            + "were left empty.";
        private const string EmptyHint = "Press Scan to read the scenes, prefabs and containers.";
        private const string EmptyMessage = "Nothing scanned yet";
        private const string MenuPath = "Tools/Base Packages/Assets/Audio/Unused Audio Clips";
        private const string NothingUnusedMessage = "No unused clips";
        private const string PrefabFilter = "t:Prefab";
        private const string RescanLabel = "Rescan";
        private const float ScanButtonHeight = 28f;
        private const string ScanLabel = "Scan for Unused Audio Clips";
        private const string SelectAllLabel = "Select All in Project";
        private const string SelectContainersLabel = "Select Affected Containers";
        private const string SlotsHeaderFormat = "Empty clip slots ({0})";
        private const string WindowTitle = "Unused Audio Clips";

        private readonly List<AudioClip> _unusedClips = new();
        private readonly List<NullClipReference> _nullClipReferences = new();
        private readonly EditorWindowStyles _styles = new();

        private Vector2 _scroll;
        private bool _hasScanned;
        private bool _showNullClipReferences;

#region Unity Callbacks
        private void OnGUI()
        {
            _styles.EnsureBuilt();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);

            if (EditorWindowChrome.PrimaryButton(_styles, _hasScanned
                    ? RescanLabel
                    : ScanLabel, GUILayout.Height(ScanButtonHeight)))
                ScanForUnusedAudioClips();

            if (!_hasScanned)
            {
                EditorWindowChrome.DrawEmptyState(_styles, EditorIcons.Script, EmptyMessage, EmptyHint);
                return;
            }

            EditorGUILayout.Space(EditorMetrics.SectionGap);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawNullClipReferences();

            EditorGUILayout.Space(EditorMetrics.SectionGap);

            DrawUnusedClips();

            EditorGUILayout.EndScrollView();
        }

        private void OnDisable() => _styles.Dispose();
#endregion

        /// <summary>Opens the window and scans immediately, so it never opens on an empty list.</summary>
        [DynamicMenuItem(MenuPath)]
        public static void ShowWindow()
        {
            FindUnusedAudioClips window = GetWindow<FindUnusedAudioClips>(WindowTitle);

            window.ScanForUnusedAudioClips();
        }

        private static HashSet<AudioClip> LoadAllClips()
        {
            if (!AssetDatabase.IsValidFolder(ClipsFolder))
            {
                CustomLogger.LogWarning($"Clips folder '{ClipsFolder}' does not exist. No clips will be found.", null);
                return new HashSet<AudioClip>();
            }

            return new HashSet<AudioClip>(AssetDatabaseUtility.LoadAll<AudioClip>(folders: new[]
            {
                ClipsFolder
            }));
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
            // Paths rather than loaded assets, so the progress bar can name the prefab it is on.
            List<string> paths = AssetDatabaseUtility.FindAssetPaths(PrefabFilter);

            for (int i = 0; i < paths.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Scanning Prefabs", paths[i], (float)i / paths.Count);

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
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

            List<AudioContainer> containers = AssetDatabaseUtility.LoadAll<AudioContainer>(folders: new[]
            {
                ContainersFolder
            });

            foreach (AudioContainer container in containers)
            {
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
        private void DrawUnusedClips()
        {
            EditorWindowChrome.DrawSectionHeader(_styles,
                string.Format(ClipsHeaderFormat, _unusedClips.Count));

            if (_unusedClips.Count == 0)
            {
                GUILayout.Label(NothingUnusedMessage, _styles.EmptyHint);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (EditorWindowChrome.SecondaryButton(_styles, SelectAllLabel))
                    Selection.objects = _unusedClips.Cast<Object>().ToArray();

                GUILayout.Space(EditorMetrics.TightGap);

                if (EditorWindowChrome.SecondaryButton(_styles, DeleteAllLabel))
                    DeleteUnusedClips();
            }

            EditorGUILayout.Space(EditorMetrics.TightGap);

            EditorWindowChrome.BeginCard(_styles);

            foreach (AudioClip clip in _unusedClips)
                EditorGUILayout.ObjectField(clip, typeof(AudioClip), false);

            EditorWindowChrome.EndCard();
        }

        private void DrawNullClipReferences()
        {
            _showNullClipReferences = EditorGUILayout.Foldout(_showNullClipReferences,
                string.Format(SlotsHeaderFormat, _nullClipReferences.Count), true);

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

                if (EditorWindowChrome.SecondaryButton(_styles, SelectContainersLabel))
                    Selection.objects = _nullClipReferences
                        .Select(reference => (Object)reference.Container)
                        .Distinct()
                        .ToArray();

                EditorGUILayout.Space(EditorMetrics.TightGap);

                EditorWindowChrome.BeginCard(_styles);

                for (int index = 0; index < _nullClipReferences.Count; index++)
                    DrawNullClipRow(_nullClipReferences[index], index);

                EditorWindowChrome.EndCard();
            }
        }

        private void DrawNullClipRow(NullClipReference reference, int index)
        {
            Rect row = EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorTableStyles.RowHeight));

            EditorRows.DrawRowBackground(row, index);

            EditorGUILayout.ObjectField(reference.Container, typeof(AudioContainer), false);
            GUILayout.Label(reference.Describe(), _styles.Detail);

            EditorGUILayout.EndHorizontal();
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

            /// <summary>The container the empty slot was found in.</summary>
            public readonly AudioContainer Container;

            private readonly int index;

            /// <summary>Records one empty clip slot.</summary>
            /// <param name="container">The container the slot belongs to.</param>
            /// <param name="index">
            /// The slot's position, or <see cref="NoClipsIndex"/> when the container holds no clips.
            /// </param>
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