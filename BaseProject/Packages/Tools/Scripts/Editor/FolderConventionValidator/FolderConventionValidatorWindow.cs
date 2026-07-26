using System;
using System.Collections.Generic;
using System.Linq;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.FolderConventionValidator
{
    /// <summary>
    /// Editor window that checks the project folders against a <see cref="FolderConventionConfig"/>
    /// and lists every violation, with a one click fix for missing folders.
    /// </summary>
    public sealed class FolderConventionValidatorWindow : EditorWindow
    {
        private const float ActionWidth = 52f;
        private const float ButtonHeight = 26f;
        private const float ConfigWidth = 220f;
        private const float CreateButtonWidth = 140f;
        private const string CreateLabel = "Create";
        private const float FixWidth = 60f;
        private const string GoToLabel = "Go to";
        private const int HeaderFontSize = 12;
        private const float IconSize = 16f;
        private const float LabelOffset = 24f;
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Project Health/Folder Conventions";
        private const float PathShare = 0.45f;
        private const float RowHeight = 22f;
        private const float RowPadding = 3f;
        private const float RulesPanelHeight = 260f;
        private const float SearchWidth = 200f;
        private const float SuccessGap = 8f;
        private const float SuccessIconSize = 48f;
        private const int SuccessTitleFontSize = 15;
        private const string WindowTitle = "Folder Conventions";

        private static readonly Color EvenRowColor = new(0f, 0f, 0f, 0.06f);
        private static readonly Color HoverRowColor = new(0.35f, 0.55f, 0.95f, 0.18f);
        private static readonly Vector2 MinWindowSize = new(620f, 320f);

        [SerializeField] private FolderConventionConfig config;
        [SerializeField] private bool showRules = true;

        private readonly List<FolderViolation> _violations = new();

        private UnityEditor.Editor _configEditor;
        private FolderViolation _pendingFix;
        private GUIStyle _headerStyle;
        private GUIStyle _messageStyle;
        private GUIStyle _pathStyle;
        private GUIStyle _successSubtitleStyle;
        private GUIStyle _successTitleStyle;
        private Texture _successTexture;
        private Vector2 _rulesScroll;
        private Vector2 _scroll;
        private bool _hasScanned;
        private bool _pendingCreateConfig;
        private bool _pendingScan;
        private bool _stylesReady;
        private int _rowIndex;
        private string _hoveredKey;
        private string _search = string.Empty;

#region Unity Callbacks
        private void OnEnable()
        {
            wantsMouseMove = true;

            if (config == null)
                config = FindConfig();
        }

        private void OnGUI()
        {
            EnsureStyles();
            HandleMouseMove();
            DrawToolbar();

            if (config == null)
            {
                DrawMissingConfig();
            }
            else
            {
                DrawRules();
                DrawContent();
            }

            ProcessPendingActions();
        }

        private void OnDisable() => DestroyConfigEditor();
#endregion

        /// <summary>Opens or focuses the window from the Tools menu.</summary>
        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            FolderConventionValidatorWindow window = GetWindow<FolderConventionValidatorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = MinWindowSize;
            window.Show();
        }

        private static FolderConventionConfig FindConfig()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(FolderConventionConfig)}");

            return guids.Length == 0
                ? null
                : AssetDatabase.LoadAssetAtPath<FolderConventionConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static void Navigate(FolderViolation violation)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(violation.Path);

            if (asset == null)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static GUIContent GetIcon(EFolderViolationType type) => type switch
        {
            EFolderViolationType.MissingFolder => EditorGUIUtility.IconContent("console.erroricon.sml"),
            _ => EditorGUIUtility.IconContent("console.warnicon.sml")
        };

        private static string Plural(int amount, string singular, string plural) => amount == 1
            ? singular
            : plural;

        private static void DrawHint(string message)
        {
            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(message, EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }

            GUILayout.FlexibleSpace();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();

                config = (FolderConventionConfig)EditorGUILayout.ObjectField(config,
                    typeof(FolderConventionConfig),
                    false,
                    GUILayout.Width(ConfigWidth));

                if (EditorGUI.EndChangeCheck())
                    ResetResults();

                using (new EditorGUI.DisabledScope(config == null))
                {
                    if (GUILayout.Button("Scan", EditorStyles.toolbarButton, GUILayout.Width(ActionWidth)))
                        _pendingScan = true;
                }

                GUILayout.FlexibleSpace();

                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField,
                    GUILayout.Width(SearchWidth));
            }
        }

        private void DrawMissingConfig()
        {
            GUILayout.FlexibleSpace();

            EditorGUILayout.HelpBox($"Assign a {nameof(FolderConventionConfig)} or create one to define the "
                + "folder rules.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create Config", GUILayout.Height(ButtonHeight),
                        GUILayout.Width(CreateButtonWidth)))
                    _pendingCreateConfig = true;

                GUILayout.FlexibleSpace();
            }

            GUILayout.FlexibleSpace();
        }

        // Embeds the config inspector so the rules can be tuned without leaving the window.
        private void DrawRules()
        {
            showRules = EditorGUILayout.Foldout(showRules, "Rules", true);

            if (!showRules)
                return;

            EnsureConfigEditor();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _rulesScroll = EditorGUILayout.BeginScrollView(_rulesScroll,
                    GUILayout.MaxHeight(RulesPanelHeight));

                EditorGUI.BeginChangeCheck();
                _configEditor.OnInspectorGUI();

                // Rescan right away so the list always matches the rules on screen.
                if (EditorGUI.EndChangeCheck()
                    && _hasScanned)
                    _pendingScan = true;

                EditorGUILayout.EndScrollView();
            }
        }

        private void EnsureConfigEditor()
        {
            if (_configEditor != null
                && _configEditor.target == config)
                return;

            DestroyConfigEditor();
            _configEditor = UnityEditor.Editor.CreateEditor(config);
        }

        private void DestroyConfigEditor()
        {
            if (_configEditor == null)
                return;

            DestroyImmediate(_configEditor);
            _configEditor = null;
        }

        private void DrawContent()
        {
            List<FolderViolation> filtered = Filter();
            DrawSummary(filtered);
            DrawBody(filtered);
        }

        private void DrawSummary(List<FolderViolation> filtered)
        {
            if (!_hasScanned)
                return;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                GUILayout.Label(BuildSummary(filtered), _headerStyle);
        }

        private string BuildSummary(List<FolderViolation> filtered) => _violations.Count == filtered.Count
            ? $"{_violations.Count} {Plural(_violations.Count, "violation", "violations")} found."
            : $"{filtered.Count} of {_violations.Count} violations shown.";

        private void DrawBody(List<FolderViolation> filtered)
        {
            if (!_hasScanned)
            {
                DrawHint("Press Scan to check the project folders.");
                return;
            }

            if (_violations.Count == 0)
            {
                DrawSuccess("Folders look clean", "Every folder follows the configured conventions.");
                return;
            }

            if (filtered.Count == 0)
            {
                DrawHint("No results match the search.");
                return;
            }

            DrawResults(filtered);
        }

        private void DrawResults(List<FolderViolation> filtered)
        {
            _hoveredKey = null;
            _rowIndex = 0;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (FolderViolation violation in filtered)
                DrawRow(violation);

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(FolderViolation violation)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, RowHeight);
            string key = $"{violation.Path}:{violation.Type}";
            bool even = _rowIndex % 2 == 0;
            _rowIndex++;

            if (rect.Contains(Event.current.mousePosition))
                _hoveredKey = key;

            if (Event.current.type == EventType.Repaint)
            {
                if (key == _hoveredKey)
                    EditorGUI.DrawRect(rect, HoverRowColor);
                else if (even)
                    EditorGUI.DrawRect(rect, EvenRowColor);
            }

            Rect iconRect = new(rect.x + RowPadding, rect.y + RowPadding, IconSize, IconSize);
            GUI.Label(iconRect, GetIcon(violation.Type));

            // A missing folder cannot be pinged, so it offers Create instead of Go to.
            float actionsWidth = violation.IsFixable
                ? FixWidth
                : ActionWidth;

            float content = rect.width - LabelOffset - actionsWidth - RowPadding * 2f;
            float pathWidth = content * PathShare;

            Rect pathRect = new(rect.x + LabelOffset, rect.y, pathWidth, rect.height);
            Rect messageRect = new(pathRect.xMax + RowPadding, rect.y, content - pathWidth, rect.height);
            Rect actionRect = new(rect.xMax - actionsWidth - RowPadding, rect.y + RowPadding, actionsWidth,
                rect.height - RowPadding * 2f);

            GUI.Label(pathRect, new GUIContent(violation.Path, violation.Path), _pathStyle);
            GUI.Label(messageRect, new GUIContent(violation.Message, violation.Message), _messageStyle);
            DrawAction(violation, actionRect);

            if (violation.IsFixable)
                return;

            if (Event.current.type == EventType.MouseDown
                && pathRect.Contains(Event.current.mousePosition))
            {
                Navigate(violation);
                Event.current.Use();
            }
        }

        private void DrawAction(FolderViolation violation, Rect rect)
        {
            if (violation.IsFixable)
            {
                if (GUI.Button(rect, CreateLabel))
                    _pendingFix = violation;

                return;
            }

            if (GUI.Button(rect, GoToLabel))
                Navigate(violation);
        }

        private void DrawSuccess(string successTitle, string subtitle)
        {
            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                GUILayout.Label(new GUIContent(_successTexture),
                    GUILayout.Width(SuccessIconSize),
                    GUILayout.Height(SuccessIconSize));

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(SuccessGap);
            GUILayout.Label(successTitle, _successTitleStyle);
            GUILayout.Label(subtitle, _successSubtitleStyle);
            GUILayout.FlexibleSpace();
        }

        private List<FolderViolation> Filter()
        {
            if (string.IsNullOrWhiteSpace(_search))
                return new List<FolderViolation>(_violations);

            string term = _search.Trim();

            return _violations
                .Where(violation => violation.Path.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                    || violation.Message.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        // Actions are deferred so the layout does not change while the rows are being drawn.
        private void ProcessPendingActions()
        {
            if (_pendingCreateConfig)
            {
                _pendingCreateConfig = false;
                CreateConfig();
                return;
            }

            if (_pendingFix != null)
            {
                FolderViolation violation = _pendingFix;
                _pendingFix = null;

                // Creating assets during OnGUI breaks the layout, so it waits for the next editor tick.
                EditorApplication.delayCall += () => ApplyFix(violation);
                return;
            }

            if (!_pendingScan)
                return;

            _pendingScan = false;
            Rescan();
        }

        private void ApplyFix(FolderViolation violation)
        {
            // The window can be closed before the delayed call runs.
            if (this == null)
                return;

            if (!FolderConventionScanner.CreateFolder(violation.Path))
                return;

            Rescan();
        }

        private void CreateConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Folder Convention Config",
                nameof(FolderConventionConfig),
                "asset",
                "Choose where the folder rules are stored.");

            if (string.IsNullOrEmpty(path))
                return;

            FolderConventionConfig created = CreateInstance<FolderConventionConfig>();
            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.SaveAssets();

            config = created;
            ResetResults();
        }

        private void Rescan()
        {
            _violations.Clear();
            _violations.AddRange(FolderConventionScanner.Scan(config));
            _hasScanned = true;
            Repaint();
        }

        private void ResetResults()
        {
            DestroyConfigEditor();
            _violations.Clear();
            _hasScanned = false;
            Repaint();
        }

        private void HandleMouseMove()
        {
            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        private void EnsureStyles()
        {
            if (_stylesReady)
                return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = HeaderFontSize
            };

            _pathStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            _messageStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            _successTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = SuccessTitleFontSize
            };

            _successSubtitleStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            _successTexture = EditorGUIUtility.IconContent("console.infoicon").image;
            _stylesReady = true;
        }
    }
}