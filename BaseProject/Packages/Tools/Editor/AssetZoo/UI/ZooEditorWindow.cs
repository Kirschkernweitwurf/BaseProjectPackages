using System;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.AssetZoo;
using Base.ToolsPackage.Editor.AssetZoo.Builder;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.ToolsPackage.Editor.AssetZoo.Generation;
using Base.ToolsPackage.Editor.AssetZoo.Overlay;
using Base.UtilityPackage.Editor;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// The Asset Zoo: drop a folder of art, press Build, and the Scene view flies to it, all of it standing on the
    /// floor at true size with its name next to it.
    /// </summary>
    /// <remarks>
    /// The sections run in the order the tool is used, and only what an artist needs every day is in view. Whatever
    /// is about file naming or the rare case sits folded under More. Build, Frame and Clear stay pinned under the
    /// scroll view, because they are what the window is reopened for.
    /// </remarks>
    internal sealed class ZooEditorWindow : EditorWindow
    {
        private const string AddZooLabel = "New…";
        private const float AddZooWidth = 56f;
        private const string BuildLabel = "Build Zoo";
        private const float ClearButtonWidth = 64f;
        private const string ClearLabel = "Clear";
        private const string ClearTooltip = "Remove the zoo from the scene.";
        private const string CollapseLabel = "Collapse All";
        private const string ConfigExtension = "asset";
        private const string CreateConfigMessage = "Where should the new zoo be saved?";
        private const string CreateConfigTitle = "New Saved Zoo";
        private const string DefaultConfigName = "ZC_ZooConfig";
        private const string DefaultPath = "Tools/Base Packages/Assets/Asset Zoo/Open Zoo Builder";
        private const string Description = "See your art side by side, at true size.";
        private const string DropBody = "Your art lands in the Scene view, sorted by folder, standing on the floor "
            + "at true size.";
        private const string DropTitle = "Drop a folder of art here";
        private const string ExpandLabel = "Expand All";
        private const float FrameButtonWidth = 64f;
        private const string FrameLabel = "Frame";
        private const string FrameTooltip = "Fly the Scene view to the whole zoo.";
        private const string GameSceneFormat = "{0} is a game scene. Saving it would ship the zoo.";
        private const string GroupsHeader = "Groups";
        private const string LastConfigKey = "Base.AssetZoo.LastConfigGuid";
        private const string LookHeader = "Look";
        private const float MainButtonHeight = 32f;
        private const float MinWindowHeight = 620f;
        private const float MinWindowWidth = 420f;
        private const string MoreFoldKey = "Base.AssetZoo.IsMoreOpen";
        private const string MoreHeader = "More";
        private const string NewConfigFormat = "ZC_{0}";
        private const string NotBuiltFormat = "{0} not built yet";
        private const string PlayModeTooltip = "Leave Play Mode to build.";
        private const string RebuildLabel = "Rebuild Zoo";
        private const string RescanLabel = "Rescan";
        private const string RescanTooltip = "Look for new art now. Building does this too.";
        private const string SavedZooLabel = "Saved Zoo";
        private const string SavedZooTooltip = "A saved zoo remembers the folders, groups and look.";
        private const string SceneFormat = "Builds into {0}";
        private const string SearchPlaceholder = "Find an asset or group";
        private const string ShowHeader = "What to Show";
        private const string StartOverBody = "This forgets your groups, their order and everything you hid, then "
            + "scans the folders again.";
        private const string StartOverCancel = "Cancel";
        private const string StartOverLabel = "Start Over…";
        private const string StartOverOk = "Start Over";
        private const string StartOverTitle = "Start this zoo over?";
        private const float SubButtonHeight = 22f;
        private const string SummaryFormat = "{0} assets in {1} groups";
        private const float ToggleButtonWidth = 94f;
        private const int ToggleColumns = 3;
        private const int ToggleRows = 2;
        private const string UntitledScene = "the open scene";
        private const string WindowIconName = "Prefab Icon";
        private const string WindowTabTitle = "Asset Zoo";
        private const string WindowTitle = "Asset Zoo Builder";

        private static readonly GUIContent[] ArrangeChoices =
        {
            new("Rows", "Rows of a set length, each asset taking only the room it needs."),
            new("Line", "All assets of a group in one long row."),
            new("Circle", "All assets of a group on a ring.")
        };

        private static readonly GUIContent[] GroupByChoices =
        {
            new("Folder", "One group per folder, the way the art is sorted already."),
            new("Name", "One group per first word of the asset's name, behind its prefix.")
        };

        private static readonly GUIContent[] GroupsChoices =
        {
            new("Blocks", "Blocks like a city map. The whole zoo stays roughly square."),
            new("Row", "All groups side by side."),
            new("Column", "All groups one behind the other.")
        };

        private static readonly GUIContent[] OrderChoices =
        {
            new("Small in Front", "Small assets in front and big ones behind them, so nothing hides."),
            new("As Listed", "The order of the list below.")
        };

        // In the order of EAlignmentMode, whose values old configs have saved.
        private static readonly GUIContent[] StandOnChoices =
        {
            new("Pivot", "The asset's own pivot stands on its place. Good for checking pivots."),
            new("Floor", "The bottom of what you see stands on the floor."),
            new("Center", "The middle of what you see stands on its place.")
        };

        private static readonly GUIContent ArrangeLabel = new("Arrange", "How the assets of a group stand.");
        private static readonly GUIContent FigureLabel = new("Scale Figure",
            "Stands at the start of every group, so everything can be judged against it. A person works best.");
        private static readonly GUIContent FloorLabel = new("Floor",
            "Builds a floor that fits the zoo, so the art has ground and shadows to stand on.");
        private static readonly GUIContent FloorMaterialLabel = new("Floor Material",
            "The material of the floor and of the boards decals land on. Empty uses the default.");
        private static readonly GUIContent GroupByLabel = new("Group By",
            "What decides which group an asset is in.");
        private static readonly GUIContent GroupGapLabel = new("Group Gap (m)", "Gap between two groups.");
        private static readonly GUIContent GroupNamesLabel = new("Group Names",
            "Draws group names and zones in the Scene view.");
        private static readonly GUIContent GroupsLabel = new("Groups Stand",
            "How the groups stand next to each other.");
        private static readonly GUIContent ItemGapLabel = new("Item Gap (m)", "Gap between two assets.");
        private static readonly GUIContent ModelsLabel = new("Models With Prefab",
            "Also show a model when a prefab already shows it. Off keeps every piece of art once.");
        private static readonly GUIContent NamesLabel = new("Names",
            "Draws asset names in the Scene view. They appear as you get closer and never overlap.");
        private static readonly GUIContent OrderLabel = new("Order", "In which order the assets of a group stand.");
        private static readonly GUIContent PerRowLabel = new("Per Row", "How many assets stand in one row.");
        private static readonly GUIContent RadiusLabel = new("Radius (m)",
            "Smallest radius of the ring. It grows when the assets would not fit.");
        private static readonly GUIContent RulerLabel = new("Ruler", "Draws a ruler at the start of every group.");
        private static readonly GUIContent SceneLabel = new("Zoo Scene",
            "The scene the zoo is built into. It opens by itself on build. Empty uses the open scene.");
        private static readonly GUIContent SizesLabel = new("Sizes",
            "Adds size and triangles to every name. Hovering a name always shows them.");
        private static readonly GUIContent StandOnLabel = new("Stand On",
            "Which point of an asset stands on its place.");

        [SerializeField] private ZooConfig config;

        private readonly ZooBuilder _builder = new();
        private readonly ZooWindowStyles _styles = new();

        private SerializedObject _serializedConfig;
        private SerializedProperty _generationProperty;
        private SerializedProperty _layoutProperty;
        private SerializedProperty _showProperty;
        private ZooGroupListView _list;
        private ZooSourcesView _sources;
        private ZooGenerationResult _lastResult;
        private Vector2 _scroll;
        private string _search = string.Empty;
        private bool _hasResult;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTabTitle, EditorIcons.Named(WindowIconName));
            wantsMouseMove = true;

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            ZooSceneIndex.Invalidated += OnZooChanged;

            if (config == null)
                LoadLastConfig();
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            if (Event.current.type == EventType.MouseMove
                || (ZooThumbnails.IsLoading && Event.current.type == EventType.Repaint))
                Repaint();

            DrawBackground();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description, false);

            DrawSavedZoo();

            // After the picker rather than before it, so picking a different zoo takes hold in the pass it was
            // picked in. Rebuilding first left the rest of the window drawing the old zoo for one more frame,
            // and then applying that stale copy over the new one.
            EnsureInitialized();

            if (_serializedConfig == null)
            {
                DrawWelcome();
                return;
            }

            _serializedConfig.Update();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawShow();
            DrawGroups();
            DrawLook();
            DrawMore();

            EditorGUILayout.EndScrollView();

            DrawActionBar();
            DrawFooter();

            if (_serializedConfig.ApplyModifiedProperties())
                OnSettingsChanged();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            ZooSceneIndex.Invalidated -= OnZooChanged;

            _styles.Dispose();
        }
