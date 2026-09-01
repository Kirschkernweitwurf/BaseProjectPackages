using Base.EditorUiPackage;
using Base.ToolPackage.Editor.AssetZoo.Builder;
using Base.ToolPackage.Editor.AssetZoo.Config;
using Base.ToolPackage.Editor.AssetZoo.Generation;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// Dockable window for quick zoo building. Tools &gt; Asset Zoo &gt; Open Zoo Builder.
    /// The last used config is remembered, so the next session is just open and generate.
    /// </summary>
    internal class ZooEditorWindow : EditorWindow
    {
        private const string AutoGenerateLabel = "Auto Generate Categories";
        private const float AuxButtonHeight = 24f;
        private const string BuildLabel = "Build Zoo";
        private const string ClearLabel = "Clear Zoo";
        private const string ConfigHeader = "Config";
        private const string ConfigLabel = "Config";

        private const string DefaultPath = "Tools/Base Packages/Assets/Asset Zoo/Open Zoo Builder";
        private const string LastConfigKey = "Base.AssetZoo.LastConfigGuid";
        private const float MainButtonHeight = 32f;
        private const float MinWindowHeight = 400f;
        private const float MinWindowWidth = 340f;
        private const string ParentLabel = "Parent (optional)";
        private const string SelectParentLabel = "Select Zoo Parent";
        private const string SelectRootLabel = "Select Zoo Root";
        private const string SetupHeader = "Setup";
        private const string StartHint = "1. Create a config via Assets > Create > Asset Zoo > Zoo Config.\n"
            + "2. Drop it in the Config field above.\n"
            + "3. Set the search folder under Generation, hit Auto Generate, hit Build.";
        private const string WindowTitle = "Asset Zoo Builder";
        private const string Description = "Builds a scene full of every asset a config points at, so a "
            + "whole library can be looked at side by side instead of one prefab at a time.";

        [SerializeField] private ZooConfig config;

        private readonly ZooBuilder _builder = new();
        private readonly EditorWindowStyles _styles = new();

        private Transform _parent;
        private Vector2 _scroll;
        private UnityEditor.Editor _cachedConfigEditor;
        private ZooGenerationResult _lastResult;
        private bool _hasResult;

#region Unity Callbacks
        private void OnEnable()
        {
            if (config == null)
                LoadLastConfig();
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);

            DrawSetup();
            DrawActions();

            if (config == null)
            {
                EditorGUILayout.HelpBox(StartHint, MessageType.Info);
                return;
            }

            DrawConfigEditor();

            EditorWindowChrome.DrawFooter(_styles, _hasResult
                ? _lastResult.Message
                : null);
        }

        private void OnDisable()
        {
            ClearCachedEditor();

            _styles.Dispose();
        }
#endregion

        /// <summary>
        /// Opens the zoo builder window without a config.
        /// </summary>
        [DynamicMenuItem(DefaultPath)]
        internal static void Open() => Open(null);

        /// <summary>Opens the builder window with the given config preselected.</summary>
        internal static void Open(ZooConfig config)
        {
            ZooEditorWindow window = GetWindow<ZooEditorWindow>("Asset Zoo");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);

            if (config == null)
                return;

            window.config = config;
            window.SaveLastConfig();
        }

        private void DrawSetup()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, SetupHeader);
            EditorWindowChrome.BeginCard(_styles);

            EditorGUI.BeginChangeCheck();

            config = EditorGUILayout.ObjectField(ConfigLabel, config, typeof(ZooConfig), false) as ZooConfig;
            _parent = EditorGUILayout.ObjectField(ParentLabel, _parent, typeof(Transform), true) as Transform;

            if (EditorGUI.EndChangeCheck())
            {
                ClearCachedEditor();
                SaveLastConfig();
                _hasResult = false;
            }

            EditorWindowChrome.EndCard();
        }

        private void DrawActions()
        {
            bool hasZoo = _builder.HasZoo;

            using (new EditorGUI.DisabledScope(config == null))
            {
                if (EditorWindowChrome.PrimaryButton(_styles, AutoGenerateLabel,
                        GUILayout.Height(MainButtonHeight)))
                    AutoGenerate();

                EditorGUILayout.Space(EditorMetrics.TightGap);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (EditorWindowChrome.PrimaryButton(_styles, BuildLabel,
                            GUILayout.Height(MainButtonHeight)))
                        _builder.Build(config, _parent);

                    GUILayout.Space(EditorMetrics.TightGap);

                    using (new EditorGUI.DisabledScope(!hasZoo))
                    {
                        if (EditorWindowChrome.SecondaryButton(_styles, ClearLabel,
                                GUILayout.Height(MainButtonHeight)))
                            _builder.Clear();
                    }
                }
            }

            EditorGUILayout.Space(EditorMetrics.TightGap);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!hasZoo))
                {
                    if (EditorWindowChrome.SecondaryButton(_styles, SelectRootLabel,
                            GUILayout.Height(AuxButtonHeight)))
                        SelectZooRoot();
                }

                GUILayout.Space(EditorMetrics.TightGap);

                using (new EditorGUI.DisabledScope(_parent == null))
                {
                    if (EditorWindowChrome.SecondaryButton(_styles, SelectParentLabel,
                            GUILayout.Height(AuxButtonHeight)))
                        SelectZooParent();
                }
            }

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        private void DrawConfigEditor()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, ConfigHeader);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EnsureCachedEditor();
            _cachedConfigEditor.OnInspectorGUI();

            EditorGUILayout.EndScrollView();
        }

        private void AutoGenerate()
        {
            _lastResult = ZooAutoGenerator.Generate(config);
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

            config = AssetDatabase.LoadAssetAtPath<ZooConfig>(path);
        }

        private void SaveLastConfig()
        {
            if (config == null)
            {
                EditorPrefs.DeleteKey(LastConfigKey);
                return;
            }

            string path = AssetDatabase.GetAssetPath(config);
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
                && _cachedConfigEditor.target == config)
                return;

            ClearCachedEditor();
            _cachedConfigEditor = UnityEditor.Editor.CreateEditor(config);
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