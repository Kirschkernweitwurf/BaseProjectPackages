#if UNITY_EDITOR
using Base.ToolPackage.Editor.AssetZoo.Builder;
using Base.ToolPackage.Editor.AssetZoo.Config;
using Base.ToolPackage.Editor.AssetZoo.Generation;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// Dockable window for quick zoo building. Tools &gt; Asset Zoo &gt; Open Zoo Builder.
    /// The last used config is remembered, so the next session is just open and generate.
    /// </summary>
    public class ZooEditorWindow : EditorWindow
    {
        private const float AuxButtonHeight = 24f;
        private const float MainButtonHeight = 32f;
        private const float MinWindowHeight = 400f;
        private const float MinWindowWidth = 340f;
        private const string LastConfigKey = "Base.AssetZoo.LastConfigGuid";

        private readonly ZooBuilder _builder = new();

        [SerializeField] private ZooConfig _config;

        private Transform _parent;
        private Vector2 _scroll;
        private UnityEditor.Editor _cachedConfigEditor;
        private ZooGenerationResult _lastResult;
        private bool _hasResult;

#region Unity Callbacks
        private void OnEnable()
        {
            if (_config == null)
                LoadLastConfig();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Asset Zoo Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();
            _config = (ZooConfig)EditorGUILayout.ObjectField("Config", _config, typeof(ZooConfig), false);
            _parent = (Transform)EditorGUILayout.ObjectField("Parent (optional)", _parent, typeof(Transform), true);
            if (EditorGUI.EndChangeCheck())
            {
                ClearCachedEditor();
                SaveLastConfig();
                _hasResult = false;
            }

            EditorGUILayout.Space(6);

            bool hasZoo = _builder.HasZoo;
            bool hasParent = _parent != null;

            using (new EditorGUI.DisabledScope(_config == null))
            {
                if (GUILayout.Button("Auto Generate Categories", GUILayout.Height(MainButtonHeight)))
                    AutoGenerate();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Build Zoo", GUILayout.Height(MainButtonHeight)))
                        _builder.Build(_config, _parent);

                    using (new EditorGUI.DisabledScope(!hasZoo))
                    {
                        if (GUILayout.Button("Clear Zoo", GUILayout.Height(MainButtonHeight)))
                            _builder.Clear();
                    }
                }
            }

            using (new EditorGUI.DisabledScope(!hasZoo))
            {
                if (GUILayout.Button("Select Zoo Root", GUILayout.Height(AuxButtonHeight)))
                    SelectZooRoot();
            }

            using (new EditorGUI.DisabledScope(!hasParent))
            {
                if (GUILayout.Button("Select Zoo Parent", GUILayout.Height(AuxButtonHeight)))
                    SelectZooParent();
            }

            EditorGUILayout.Space(6);

            if (_config == null)
            {
                EditorGUILayout.HelpBox("1. Create a config via Assets > Create > Asset Zoo > Zoo Config.\n"
                    + "2. Drop it in the Config field above.\n"
                    + "3. Set the search folder under Generation, hit Auto Generate, hit Build.",
                    MessageType.Info);

                return;
            }

            if (_hasResult)
            {
                EditorGUILayout.HelpBox(_lastResult.Message,
                    _lastResult.Success
                        ? MessageType.Info
                        : MessageType.Warning);
            }

            EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EnsureCachedEditor();
            _cachedConfigEditor.OnInspectorGUI();

            EditorGUILayout.EndScrollView();
        }

        private void OnDisable() => ClearCachedEditor();
#endregion

        /// <summary>
        /// Opens the zoo builder window without a config.
        /// </summary>
        [DynamicMenuItem("Tools/Base Packages/Asset Zoo/Open Zoo Builder")]
        public static void Open() => Open(null);

        public static void Open(ZooConfig config)
        {
            ZooEditorWindow window = GetWindow<ZooEditorWindow>("Asset Zoo");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);

            if (config == null)
                return;

            window._config = config;
            window.SaveLastConfig();
        }

        private void AutoGenerate()
        {
            _lastResult = ZooAutoGenerator.Generate(_config);
            _hasResult = true;
        }

        private void LoadLastConfig()
        {
            string guid = EditorPrefs.GetString(LastConfigKey, string.Empty);
            if (string.IsNullOrEmpty(guid))
                return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return;

            _config = AssetDatabase.LoadAssetAtPath<ZooConfig>(path);
        }

        private void SaveLastConfig()
        {
            if (_config == null)
            {
                EditorPrefs.DeleteKey(LastConfigKey);
                return;
            }

            string path = AssetDatabase.GetAssetPath(_config);
            if (string.IsNullOrEmpty(path))
                return;

            EditorPrefs.SetString(LastConfigKey, AssetDatabase.AssetPathToGUID(path));
        }

        private void EnsureCachedEditor()
        {
            // Rebuild the cached inspector, if it was never created,
            // its underlying target object was destroyed (asset deleted),
            // it's pointing at a different config than the one currently selected
            if (_cachedConfigEditor != null
                && _cachedConfigEditor.target != null
                && _cachedConfigEditor.target == _config)
                return;

            ClearCachedEditor();
            _cachedConfigEditor = UnityEditor.Editor.CreateEditor(_config);
        }

        private void ClearCachedEditor()
        {
            if (_cachedConfigEditor != null)
                DestroyImmediate(_cachedConfigEditor);

            _cachedConfigEditor = null;
        }

        private void SelectZooRoot()
        {
            GameObject root = _builder.GetZooRoot();
            if (root == null)
                return;

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        private void SelectZooParent()
        {
            if (_parent == null)
                return;

            Selection.activeGameObject = _parent.gameObject;
            EditorGUIUtility.PingObject(_parent);
        }
    }
}
#endif