#endregion

        /// <summary>
        /// Opens the builder on the saved zoo used last.
        /// </summary>
        [DynamicMenuItem(DefaultPath)]
        internal static void Open() => Open(null);

        /// <summary>Opens the builder with the given saved zoo.</summary>
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

        // Asked of every asset that is double-clicked. What was clicked is also what is selected, which saves
        // converting the id, a call Unity keeps changing between versions.
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is not ZooConfig saved)
                return false;

            Open(saved);

            return true;
        }

        private static int ClampToChoices(int value, int count) => Mathf.Clamp(value, 0, count - 1);

        private static void DrawEnumChoices(ZooWindowStyles styles, SerializedProperty property, GUIContent label,
            GUIContent[] choices)
        {
            int picked = ClampToChoices(property.enumValueIndex, choices.Length);
            property.enumValueIndex = ZooControls.Segmented(styles, label, choices, picked);
        }

        private static SerializedProperty Find(SerializedProperty parent, string name)
            => CustomEditorUtility.FindProp(parent, name);

        private void OnUndoRedoPerformed()
        {
            if (config == null)
                return;

            _builder.Arrange(config);
            Repaint();
        }

        private void OnZooChanged() => Repaint();

        private void OnSettingsChanged() => _builder.Arrange(config);

        private void EnsureInitialized()
        {
            if (config == null)
            {
                _serializedConfig = null;
                _list = null;
                _sources = null;

                return;
            }

            if (_serializedConfig != null
                && _serializedConfig.targetObject == config)
                return;

            _serializedConfig = new SerializedObject(config);
            _generationProperty = CustomEditorUtility.FindProp(_serializedConfig, nameof(ZooConfig.Generation));
            _layoutProperty = CustomEditorUtility.FindProp(_serializedConfig, nameof(ZooConfig.Layout));
            _showProperty = CustomEditorUtility.FindProp(_serializedConfig, nameof(ZooConfig.Show));

            _list = new ZooGroupListView(CustomEditorUtility.FindProp(_serializedConfig,
                nameof(ZooConfig.Categories)), config, OnSettingsChanged);

            _sources = new ZooSourcesView(Scan);
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

        private void DrawSavedZoo()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();

                config = EditorGUILayout.ObjectField(new GUIContent(SavedZooLabel, SavedZooTooltip), config,
                    typeof(ZooConfig), false) as ZooConfig;

                if (EditorGUI.EndChangeCheck())
                {
                    SaveLastConfig();

                    _hasResult = false;
                    _search = string.Empty;
                }

                if (GUILayout.Button(AddZooLabel, _styles.SecondaryButton, GUILayout.Width(AddZooWidth),
                        GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    CreateConfig(DefaultConfigName, null);
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        private void DrawWelcome()
        {
            Rect area = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            area.xMin += EditorMetrics.SectionGap;
            area.xMax -= EditorMetrics.SectionGap;
            area.yMax -= EditorMetrics.SectionGap;

            ZooControls.DrawRoundedOutline(area, ZooSourcesView.IsDraggingArt()
                ? EditorPalette.Accent
                : EditorPalette.Border, 1.5f, EditorMetrics.CardCornerRadius);

            Rect title = new(area.x, area.center.y - 34f, area.width, 22f);
            Rect body = new(area.x + 24f, title.yMax + 2f, area.width - 48f, 44f);

            GUI.Label(title, DropTitle, _styles.EmptyTitle);
            GUI.Label(body, DropBody, _styles.EmptyHint);

            HandleWelcomeDrop(area);
        }

        private void HandleWelcomeDrop(Rect area)
        {
            Event current = Event.current;

            if ((current.type != EventType.DragUpdated && current.type != EventType.DragPerform)
                || !area.Contains(current.mousePosition)
                || !ZooSourcesView.IsDraggingArt())
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                string[] dropped = DragAndDrop.paths;
                string first = dropped.Length > 0
                    ? System.IO.Path.GetFileNameWithoutExtension(dropped[0])
                    : string.Empty;

                CreateConfig(string.Format(NewConfigFormat, first), dropped);
                GUIUtility.ExitGUI();
            }

            current.Use();
        }

        private void DrawShow()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, ShowHeader);
            EditorWindowChrome.BeginCard(_styles);

            _sources.Draw(_styles, config);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            EditorGUI.BeginChangeCheck();

            DrawEnumChoices(_styles, Find(_generationProperty, nameof(AutoGenerateSettings.GroupBy)), GroupByLabel,
                GroupByChoices);

            bool isGroupingChanged = EditorGUI.EndChangeCheck();

            if (_hasResult)
            {
                EditorGUILayout.Space(EditorMetrics.ItemGap);
                ZooResultView.Draw(_styles, _lastResult);
            }

            EditorWindowChrome.EndCard();

            _sources.HandleDrop(GUILayoutUtility.GetLastRect(), config);

            if (isGroupingChanged)
            {
                _serializedConfig.ApplyModifiedProperties();
                ScanAndStartOver();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        private void DrawGroups()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, GroupsHeader);

            DrawGroupToolbar();

            _list.Draw(_styles, _search);

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        // Laid out from one rectangle rather than as a horizontal group, so the field and the button
        // are the same height. Unity's own search field carries a fixed height from the skin and
        // ignored the one the group gave it, which is what left the button standing taller.
        private void DrawGroupToolbar()
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

        private void DrawLook()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, LookHeader);
            EditorWindowChrome.BeginCard(_styles);

            SerializedProperty type = Find(_layoutProperty, nameof(LayoutSettings.Type));

            DrawEnumChoices(_styles, type, ArrangeLabel, ArrangeChoices);
            DrawArrangeDetails((ELayoutType)type.enumValueIndex);

            DrawEnumChoices(_styles, Find(_layoutProperty, nameof(LayoutSettings.Order)), OrderLabel, OrderChoices);
            DrawEnumChoices(_styles, Find(_layoutProperty, nameof(LayoutSettings.Alignment)), StandOnLabel,
                StandOnChoices);

            EditorGUILayout.Slider(Find(_layoutProperty, nameof(LayoutSettings.Spacing)), 0f, 5f, ItemGapLabel);
            EditorGUILayout.Slider(Find(_layoutProperty, nameof(LayoutSettings.CategorySpacing)), 0f, 20f,
                GroupGapLabel);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            DrawToggles();

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            DrawScaleFigure();

            EditorWindowChrome.EndCard();

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        private void DrawArrangeDetails(ELayoutType type)
        {
            if (type == ELayoutType.Rows)
                EditorGUILayout.IntSlider(Find(_layoutProperty, nameof(LayoutSettings.ItemsPerRow)), 1, 20,
                    PerRowLabel);

            if (type == ELayoutType.Circle)
                EditorGUILayout.Slider(Find(_layoutProperty, nameof(LayoutSettings.CircleRadius)), 0f, 50f,
                    RadiusLabel);
        }

        // Laid out from one rectangle, because a layout toggle asks for the width of a whole inspector field, and
        // a few of them side by side pushed the window into scrolling sideways.
        private void DrawToggles()
        {
            float height = EditorGUIUtility.singleLineHeight;
            Rect area = GUILayoutUtility.GetRect(0f, height * ToggleRows + EditorMetrics.TightGap,
                GUILayout.ExpandWidth(true));

            float width = area.width / ToggleColumns;

            DrawToggle(new Rect(area.x, area.y, width, height), nameof(ShowSettings.HasItemNames), NamesLabel);
            DrawToggle(new Rect(area.x + width, area.y, width, height), nameof(ShowSettings.HasSizes), SizesLabel);
            DrawToggle(new Rect(area.x + width * 2f, area.y, width, height), nameof(ShowSettings.HasGroupNames),
                GroupNamesLabel);

            float lower = area.y + height + EditorMetrics.TightGap;

            DrawToggle(new Rect(area.x, lower, width, height), nameof(ShowSettings.HasRuler), RulerLabel);
            DrawToggle(new Rect(area.x + width, lower, width, height), nameof(ShowSettings.HasFloor), FloorLabel);
        }

        private void DrawToggle(Rect rect, string field, GUIContent label)
        {
            SerializedProperty property = Find(_showProperty, field);
            property.boolValue = EditorGUI.ToggleLeft(rect, label, property.boolValue);
        }

        // Changing the figure means different objects in every group, so it is the one setting that builds.
        private void DrawScaleFigure()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(Find(_showProperty, nameof(ShowSettings.ScaleFigure)), FigureLabel);

            if (!EditorGUI.EndChangeCheck())
                return;

            _serializedConfig.ApplyModifiedProperties();

            if (ZooSceneIndex.Root != null)
                _builder.Build(config);
        }

        private void DrawMore()
        {
            bool isOpen = SessionState.GetBool(MoreFoldKey, false);
            bool isNowOpen = EditorGUILayout.Foldout(isOpen, MoreHeader, true);

            if (isNowOpen != isOpen)
                SessionState.SetBool(MoreFoldKey, isNowOpen);

            if (!isNowOpen)
                return;

            EditorWindowChrome.BeginCard(_styles);

            EditorGUILayout.PropertyField(CustomEditorUtility.FindProp(_serializedConfig, nameof(ZooConfig.ZooScene)),
                SceneLabel);

            DrawEnumChoices(_styles, Find(_layoutProperty, nameof(LayoutSettings.Groups)), GroupsLabel, GroupsChoices);

            EditorGUILayout.PropertyField(Find(_showProperty, nameof(ShowSettings.FloorMaterial)), FloorMaterialLabel);
            EditorGUILayout.PropertyField(Find(_generationProperty, nameof(AutoGenerateSettings.HasModelsWithPrefab)),
                ModelsLabel);
            EditorGUILayout.PropertyField(Find(_generationProperty, nameof(AutoGenerateSettings.SearchDepth)));

            if (config.Generation.GroupBy == EGroupBy.Name)
                DrawNameRules();

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent(RescanLabel, RescanTooltip), _styles.SecondaryButton,
                        GUILayout.Height(SubButtonHeight)))
                    Scan();

                if (GUILayout.Button(StartOverLabel, _styles.SecondaryButton, GUILayout.Height(SubButtonHeight))
                    && EditorUtility.DisplayDialog(StartOverTitle, StartOverBody, StartOverOk, StartOverCancel))
                    ScanAndStartOver();
            }

            EditorWindowChrome.EndCard();

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        private void DrawNameRules()
        {
            using (new EditorListTintScope())
                EditorGUILayout.PropertyField(Find(_generationProperty, nameof(AutoGenerateSettings.Prefixes)), true);

            EditorGUILayout.PropertyField(Find(_generationProperty, nameof(AutoGenerateSettings.CanDetectPrefixes)));
            EditorGUILayout.PropertyField(Find(_generationProperty, nameof(AutoGenerateSettings.MinPrefixOccurrences)));
            EditorGUILayout.PropertyField(Find(_generationProperty, nameof(AutoGenerateSettings.MaxPrefixLength)));
            EditorGUILayout.PropertyField(Find(_generationProperty, nameof(AutoGenerateSettings.Separator)));
            EditorGUILayout.PropertyField(Find(_generationProperty, nameof(AutoGenerateSettings.IsPrefixCaseIgnored)));
        }

        /// <summary>
        /// Build, Frame and Clear, pinned under the scroll view rather than scrolling away with the rest.
        /// </summary>
        private void DrawActionBar()
        {
            EditorGUILayout.Space(EditorMetrics.TightGap);

            Rect area = GUILayoutUtility.GetRect(0f, MainButtonHeight, GUILayout.ExpandWidth(true));

            Rect clear = new(area.xMax - ClearButtonWidth, area.y, ClearButtonWidth, area.height);
            Rect frame = new(clear.x - EditorMetrics.TightGap - FrameButtonWidth, area.y, FrameButtonWidth,
                area.height);
            Rect build = new(area.x, area.y, Mathf.Max(0f, frame.x - area.x - EditorMetrics.TightGap), area.height);

            bool hasZoo = ZooSceneIndex.Root != null;

            string buildLabel = hasZoo
                ? RebuildLabel
                : BuildLabel;

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                string tooltip = Application.isPlaying
                    ? PlayModeTooltip
                    : string.Empty;

                if (GUI.Button(build, new GUIContent(buildLabel, tooltip), _styles.PrimaryButton))
                    EditorApplication.delayCall += BuildZoo;
            }

            // Both of the others need a zoo in the scene to act on, so they share one disabled state rather than
            // the window offering an action that can only report that there is nothing.
            using (new EditorGUI.DisabledScope(!hasZoo || Application.isPlaying))
            {
                if (GUI.Button(frame, new GUIContent(FrameLabel, FrameTooltip), _styles.SecondaryButton))
                    ZooSceneNavigator.FlyToZoo();

                if (GUI.Button(clear, new GUIContent(ClearLabel, ClearTooltip), _styles.SecondaryButton))
                    _builder.Clear();
            }
        }

        private void DrawFooter()
        {
            int assets = 0;

            foreach (ZooCategory category in config.Categories)
                assets += category.Entries.Count;

            string left = string.Format(SummaryFormat, assets, config.Categories.Count);
            string line = left + " · " + DescribeTarget(assets);

            EditorWindowChrome.DrawFooter(_styles, line);
        }

        private string DescribeTarget(int assets)
        {
            int missing = CountNotBuilt(assets);

            if (ZooSceneIndex.Root != null
                && missing > 0)
                return string.Format(NotBuiltFormat, missing);

            if (config.ZooScene != null)
                return string.Format(SceneFormat, config.ZooScene.name);

            Scene scene = SceneManager.GetActiveScene();

            if (IsGameScene(scene))
                return string.Format(GameSceneFormat, scene.name);

            return string.Format(SceneFormat, string.IsNullOrEmpty(scene.name)
                ? UntitledScene
                : scene.name);
        }

        private static int CountNotBuilt(int assets)
        {
            int built = 0;

            foreach (ZooItem item in ZooSceneIndex.Items)
            {
                if (item != null
                    && !item.IsFigure)
                    built++;
            }

            return Mathf.Max(0, assets - built);
        }

        private static bool IsGameScene(Scene scene)
        {
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (buildScene.enabled
                    && string.Equals(buildScene.path, scene.path, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private void Scan()
        {
            _lastResult = ZooAutoGenerator.Generate(config);
            _hasResult = true;

            RefreshList();
        }

        private void ScanAndStartOver()
        {
            _lastResult = ZooAutoGenerator.Generate(config, true);
            _hasResult = true;

            RefreshList();
        }

        // The generator writes to the saved zoo directly, so the serialized view has to be pulled back in before
        // this pass applies its own, now stale, copy over the new groups.
        private void RefreshList()
        {
            _serializedConfig.Update();
            Repaint();
        }

        private void BuildZoo()
        {
            if (config == null
                || Application.isPlaying
                || !TryOpenZooScene())
                return;

            bool hadZoo = ZooSceneIndex.Root != null;

            _lastResult = ZooAutoGenerator.Generate(config);
            _hasResult = true;

            _builder.Build(config, hadZoo
                ? _lastResult.NewAssets
                : null);

            AssetDatabase.SaveAssetIfDirty(config);

            FlyAfterBuild(hadZoo);
            EnsureInitialized();
            _serializedConfig?.Update();
            Repaint();
        }

        // A first build shows the whole zoo. A rebuild leaves the camera where the artist was working, unless new
        // art arrived, because that is what they rebuilt to see.
        private void FlyAfterBuild(bool hadZoo)
        {
            if (!hadZoo)
            {
                ZooSceneNavigator.FlyToZoo();
                return;
            }

            if (_lastResult.NewAssets.Count == 0)
                return;

            GameObject first = _lastResult.NewAssets[0];

            foreach (ZooItem item in ZooSceneIndex.Items)
            {
                if (item != null
                    && item.Asset == first
                    && !item.IsFigure)
                {
                    ZooSceneNavigator.FlyToItem(item);
                    return;
                }
            }
        }

        private bool TryOpenZooScene()
        {
            if (config.ZooScene == null)
                return true;

            string path = AssetDatabase.GetAssetPath(config.ZooScene);

            if (SceneManager.GetActiveScene().path == path)
                return true;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ZooSceneIndex.Invalidate();

            return true;
        }

        private void CreateConfig(string suggestedName, string[] sources)
        {
            string path = EditorUtility.SaveFilePanelInProject(CreateConfigTitle, suggestedName, ConfigExtension,
                CreateConfigMessage);

            if (string.IsNullOrEmpty(path))
                return;

            ZooConfig created = CreateInstance<ZooConfig>();

            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.SaveAssets();

            config = created;
            SaveLastConfig();
            EnsureInitialized();

            if (sources == null)
                return;

            foreach (string source in sources)
                ZooAutoGenerator.TryAddSource(created, source);

            Scan();
        }

        private void LoadLastConfig()
        {
            string guid = EditorPrefs.GetString(LastConfigKey, string.Empty);
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!string.IsNullOrEmpty(path))
                config = AssetDatabase.LoadAssetAtPath<ZooConfig>(path);

            if (config != null)
                return;

            // First time on this machine. The team's saved zoo is right there in the project, so nobody should
            // have to go and find it.
            string[] saved = AssetDatabase.FindAssets($"t:{nameof(ZooConfig)}");

            if (saved.Length > 0)
                config = AssetDatabase.LoadAssetAtPath<ZooConfig>(AssetDatabase.GUIDToAssetPath(saved[0]));
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
    }
}