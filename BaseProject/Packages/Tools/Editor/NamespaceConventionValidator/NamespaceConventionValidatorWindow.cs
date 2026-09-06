using System;
using System.Collections.Generic;
using System.Linq;
using Base.ToolsPackage.Editor.Shared;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Overview = Base.ToolsPackage.Editor.OverviewGui.OverviewGui;

namespace Base.ToolsPackage.Editor.NamespaceConventionValidator
{
    /// <summary>
    /// Editor window that checks the scripts below a folder against a
    /// <see cref="NamespaceConventionConfig"/> and lists every namespace that does not read as the
    /// folder it sits in.
    /// </summary>
    internal sealed class NamespaceConventionValidatorWindow : EditorWindow
    {
        private const float ActionWidth = 52f;
        private const float ButtonHeight = 26f;
        private const float ConfigWidth = 220f;
        private const float CreateButtonWidth = 140f;
        private const string GoToLabel = "Go to";
        private const float IconSize = 16f;
        private const float LabelOffset = 24f;
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Project Health/Namespace Conventions";
        private const float PathShare = 0.45f;
        private const float RowHeight = 22f;
        private const float RowPadding = 3f;
        private const float RulesPanelHeight = 260f;
        private const float SearchWidth = 200f;
        private const string WindowTitle = "Namespace Conventions";

        private static readonly Color EvenRowColor = new(0f, 0f, 0f, 0.06f);
        private static readonly Color HoverRowColor = new(0.35f, 0.55f, 0.95f, 0.18f);
        private static readonly Vector2 MinWindowSize = new(620f, 320f);

        [SerializeField] private NamespaceConventionConfig config;
        [SerializeField] private bool showRules = true;

        private readonly List<NamespaceViolation> _violations = new();

        private UnityEditor.Editor _configEditor;
        private Vector2 _rulesScroll;
        private Vector2 _scroll;
        private bool _hasScanned;
        private bool _pendingCreateConfig;
        private bool _pendingScan;
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
            Overview.EnsureStyles();
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
            NamespaceConventionValidatorWindow window = GetWindow<NamespaceConventionValidatorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = MinWindowSize;
            window.Show();
        }

        private static NamespaceConventionConfig FindConfig()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(NamespaceConventionConfig)}");

            return guids.Length == 0
                ? null
                : AssetDatabase.LoadAssetAtPath<NamespaceConventionConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static void Navigate(NamespaceViolation violation)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(violation.Path);

            if (asset == null)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static GUIContent GetIcon(ENamespaceViolationType type) => type switch
        {
            ENamespaceViolationType.MissingNamespace => EditorGUIUtility.IconContent("console.erroricon.sml"),
            _ => EditorGUIUtility.IconContent("console.warnicon.sml")
        };

        private static string Plural(int amount, string singular, string plural) => amount == 1
            ? singular
            : plural;

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();

                config = (NamespaceConventionConfig)EditorGUILayout.ObjectField(config,
                    typeof(NamespaceConventionConfig),
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

            EditorGUILayout.HelpBox($"Assign a {nameof(NamespaceConventionConfig)} or create one to pick the "
                + "folder the namespaces are measured from.", MessageType.Info);

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
            List<NamespaceViolation> filtered = Filter();
            DrawSummary(filtered);
            DrawBody(filtered);
        }

        private void DrawSummary(List<NamespaceViolation> filtered)
        {
            if (!_hasScanned)
                return;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                GUILayout.Label(BuildSummary(filtered), Overview.HeaderStyle);
        }

        private string BuildSummary(List<NamespaceViolation> filtered) => _violations.Count == filtered.Count
            ? $"{_violations.Count} {Plural(_violations.Count, "violation", "violations")} found."
            : $"{filtered.Count} of {_violations.Count} violations shown.";

        private void DrawBody(List<NamespaceViolation> filtered)
        {
            if (!_hasScanned)
            {
                Overview.DrawHint("Press Scan to check the namespaces below the root folder.");
                return;
            }

            if (_violations.Count == 0)
            {
                Overview.DrawSuccess("Namespaces look clean", "Every script reads as the folder it sits in.");
                return;
            }

            if (filtered.Count == 0)
            {
                Overview.DrawHint("No results match the search.");
                return;
            }

            DrawResults(filtered);
        }

        private void DrawResults(List<NamespaceViolation> filtered)
        {
            _hoveredKey = null;
            _rowIndex = 0;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (NamespaceViolation violation in filtered)
                DrawRow(violation);

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(NamespaceViolation violation)
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

            float content = rect.width - LabelOffset - ActionWidth - RowPadding * 2f;
            float pathWidth = content * PathShare;

            Rect pathRect = new(rect.x + LabelOffset, rect.y, pathWidth, rect.height);
            Rect messageRect = new(pathRect.xMax + RowPadding, rect.y, content - pathWidth, rect.height);
            Rect actionRect = new(rect.xMax - ActionWidth - RowPadding, rect.y + RowPadding, ActionWidth,
                rect.height - RowPadding * 2f);

            GUI.Label(pathRect, new GUIContent(violation.Path, violation.Path), Overview.PathStyle);
            GUI.Label(messageRect, new GUIContent(violation.Message, violation.Message), Overview.DetailStyle);

            if (GUI.Button(actionRect, GoToLabel))
                Navigate(violation);

            if (Event.current.type == EventType.MouseDown
                && pathRect.Contains(Event.current.mousePosition))
            {
                Navigate(violation);
                Event.current.Use();
            }
        }

        private List<NamespaceViolation> Filter()
        {
            if (string.IsNullOrWhiteSpace(_search))
                return new List<NamespaceViolation>(_violations);

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

            if (!_pendingScan)
                return;

            _pendingScan = false;
            Rescan();
        }

        private void CreateConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Namespace Convention Config",
                nameof(NamespaceConventionConfig),
                "asset",
                "Choose where the namespace rules are stored.");

            if (string.IsNullOrEmpty(path))
                return;

            NamespaceConventionConfig created = CreateInstance<NamespaceConventionConfig>();
            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.SaveAssets();

            config = created;
            ResetResults();
        }

        private void Rescan()
        {
            _violations.Clear();
            _violations.AddRange(NamespaceConventionScanner.Scan(config, AssetDatabaseIndex.Default));
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
    }
}