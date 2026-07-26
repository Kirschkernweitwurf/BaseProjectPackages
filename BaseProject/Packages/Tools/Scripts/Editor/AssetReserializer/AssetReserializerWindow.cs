using System.Collections.Generic;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Base.ToolPackage.Editor.AssetReserializer
{
    /// <summary>
    /// Rewrites assets with the current serializer so that a
    /// <see cref="FormerlySerializedAsAttribute"/> rename actually lands on disk. Scope the run to a
    /// few folders, check the count first, then commit the diff it produces.
    /// </summary>
    public sealed class AssetReserializerWindow : EditorWindow
    {
        private const float ButtonHeight = 28f;
        private const string ConfirmCancel = "Cancel";
        private const string ConfirmMessage = "This rewrites {0} asset(s) on disk using the current serializer.\n\n"
            + "The diff will be larger than the field rename alone. Make sure your work is committed first.";
        private const string ConfirmOk = "Reserialize";
        private const string ConfirmTitle = "Reserialize Assets";
        private const string MenuPath = "Tools/Base Packages/Assets/Reserialize Assets";
        private const float MinHeight = 340f;
        private const float MinWidth = 380f;
        private const string ScopeHint = "No folders listed, so the whole project is searched. "
            + "Add folders to keep the diff small.";
        private const string WindowTitle = "Reserialize Assets";

        [Tooltip("Folders to search. Leave empty to search the whole project.")]
        [SerializeField] private List<DefaultAsset> folders = new();

        [Tooltip("Include prefab assets in the run.")]
        [SerializeField] private bool includePrefabs = true;

        [Tooltip("Include scene assets in the run.")]
        [SerializeField] private bool includeScenes = true;

        [Tooltip("Include ScriptableObject assets in the run.")]
        [SerializeField] private bool includeScriptableObjects = true;

        private SerializedObject _serialized;
        private SerializedProperty _foldersProperty;
        private Vector2 _scroll;
        private string _result = string.Empty;

        /// <summary>The asset kinds currently ticked in the window.</summary>
        private EReserializeAssetKinds Kinds
        {
            get
            {
                EReserializeAssetKinds kinds = EReserializeAssetKinds.None;

                if (includePrefabs)
                    kinds |= EReserializeAssetKinds.Prefabs;

                if (includeScenes)
                    kinds |= EReserializeAssetKinds.Scenes;

                if (includeScriptableObjects)
                    kinds |= EReserializeAssetKinds.ScriptableObjects;

                return kinds;
            }
        }

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);

            _serialized = new SerializedObject(this);
            _foldersProperty = _serialized.FindProperty(nameof(folders));
        }

        private void OnGUI()
        {
            _serialized.Update();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawScope();
            EditorGUILayout.Space();
            DrawKinds();
            EditorGUILayout.Space();
            DrawActions();

            EditorGUILayout.EndScrollView();
            _serialized.ApplyModifiedProperties();
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            AssetReserializerWindow window = GetWindow<AssetReserializerWindow>();

            window.minSize = new Vector2(MinWidth, MinHeight);
            window.Show();
        }

        /// <summary>
        /// Returns the folders currently selected in the Project window.
        /// </summary>
        private static List<DefaultAsset> SelectedFolders()
        {
            List<DefaultAsset> selected = new();

            foreach (DefaultAsset folder in Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets))
            {
                if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(folder)))
                    selected.Add(folder);
            }

            return selected;
        }

        private void DrawScope()
        {
            EditorGUILayout.PropertyField(_foldersProperty, true);

            if (folders.Count == 0)
                EditorGUILayout.HelpBox(ScopeHint, MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Selected Folders"))
                    AddSelectedFolders();

                using (new EditorGUI.DisabledScope(folders.Count == 0))
                {
                    if (GUILayout.Button("Clear Folders"))
                        ClearFolders();
                }
            }
        }

        private void DrawKinds()
        {
            includePrefabs = EditorGUILayout.ToggleLeft("Include prefabs", includePrefabs);
            includeScenes = EditorGUILayout.ToggleLeft("Include scenes", includeScenes);
            includeScriptableObjects = EditorGUILayout.ToggleLeft("Include ScriptableObjects",
                includeScriptableObjects);
        }

        private void DrawActions()
        {
            using (new EditorGUI.DisabledScope(Kinds == EReserializeAssetKinds.None))
            {
                if (GUILayout.Button("Count Matching Assets"))
                    CountAssets();

                if (GUILayout.Button("Reserialize", GUILayout.Height(ButtonHeight)))
                    Reserialize();
            }

            if (!string.IsNullOrEmpty(_result))
                EditorGUILayout.HelpBox(_result, MessageType.Info);
        }

        private void AddSelectedFolders()
        {
            foreach (DefaultAsset folder in SelectedFolders())
            {
                if (!folders.Contains(folder))
                    folders.Add(folder);
            }

            _serialized.Update();
            _result = string.Empty;
        }

        private void ClearFolders()
        {
            folders.Clear();

            _serialized.Update();
            _result = string.Empty;
        }

        private void CountAssets()
        {
            int count = ReserializeRunner.CollectPaths(FolderPaths(), Kinds).Count;

            _result = count == 0
                ? "Nothing matched the current scope."
                : $"{count} asset(s) would be reserialized.";
        }

        private void Reserialize()
        {
            IReadOnlyList<string> paths = ReserializeRunner.CollectPaths(FolderPaths(), Kinds);

            if (paths.Count == 0)
            {
                _result = "Nothing matched the current scope.";
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(ConfirmTitle, string.Format(ConfirmMessage, paths.Count),
                ConfirmOk, ConfirmCancel);

            if (!confirmed)
                return;

            int count = ReserializeRunner.Run(paths);

            _result = $"Reserialized {count} asset(s). Review the diff before committing.";
        }

        /// <summary>
        /// Returns the project relative paths of every valid folder in the list.
        /// </summary>
        private List<string> FolderPaths()
        {
            List<string> paths = new(folders.Count);

            foreach (DefaultAsset folder in folders)
            {
                if (folder == null)
                    continue;

                string path = AssetDatabase.GetAssetPath(folder);

                if (AssetDatabase.IsValidFolder(path))
                    paths.Add(path);
            }

            return paths;
        }
    }
}