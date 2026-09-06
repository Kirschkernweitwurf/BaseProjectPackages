using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.AssetZoo.Builder;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.ToolsPackage.Editor.AssetZoo.Generation;
using Base.UtilityPackage.Editor;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// Dockable window for quick zoo building. Tools &gt; Asset Zoo &gt; Open Zoo Builder.
    /// The last used config is remembered, so the next session is just open and generate.
    /// </summary>
    /// <remarks>
    /// The sections run in the order the tool is used: pick a config, point generation at a folder
    /// and run it, check what came out, tune how it is laid out, build. Build and clear sit in a bar
    /// pinned under the scroll view, because they are what the window is reopened for and scrolling
    /// to reach them was the main thing wrong with the first layout.
    /// </remarks>
    internal class ZooEditorWindow : EditorWindow
    {
        private const string AppearanceHeader = "Appearance";
        private const string BuildLabel = "Build Zoo";
        private const float ClearButtonWidth = 84f;
        private const string ClearZooLabel = "Clear Zoo";
        private const string CollapseLabel = "Collapse All";
        private const string ConfigExtension = "asset";
        private const string ConfigLabel = "Config";
        private const float CreateButtonWidth = 160f;
        private const string CreateConfigLabel = "Create Config";
        private const string CreateConfigMessage = "Where should the new zoo config be saved?";
        private const string CreateConfigTitle = "Create Zoo Config";
        private const string DefaultConfigName = "ZC_ZooConfig";
        private const string DefaultPath = "Tools/Base Packages/Assets/Asset Zoo/Open Zoo Builder";
        private const string Description = "Builds a scene full of every asset a config points at, so a "
            + "whole library can be looked at side by side instead of one prefab at a time.";
        private const string ExpandLabel = "Expand All";
        private const string GenerateHeader = "Generate";
        private const string GenerateLabel = "Auto Generate Categories";
        private const string LastConfigKey = "Base.AssetZoo.LastConfigGuid";
        private const float MainButtonHeight = 30f;
        private const float MinWindowHeight = 620f;
        private const float MinWindowWidth = 480f;
        private const string NoConfigHint = "Pick a config above, or create one to start from scratch.";
        private const string NoConfigTitle = "No config selected";
        private const string ParentLabel = "Parent (optional)";
        private const string SearchPlaceholder = "Filter categories";
        private const float SelectRootButtonWidth = 104f;
        private const string SelectRootLabel = "Select Root";
        private const string SetupHeader = "Setup";
        private const float SubButtonHeight = 22f;
        private const float ToggleButtonWidth = 94f;
        private const string WindowIconName = "Prefab Icon";
        private const string WindowTabTitle = "Asset Zoo";
        private const string WindowTitle = "Asset Zoo Builder";

        [SerializeField] private ZooConfig config;

        private readonly ZooBuilder _builder = new();
        private readonly EditorWindowStyles _styles = new();

        private ZooCategoryListView _list;
        private SerializedObject _serializedConfig;
        private SerializedProperty _generationProperty;
        private SerializedProperty _labelsProperty;
        private SerializedProperty _layoutProperty;
        private Transform _parent;
        private Vector2 _scroll;
        private ZooGenerationResult _lastResult;
        private string _search = string.Empty;
        private bool _hasResult;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTabTitle, EditorIcons.Named(WindowIconName));

            if (config == null)
                LoadLastConfig();
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();
            HandleMouseMove();
            DrawBackground();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description, false);

            DrawSetup();

            // After the config field rather than before it, so picking a different config takes hold
            // in the pass it was picked in. Rebuilding first left the rest of the window drawing the
            // old config for one more frame, and then applying that stale copy over the new one.
            EnsureInitialized();

            if (_serializedConfig == null)
            {
                DrawNoConfig();
                return;
            }

            _serializedConfig.Update();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawGenerate();
            DrawCategories();
            DrawAppearance();

            EditorGUILayout.EndScrollView();

            DrawActionBar();
            EditorWindowChrome.DrawFooter(_styles, Summary());

            _serializedConfig.ApplyModifiedProperties();
        }

        private void OnDisable() => _styles.Dispose();
