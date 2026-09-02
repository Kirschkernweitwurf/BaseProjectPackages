using System;
using System.Collections.Generic;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// Editor window that shows every prefab in the project as a variant tree, how far each variant drifted
    /// from its base, and which variants look redundant, overloaded, or too deeply chained.
    /// </summary>
    internal sealed class PrefabOverviewWindow : EditorWindow
    {
        private const float BadgeGap = 4f;
        private const float BadgeWidth = 34f;
        private const string BaseButtonLabel = "Base";
        private const float ButtonGap = 2f;
        private const float ButtonWidth = 52f;
        private const string CollapseButtonLabel = "Collapse All";
        private const float CollapseButtonWidth = 86f;
        private const float EdgePadding = 4f;
        private const string ExpandButtonLabel = "Expand All";
        private const float ExpandButtonWidth = 80f;
        private const float FilterFieldWidth = 90f;
        private const float IconSize = 16f;
        private const float IndentStep = 14f;
        private const float IssueBadgeWidth = 18f;
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Project Health/Prefab Overview";
        private const float MinLabelWidth = 60f;
        private const float MinWindowHeight = 340f;
        private const float MinWindowWidth = 620f;
        private const string OpenButtonLabel = "Open";
        private const float OverridesToggleWidth = 88f;
        private const float PackagesToggleWidth = 86f;
        private const float RowActionsWidth = 214f;
        private const float RowPadding = 3f;
        private const string ScanButtonLabel = "Scan Project";
        private const float SearchFieldHeight = 20f;
        private const float SearchFieldWidth = 200f;
        private const float ToolbarButtonHeight = 26f;
        private const float ToolbarButtonWidth = 150f;
        private const string WindowTitle = "Prefabs";

        private readonly Dictionary<string, bool> _foldouts = new();
        private readonly Dictionary<string, PrefabRowContent> _rowContent = new();
        private readonly HashSet<string> _visible = new();
        private readonly List<PrefabEntry> _entries = new();
        private readonly List<PrefabEntry> _roots = new();

        private Vector2 _scroll;
        private string _search = string.Empty;
        private EPrefabViewFilter _filter;
        private bool _analyzeOverrides = true;
        private bool _includePackages;
        private bool _overridesAnalyzed;
        private bool _hasScanned;
        private bool _pendingRescan;
        private bool _visibleDirty;
        private string _hoveredKey;
        private int _rowIndex;
        private int _variantCount;
        private int _baseCount;
        private int _issueCount;
        private int _deepestChain;

#region Unity Callbacks
        private void OnGUI()
        {
            OverviewGui.EnsureStyles();
            HandleMouseMove();

            DrawActionBar();
            DrawFilters();
            DrawSummary();
            DrawBody();
            ProcessPendingActions();
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            PrefabOverviewWindow window = GetWindow<PrefabOverviewWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private static void OpenPrefab(PrefabEntry entry)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.AssetPath);

            if (prefab == null)
                return;

            AssetDatabase.OpenAsset(prefab);
        }

        private void HandleMouseMove()
        {
            wantsMouseMove = true;

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        private void DrawActionBar()
        {
            EditorGUILayout.Space(EdgePadding);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(EdgePadding, false);

                if (GUILayout.Button(ScanButtonLabel,
                        GUILayout.Height(ToolbarButtonHeight),
                        GUILayout.Width(ToolbarButtonWidth)))
                    _pendingRescan = true;

                GUILayout.FlexibleSpace();

                EPrefabViewFilter filter =
                    (EPrefabViewFilter)EditorGUILayout.EnumPopup(_filter, GUILayout.Width(FilterFieldWidth));

                if (filter != _filter)
                {
                    _filter = filter;
                    _visibleDirty = true;
                }

                string search = EditorGUILayout.TextField(_search,
                    EditorStyles.toolbarSearchField,
                    GUILayout.Width(SearchFieldWidth),
                    GUILayout.Height(SearchFieldHeight));

                if (search != _search)
                {
                    _search = search;
                    _visibleDirty = true;
                }

                EditorGUILayout.Space(EdgePadding, false);
            }

            EditorGUILayout.Space(EdgePadding);
        }

        private void DrawFilters()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUIContent packagesContent = new(" Packages",
                    "Also scans prefabs inside packages. Base prefabs from packages are always pulled in. "
                    + "Applies on the next scan.");

                GUIContent overridesContent = new(" Overrides",
                    "Opens every variant to count its overrides, which makes the scan noticeably slower. "
                    + "Applies on the next scan.");

                _includePackages = GUILayout.Toggle(_includePackages, packagesContent,
                    GUILayout.Width(PackagesToggleWidth));

                _analyzeOverrides = GUILayout.Toggle(_analyzeOverrides, overridesContent,
                    GUILayout.Width(OverridesToggleWidth));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(ExpandButtonLabel, EditorStyles.miniButtonLeft,
                        GUILayout.Width(ExpandButtonWidth)))
                    SetAllFoldouts(true);

                if (GUILayout.Button(CollapseButtonLabel, EditorStyles.miniButtonRight,
                        GUILayout.Width(CollapseButtonWidth)))
                    SetAllFoldouts(false);
            }
        }

        private void DrawSummary()
        {
            if (!_hasScanned)
                return;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                string message = $"{_entries.Count} {OverviewGui.Plural(_entries.Count, "prefab", "prefabs")}, "
                    + $"{_variantCount} {OverviewGui.Plural(_variantCount, "variant", "variants")}, "
                    + $"{_baseCount} with variants, deepest chain {_deepestChain}, "
                    + $"{_issueCount} flagged.";

                GUILayout.Label(message, OverviewGui.HeaderStyle);
            }
        }

        private void DrawBody()
        {
            if (!_hasScanned)
            {
                OverviewGui.DrawHint("Press Scan Project to build the prefab overview.");
                return;
            }

            if (_entries.Count == 0)
            {
                OverviewGui.DrawHint("No prefabs found in the selected scope.");
                return;
            }

            if (_visibleDirty)
                RebuildVisible();

            if (_visible.Count == 0)
            {
                DrawEmptyResult();
                return;
            }

            DrawTree();
        }

        private void DrawEmptyResult()
        {
            if (_filter == EPrefabViewFilter.Issues
                && string.IsNullOrWhiteSpace(_search))
            {
                OverviewGui.DrawSuccess("No prefab issues", "No variant looks redundant, overloaded, or orphaned.");
                return;
            }

            OverviewGui.DrawHint("No prefabs match the current filter.");
        }

        private void RebuildVisible()
        {
            _visible.Clear();

            foreach (PrefabEntry root in _roots)
                MarkVisible(root);

            _visibleDirty = false;
        }

        // A prefab stays visible when it matches itself or when it carries a matching variant below it,
        // so that the chain leading to a match is never cut off.
        private bool MarkVisible(PrefabEntry entry)
        {
            bool childVisible = false;

            foreach (PrefabEntry child in entry.Children)
                childVisible |= MarkVisible(child);

            if (!childVisible
                && !Matches(entry))
                return false;

            _visible.Add(entry.Guid);

            return true;
        }

        private bool Matches(PrefabEntry entry)
        {
            if (!MatchesFilter(entry))
                return false;

            if (string.IsNullOrWhiteSpace(_search))
                return true;

            string term = _search.Trim();

            return entry.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                || entry.AssetPath.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool MatchesFilter(PrefabEntry entry)
        {
            switch (_filter)
            {
                case EPrefabViewFilter.Variants:
                    return entry.Kind == EPrefabKind.Variant;

                case EPrefabViewFilter.Issues:
                    return entry.Issues != EPrefabIssue.None;

                default:
                    return true;
            }
        }

        private void DrawTree()
        {
            _hoveredKey = null;
            _rowIndex = 0;

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (PrefabEntry root in _roots)
                DrawEntry(root, 0);

            EditorGUILayout.EndScrollView();
        }

        private void DrawEntry(PrefabEntry entry, int depth)
        {
            if (!_visible.Contains(entry.Guid))
                return;

            DrawRow(entry, depth);

            if (!IsExpanded(entry))
                return;

            foreach (PrefabEntry child in entry.Children)
                DrawEntry(child, depth + 1);
        }

        private void DrawRow(PrefabEntry entry, int depth)
        {
            // Bail out before reserving a rect, so that layout and repaint stay in sync.
            if (!_rowContent.TryGetValue(entry.Guid, out PrefabRowContent content))
                return;

            Rect rect = EditorGUILayout.GetControlRect(false, OverviewGui.RowHeight);
            bool even = _rowIndex % 2 == 0;
            _rowIndex++;

            if (rect.Contains(Event.current.mousePosition))
                _hoveredKey = entry.Guid;

            OverviewGui.DrawRowBackground(rect, entry.Guid == _hoveredKey, even);

            Rect foldoutRect = new(rect.x + depth * IndentStep + EdgePadding, rect.y + RowPadding, IconSize,
                IconSize);

            Rect iconRect = new(foldoutRect.xMax, rect.y + RowPadding, IconSize, IconSize);
            float labelX = iconRect.xMax + EdgePadding;
            float labelWidth = Mathf.Max(MinLabelWidth, rect.xMax - RowActionsWidth - labelX);
            Rect labelRect = new(labelX, rect.y, labelWidth, rect.height);

            DrawFoldout(entry, foldoutRect);
            GUI.Label(iconRect, content.Icon);
            GUI.Label(labelRect, content.Label, OverviewGui.PathStyle);

            DrawBadges(content, rect);
            DrawRowButtons(entry, rect);
            HandleRowClick(entry, labelRect);
        }

        private void DrawFoldout(PrefabEntry entry, Rect rect)
        {
            if (entry.Children.Count == 0)
                return;

            // While a search is running the tree is forced open, so the arrow would only fight the search.
            using (new EditorGUI.DisabledScope(!string.IsNullOrWhiteSpace(_search)))
            {
                bool expanded = IsExpanded(entry);
                bool result = GUI.Toggle(rect, expanded, GUIContent.none, EditorStyles.foldout);

                if (result != expanded)
                    _foldouts[entry.Guid] = result;
            }
        }

        private void DrawBadges(PrefabRowContent content, Rect rect)
        {
            float y = rect.y + RowPadding;
            float height = rect.height - RowPadding * 2f;
            float x = rect.xMax - RowActionsWidth + EdgePadding;

            if (content.VariantBadge != null)
                GUI.Label(new Rect(x, y, BadgeWidth, height), content.VariantBadge,
                    OverviewGui.BadgeStyle(EOverviewAccent.Neutral));

            // Each badge keeps its column even when it is missing, so the rows stay aligned.
            x += BadgeWidth + BadgeGap;

            if (content.OverrideBadge != null)
                GUI.Label(new Rect(x, y, BadgeWidth, height), content.OverrideBadge,
                    OverviewGui.BadgeStyle(content.OverrideAccent));

            x += BadgeWidth + BadgeGap;

            if (content.IssueBadge != null)
                GUI.Label(new Rect(x, y, IssueBadgeWidth, height), content.IssueBadge,
                    OverviewGui.BadgeStyle(EOverviewAccent.Warning));
        }

        private void DrawRowButtons(PrefabEntry entry, Rect rect)
        {
            float y = rect.y + RowPadding;
            float height = rect.height - RowPadding * 2f;
            Rect openRect = new(rect.xMax - ButtonWidth - EdgePadding, y, ButtonWidth, height);
            Rect baseRect = new(openRect.x - ButtonWidth - ButtonGap, y, ButtonWidth, height);

            using (new EditorGUI.DisabledScope(entry.BaseEntry == null))
            {
                if (GUI.Button(baseRect, BaseButtonLabel))
                    OverviewGui.Navigate(entry.BaseEntry.AssetPath);
            }

            if (GUI.Button(openRect, OpenButtonLabel))
                OpenPrefab(entry);
        }

        private void HandleRowClick(PrefabEntry entry, Rect labelRect)
        {
            if (Event.current.type != EventType.MouseDown
                || Event.current.button != 0)
                return;

            if (!labelRect.Contains(Event.current.mousePosition))
                return;

            if (Event.current.clickCount >= 2)
                OpenPrefab(entry);
            else
                OverviewGui.Navigate(entry.AssetPath);

            Event.current.Use();
        }

        // While a search is active the whole tree is opened, otherwise matches deeper down would stay hidden.
        private bool IsExpanded(PrefabEntry entry)
        {
            if (entry.Children.Count == 0)
                return false;

            if (!string.IsNullOrWhiteSpace(_search))
                return true;

            if (!_foldouts.TryGetValue(entry.Guid, out bool expanded))
                return true;

            return expanded;
        }

        private void SetAllFoldouts(bool expanded)
        {
            foreach (PrefabEntry entry in _entries)
            {
                if (entry.Children.Count > 0)
                    _foldouts[entry.Guid] = expanded;
            }
        }

        // Scanning changes how many controls the window draws, so it runs after the current layout pass.
        private void ProcessPendingActions()
        {
            if (!_pendingRescan)
                return;

            _pendingRescan = false;
            Rescan();
        }

        private void Rescan()
        {
            _entries.Clear();
            _roots.Clear();
            _foldouts.Clear();
            _rowContent.Clear();
            _visible.Clear();

            List<PrefabEntry> scanned = PrefabScanner.Scan(_includePackages, _analyzeOverrides);

            _entries.AddRange(scanned);
            _roots.AddRange(PrefabHierarchyBuilder.Build(scanned, _analyzeOverrides));
            _overridesAnalyzed = _analyzeOverrides;

            BuildRowContent();
            UpdateStatistics();

            _hasScanned = true;
            _visibleDirty = true;

            Repaint();
        }

        private void BuildRowContent()
        {
            foreach (PrefabEntry entry in _entries)
                _rowContent[entry.Guid] = new PrefabRowContent(entry, _overridesAnalyzed);
        }

        private void UpdateStatistics()
        {
            _variantCount = 0;
            _baseCount = 0;
            _issueCount = 0;
            _deepestChain = 0;

            foreach (PrefabEntry entry in _entries)
            {
                if (entry.Kind == EPrefabKind.Variant)
                    _variantCount++;

                if (entry.Children.Count > 0)
                    _baseCount++;

                if (entry.Issues != EPrefabIssue.None)
                    _issueCount++;

                _deepestChain = Mathf.Max(_deepestChain, entry.Depth);
            }
        }
    }
}