#endregion

        /// <summary>
        /// Opens the zoo builder window without a config.
        /// </summary>
        [DynamicMenuItem(DefaultPath)]
        internal static void Open() => Open(null);

        /// <summary>Opens the builder window with the given config preselected.</summary>
        internal static void Open(ZooConfig config)
        {
            ZooEditorWindow window = GetWindow<ZooEditorWindow>();
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();

            if (config == null)
                return;

            window.config = config;
            window.SaveLastConfig();
        }

        /// <summary>
        /// Rebuilds the serialized view when the window opens on a different config than the one it
        /// was left with, or when the config it held was deleted.
        /// </summary>
        private void EnsureInitialized()
        {
            if (config == null)
            {
                _serializedConfig = null;
                _list = null;

                return;
            }

            if (_serializedConfig != null
                && _serializedConfig.targetObject == config)
                return;

            _serializedConfig = new SerializedObject(config);
            _generationProperty = CustomEditorUtility.FindProp(_serializedConfig, nameof(ZooConfig.Generation));
            _labelsProperty = CustomEditorUtility.FindProp(_serializedConfig, nameof(ZooConfig.Labels));
            _layoutProperty = CustomEditorUtility.FindProp(_serializedConfig, nameof(ZooConfig.Layout));

            _list = new ZooCategoryListView(CustomEditorUtility.FindProp(_serializedConfig,
                nameof(ZooConfig.Categories)));
        }

        private void HandleMouseMove()
        {
            wantsMouseMove = true;

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        /// <summary>
        /// Fills the window with the theme background before anything else is drawn. Without it the
        /// editor's own grey shows wherever no card covers it, which banded the section headers.
        /// </summary>
        private void DrawBackground()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(new Rect(0f, 0f, position.width, position.height), EditorPalette.Background);
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
                SaveLastConfig();

                _hasResult = false;
                _search = string.Empty;
            }

            EditorWindowChrome.EndCard();

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        private void DrawNoConfig()
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label(NoConfigTitle, _styles.EmptyTitle);
            GUILayout.Label(NoConfigHint, _styles.EmptyHint);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (EditorWindowChrome.PrimaryButton(_styles, CreateConfigLabel,
                        GUILayout.Width(CreateButtonWidth), GUILayout.Height(MainButtonHeight)))
                    CreateConfig();

                GUILayout.FlexibleSpace();
            }

            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// The generation settings and the button that runs them, together. Splitting them meant
        /// setting the search folder at the bottom of the window and scrolling back up to run it.
        /// </summary>
        private void DrawGenerate()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, GenerateHeader);
            EditorWindowChrome.BeginCard(_styles);

            _generationProperty.isExpanded = true;

            // The prefix list inside is one of Unity's own, drawn by Unity, so the only way to get it
            // out of the built-in grey is to tint it where it is drawn.
            using (new EditorListTintScope())
                EditorGUILayout.PropertyField(_generationProperty, true);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            if (EditorWindowChrome.PrimaryButton(_styles, GenerateLabel, GUILayout.Height(MainButtonHeight)))
                AutoGenerate();

            EditorWindowChrome.EndCard();

            if (_hasResult)
            {
                EditorGUILayout.Space(EditorMetrics.TightGap);
                ZooResultView.Draw(_styles, _lastResult);
            }

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        private void DrawCategories()
        {
            DrawCategoryToolbar();

            _list.Draw(_styles, _search);

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        // Laid out from one rectangle rather than as a horizontal group, so the field and the button
        // are the same height. Unity's own search field carries a fixed height from the skin and
        // ignored the one the group gave it, which is what left the button standing taller.
        private void DrawCategoryToolbar()
        {
            Rect area = GUILayoutUtility.GetRect(0f, SubButtonHeight, GUILayout.ExpandWidth(true));

            Rect toggle = new(area.xMax - ToggleButtonWidth, area.y, ToggleButtonWidth, area.height);
            Rect search = new(area.x, area.y, Mathf.Max(0f, toggle.x - area.x - EditorMetrics.TightGap),
                area.height);

            _search = EditorSearchField.Draw(_styles, search, _search, SearchPlaceholder);

            using (new EditorGUI.DisabledScope(_list.Count == 0))
            {
                // One button that reverses itself, so a second press undoes the first rather than
                // needing a second button next to it.
                bool isAnyExpanded = _list.HasExpanded();

                string toggleLabel = isAnyExpanded
                    ? CollapseLabel
                    : ExpandLabel;

                if (GUI.Button(toggle, toggleLabel, _styles.SecondaryButton))
                    _list.SetAllExpanded(!isAnyExpanded);
            }

            EditorGUILayout.Space(EditorMetrics.TightGap);
        }

        private void DrawAppearance()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, AppearanceHeader);
            EditorWindowChrome.BeginCard(_styles);

            using (new EditorListTintScope())
            {
                EditorGUILayout.PropertyField(_layoutProperty, true);
                EditorGUILayout.PropertyField(_labelsProperty, true);
            }

            EditorWindowChrome.EndCard();

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        /// <summary>
        /// Build and clear, pinned under the scroll view rather than scrolling away with the rest.
        /// </summary>
        private void DrawActionBar()
        {
            EditorGUILayout.Space(EditorMetrics.TightGap);

            Rect area = GUILayoutUtility.GetRect(0f, MainButtonHeight, GUILayout.ExpandWidth(true));

            Rect clear = new(area.xMax - ClearButtonWidth, area.y, ClearButtonWidth, area.height);
            Rect select = new(clear.x - EditorMetrics.TightGap - SelectRootButtonWidth, area.y,
                SelectRootButtonWidth, area.height);

            Rect build = new(area.x, area.y, Mathf.Max(0f, select.x - area.x - EditorMetrics.TightGap),
                area.height);

            if (GUI.Button(build, BuildLabel, _styles.PrimaryButton))
                _builder.Build(config, _parent);

            // Both of the others need a zoo in the scene to act on, so they share one disabled state
            // rather than the window offering an action that can only report that there is nothing.
            using (new EditorGUI.DisabledScope(!_builder.HasZoo))
            {
                if (GUI.Button(select, SelectRootLabel, _styles.SecondaryButton))
                    SelectZooRoot();

                if (GUI.Button(clear, ClearZooLabel, _styles.SecondaryButton))
                    _builder.Clear();
            }
        }

        private void AutoGenerate()
        {
            _lastResult = ZooAutoGenerator.Generate(config);
            _hasResult = true;

            // The generator writes to the config directly, so the serialized view has to be pulled
            // back in before this pass applies its own, now stale, copy over the new categories.
            _serializedConfig.Update();
            _list.SetAllExpanded(false);
        }

        private void CreateConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(CreateConfigTitle, DefaultConfigName,
                ConfigExtension, CreateConfigMessage);

            if (string.IsNullOrEmpty(path))
                return;

            ZooConfig created = CreateInstance<ZooConfig>();

            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.SaveAssets();

            config = created;
            SaveLastConfig();
        }

        /// <summary>The line at the foot of the window, saying what the config currently holds.</summary>
        private string Summary() => $"{_list.Count} categories, {_list.EntryCount()} assets in {config.name}.";

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

        private void SelectZooRoot()
        {
            GameObject root = _builder.GetZooRoot();
            if (root == null)
                return;

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }
    }
}