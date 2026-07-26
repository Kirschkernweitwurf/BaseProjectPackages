using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.NamingConventions.Data;
using Base.ToolPackage.Editor.NamingConventions.Renaming;
using Base.ToolPackage.Editor.NamingConventions.Scanning;
using Base.ToolPackage.MenuManagerWindow;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>
    /// Lists every asset that breaks the project naming conventions and renames it on the spot.
    /// The rules live in an <see cref="AssetNamingRuleSet"/> asset, so they are versioned with the
    /// project, they can be read from the assets that already exist with a single button, and they
    /// stay editable in the rule table afterwards. Rules, Dismissed, History and Scan Results are
    /// collapsible sections, each with its own accent color, and every applied rename lands in the
    /// clearable History.
    /// </summary>
    public sealed class AssetNamingWindow : EditorWindow
    {
        private const float ButtonWidth = 58f;
        private const float DetectWidth = 84f;
        private const float DismissWidth = 56f;
        private const float FieldInset = 2f;
        private const float GoToWidth = 46f;
        private const float MinimumHeight = 420f;
        private const float MinimumWidth = 900f;
        private const float NameWidth = 200f;
        private const float ReasonWidth = 155f;
        private const float RenameAllWidth = 76f;
        private const float RenameWidth = 60f;
        private const float RestoreWidth = 60f;
        private const string RenameArrow = " -> ";
        private const float RuleWidth = 95f;
        private const float SearchWidth = 160f;
        private const float SectionGap = 6f;
        private const float SmallIconSize = 16f;
        private const float SuggestionMinWidth = 180f;
        private const string SuggestionControlPrefix = "AssetNamingSuggestion";
        private const float TimeWidth = 110f;
        private const string WindowTitle = "Asset Naming";

        private static readonly GUIContent ClearHistoryContent = new("Clear", "Drop the whole rename history");

        private static readonly GUIContent ClearDismissedContent = new("Clear",
            "Bring every dismissed asset back into the scan");

        private static readonly GUIContent CreateContent = new("Create Rule Set",
            "Create the rule set asset so the conventions are versioned with the project");

        private static readonly GUIContent DetectContent = new("Auto-Detect",
            "Read the conventions from the assets that already exist and overwrite the rules");

        private static readonly GUIContent DismissContent = new("Dismiss",
            "Take this asset out of the scan. It moves to the Dismissed section and can be restored.");

        private static readonly GUIContent GoToContent = new("Go To", "Ping and select the asset in the Project view");

        private static readonly GUIContent NewNameHeader = new("New Name",
            "Suggested replacement. Edit it freely, then press Rename or Enter.");

        private static readonly GUIContent ReasonHeader = new("Reason", "Why the current name was rejected");

        private static readonly GUIContent RenameAllContent = new("Rename All",
            "Apply every suggestion in the current list");

        private static readonly GUIContent RenameContent = new("Rename", "Apply the suggested file name");
        private static readonly GUIContent RestoreContent = new("Restore", "Bring the asset back into the scan");
        private static readonly GUIContent RuleHeader = new("Rule", "The rule the asset was checked against");
        private static readonly GUIContent ScanContent = new("Scan", "Scan the project for violations");

        private readonly List<AssetNamingViolation> _all = new();
        private readonly List<AssetNamingViolation> _filtered = new();

        [SerializeField] private bool showDismissed;
        [SerializeField] private bool showFragments;
        [SerializeField] private bool showHistory;
        [SerializeField] private bool showResults = true;
        [SerializeField] private bool showRules = true;

        private AssetNamingRuleSet _ruleSet;
        private AssetNamingViolation _pendingRename;
        private AssetNamingViolation _pendingDismiss;
        private List<string> _dismissedPaths;
        private string _pendingRestoreGuid = string.Empty;
        private string _search = string.Empty;
        private int _pendingRuleRemoval = AssetNamingRuleGui.NoIndex;
        private int _pendingFragmentRemoval = AssetNamingRuleGui.NoIndex;
        private bool _isAddRulePending;
        private bool _isAddFragmentPending;
        private bool _isClearDismissedPending;
        private bool _isClearHistoryPending;
        private bool _isRenameAllPending;
        private bool _needsScan;
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            _ruleSet = AssetNamingRuleSet.Load();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_ruleSet == null)
            {
                DrawMissingRuleSet();
                return;
            }

            if (_needsScan)
                Rescan();

            EditorGUIUtility.SetIconSize(new Vector2(SmallIconSize, SmallIconSize));

            DrawRulesSection();
            EditorGUILayout.Space(SectionGap);
            DrawDismissedSection();
            DrawHistorySection();
            DrawResultsSection();

            EditorGUIUtility.SetIconSize(Vector2.zero);
            ApplyPending();
        }
#endregion

        /// <summary>Opens or focuses the window from the Tools menu.</summary>
        [DynamicMenuItem("Tools/Base Packages/Assets/Asset Naming Conventions")]
        private static void Open()
        {
            AssetNamingWindow window = GetWindow<AssetNamingWindow>(WindowTitle);

            window.minSize = new Vector2(MinimumWidth, MinimumHeight);
            window.Show();
        }

        private static void PingAsset(string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static GUIContent NameContent(AssetNamingViolation violation)
            => new(violation.CurrentName, AssetDatabase.GetCachedIcon(violation.AssetPath), violation.AssetPath);

        private static bool IsSubmitted(string controlName)
        {
            if (Event.current.type != EventType.KeyDown)
                return false;

            if (Event.current.keyCode != KeyCode.Return
                && Event.current.keyCode != KeyCode.KeypadEnter)
                return false;

            return GUI.GetNameOfFocusedControl() == controlName;
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                using (new EditorGUI.DisabledScope(_ruleSet == null))
                {
                    if (GUILayout.Button(ScanContent, EditorStyles.toolbarButton, GUILayout.Width(ButtonWidth)))
                        _needsScan = true;

                    if (GUILayout.Button(DetectContent, EditorStyles.toolbarButton, GUILayout.Width(DetectWidth)))
                        DetectConventions();

                    using (new EditorGUI.DisabledScope(_filtered.Count == 0))
                    {
                        if (GUILayout.Button(RenameAllContent, EditorStyles.toolbarButton,
                                GUILayout.Width(RenameAllWidth)))
                            _isRenameAllPending = true;
                    }

                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginChangeCheck();
                    _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField,
                        GUILayout.MinWidth(SearchWidth));

                    if (EditorGUI.EndChangeCheck())
                        RunQuery();
                }
            }
        }

        private void DrawMissingRuleSet()
        {
            EditorGUILayout.HelpBox("No asset rule set found. Create one so the conventions are versioned with "
                + "the project, then press Auto-Detect to read the conventions the assets already follow.",
                MessageType.Info);

            if (GUILayout.Button(CreateContent, GUILayout.Width(NameWidth)))
                _ruleSet = AssetNamingRuleSet.Create();
        }

        private void DrawRulesSection()
        {
            showRules = AssetNamingGui.DrawSectionHeader(showRules, "Rules", _ruleSet.Rules.Count,
                AssetNamingGui.RulesAccent);

            if (!showRules)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                showFragments = AssetNamingRuleGui.DrawOptions(_ruleSet, showFragments,
                    out bool isAddFragmentRequested, out int fragmentRemovalIndex);

                if (isAddFragmentRequested)
                    _isAddFragmentPending = true;

                if (fragmentRemovalIndex != AssetNamingRuleGui.NoIndex)
                    _pendingFragmentRemoval = fragmentRemovalIndex;

                EditorGUILayout.Space(SectionGap);

                int ruleRemovalIndex = AssetNamingRuleGui.DrawRules(_ruleSet);

                if (ruleRemovalIndex != AssetNamingRuleGui.NoIndex)
                    _pendingRuleRemoval = ruleRemovalIndex;

                if (AssetNamingRuleGui.DrawAddButton())
                    _isAddRulePending = true;
            }
        }

        private void DrawDismissedSection()
        {
            _dismissedPaths ??= BuildDismissedPaths();

            if (_dismissedPaths.Count == 0)
                return;

            List<string> visible = FilterDismissed();

            showDismissed = AssetNamingGui.DrawSectionHeader(showDismissed, "Dismissed", visible.Count,
                AssetNamingGui.DismissedAccent);

            if (!showDismissed)
                return;

            float height = visible.Count * AssetNamingGui.RowHeight;
            Rect area = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));

            for (int index = 0; index < visible.Count; index++)
            {
                Rect row = new(area.x, area.y + index * AssetNamingGui.RowHeight, area.width,
                    AssetNamingGui.RowHeight);
                DrawDismissedRow(row, index, visible[index]);
            }

            if (GUILayout.Button(ClearDismissedContent, GUILayout.Width(ButtonWidth)))
                _isClearDismissedPending = true;
        }

        private List<string> FilterDismissed()
        {
            if (string.IsNullOrWhiteSpace(_search))
                return _dismissedPaths;

            List<string> visible = new();

            foreach (string path in _dismissedPaths)
            {
                if (!path.Contains(_search, StringComparison.OrdinalIgnoreCase))
                    continue;

                visible.Add(path);
            }

            return visible;
        }

        private void DrawDismissedRow(Rect row, int index, string path)
        {
            AssetNamingGui.DrawRowBackground(row, index);

            float padding = AssetNamingGui.Padding;
            float cursor = row.x + padding;

            Rect nameRect = new(cursor, row.y, NameWidth, row.height);
            cursor += NameWidth + padding;

            float pathWidth = row.xMax - cursor - GoToWidth - RestoreWidth - padding * 3f;
            Rect pathRect = new(cursor, row.y, pathWidth, row.height);
            cursor += pathWidth + padding;

            Rect goToRect = new(cursor, row.y + FieldInset, GoToWidth, row.height - FieldInset * 2f);
            cursor += GoToWidth + padding;

            Rect restoreRect = new(cursor, row.y + FieldInset, RestoreWidth, row.height - FieldInset * 2f);

            GUIContent name = new(Path.GetFileNameWithoutExtension(path), AssetDatabase.GetCachedIcon(path), path);

            GUI.Label(nameRect, name, AssetNamingGui.NameStyle);
            GUI.Label(pathRect, path, AssetNamingGui.DetailStyle);

            if (GUI.Button(goToRect, GoToContent, EditorStyles.miniButton))
                PingAsset(path);

            if (GUI.Button(restoreRect, RestoreContent, EditorStyles.miniButton))
                _pendingRestoreGuid = AssetDatabase.AssetPathToGUID(path);
        }

        private void DrawHistorySection()
        {
            if (AssetNamingHistoryStore.Count == 0)
                return;

            IReadOnlyList<AssetNamingHistoryEntry> entries = AssetNamingHistoryStore.Entries;

            showHistory = AssetNamingGui.DrawSectionHeader(showHistory, "History", entries.Count,
                AssetNamingGui.HistoryAccent);

            if (!showHistory)
                return;

            float height = entries.Count * AssetNamingGui.RowHeight;
            Rect area = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));

            for (int index = 0; index < entries.Count; index++)
            {
                Rect row = new(area.x, area.y + index * AssetNamingGui.RowHeight, area.width,
                    AssetNamingGui.RowHeight);
                DrawHistoryRow(row, index, entries[index]);
            }

            if (GUILayout.Button(ClearHistoryContent, GUILayout.Width(ButtonWidth)))
                _isClearHistoryPending = true;

            EditorGUILayout.Space(SectionGap);
        }

        private void DrawHistoryRow(Rect row, int index, AssetNamingHistoryEntry entry)
        {
            AssetNamingGui.DrawRowBackground(row, index);

            float padding = AssetNamingGui.Padding;
            float cursor = row.x + padding;

            float renameWidth = row.xMax - cursor - TimeWidth - GoToWidth - padding * 3f;
            Rect renameRect = new(cursor, row.y, renameWidth, row.height);
            cursor += renameWidth + padding;

            Rect timeRect = new(cursor, row.y, TimeWidth, row.height);
            cursor += TimeWidth + padding;

            Rect goToRect = new(cursor, row.y + FieldInset, GoToWidth, row.height - FieldInset * 2f);

            GUIContent rename = new(entry.oldName + RenameArrow + entry.newName, entry.assetPath);

            GUI.Label(renameRect, rename, AssetNamingGui.NameStyle);
            GUI.Label(timeRect, entry.time, AssetNamingGui.DetailStyle);

            if (GUI.Button(goToRect, GoToContent, EditorStyles.miniButton))
                PingAsset(entry.assetPath);
        }

        private void DrawResultsSection()
        {
            showResults = AssetNamingGui.DrawSectionHeader(showResults, "Scan Results", _filtered.Count,
                AssetNamingGui.ResultsAccent);

            if (!showResults)
                return;

            if (_ruleSet.Rules.Count == 0)
            {
                EditorGUILayout.HelpBox("The rule set is empty. Press Auto-Detect to read the conventions from "
                    + "the project, or add a rule by hand.", MessageType.Info);
                return;
            }

            if (_all.Count == 0)
            {
                EditorGUILayout.HelpBox("Nothing scanned yet. Press Scan to check the project.", MessageType.None);
                return;
            }

            if (_filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No violations match the current search.", MessageType.Info);
                return;
            }

            DrawColumnHeader();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            float height = _filtered.Count * AssetNamingGui.RowHeight;
            Rect area = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));

            DrawVisibleRows(area);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>Draws the column titles above the list, aligned with the row layout.</summary>
        private void DrawColumnHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, AssetNamingGui.RowHeight, GUILayout.ExpandWidth(true));
            GUIStyle style = EditorStyles.miniBoldLabel;
            float padding = AssetNamingGui.Padding;
            float cursor = rect.x + padding;

            GUI.Label(new Rect(cursor, rect.y, NameWidth, rect.height), new GUIContent("Asset",
                "Current file name. Press Go To to find it in the Project view."), style);
            cursor += NameWidth + padding;

            GUI.Label(new Rect(cursor, rect.y, RuleWidth, rect.height), RuleHeader, style);
            cursor += RuleWidth + padding;

            GUI.Label(new Rect(cursor, rect.y, ReasonWidth, rect.height), ReasonHeader, style);
            cursor += ReasonWidth + padding;

            GUI.Label(new Rect(cursor, rect.y, NameWidth, rect.height), NewNameHeader, style);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(0f, 0f, 0f, 0.3f));
        }

        /// <summary>Draws only the rows inside the scroll view, so long lists stay responsive.</summary>
        private void DrawVisibleRows(Rect area)
        {
            float rowHeight = AssetNamingGui.RowHeight;
            int first = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / rowHeight) - 1);
            int visible = Mathf.CeilToInt(position.height / rowHeight) + 2;
            int last = Mathf.Min(_filtered.Count, first + visible);

            for (int index = first; index < last; index++)
            {
                Rect row = new(area.x, area.y + index * rowHeight, area.width, rowHeight);
                DrawRow(row, index);
            }
        }

        private void DrawRow(Rect row, int index)
        {
            AssetNamingViolation violation = _filtered[index];

            AssetNamingGui.DrawRowBackground(row, index);

            float padding = AssetNamingGui.Padding;
            float cursor = row.x + padding;

            Rect nameRect = new(cursor, row.y, NameWidth, row.height);
            cursor += NameWidth + padding;

            Rect ruleRect = new(cursor, row.y, RuleWidth, row.height);
            cursor += RuleWidth + padding;

            Rect reasonRect = new(cursor, row.y, ReasonWidth, row.height);
            cursor += ReasonWidth + padding;

            float buttonsWidth = GoToWidth + RenameWidth + DismissWidth + padding * 3f;
            float suggestionWidth = Mathf.Max(SuggestionMinWidth, row.xMax - cursor - buttonsWidth - padding);
            Rect suggestionRect = new(cursor, row.y + FieldInset, suggestionWidth, row.height - FieldInset * 2f);
            cursor += suggestionWidth + padding;

            Rect renameRect = new(cursor, row.y + FieldInset, RenameWidth, row.height - FieldInset * 2f);
            cursor += RenameWidth + padding;

            Rect goToRect = new(cursor, row.y + FieldInset, GoToWidth, row.height - FieldInset * 2f);
            cursor += GoToWidth + padding;

            Rect dismissRect = new(cursor, row.y + FieldInset, DismissWidth, row.height - FieldInset * 2f);

            GUI.Label(nameRect, NameContent(violation), AssetNamingGui.NameStyle);
            GUI.Label(ruleRect, violation.RuleLabel, AssetNamingGui.DetailStyle);
            GUI.Label(reasonRect, violation.Reason, AssetNamingGui.DetailStyle);

            string controlName = SuggestionControlPrefix + index;
            GUI.SetNextControlName(controlName);
            violation.Suggestion = EditorGUI.TextField(suggestionRect, violation.Suggestion);

            if (GUI.Button(goToRect, GoToContent, EditorStyles.miniButton))
                PingAsset(violation.AssetPath);

            if (GUI.Button(dismissRect, DismissContent, EditorStyles.miniButton))
                _pendingDismiss = violation;

            bool isRenameClicked = GUI.Button(renameRect, RenameContent, EditorStyles.miniButton);

            if (!isRenameClicked
                && !IsSubmitted(controlName))
                return;

            _pendingRename = violation;
        }

        private List<string> BuildDismissedPaths()
        {
            List<string> paths = new();

            foreach (string guid in AssetNamingDismissStore.GetAll())
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(path))
                    continue;

                paths.Add(path);
            }

            paths.Sort(StringComparer.Ordinal);

            return paths;
        }

        private void Rescan()
        {
            _needsScan = false;
            _all.Clear();
            _all.AddRange(AssetNamingScanner.Scan(_ruleSet));
            RunQuery();
        }

        private void RunQuery()
        {
            _filtered.Clear();

            foreach (AssetNamingViolation violation in _all)
            {
                if (AssetNamingDismissStore.IsDismissed(violation.Guid))
                    continue;

                if (!IsMatchingSearch(violation))
                    continue;

                _filtered.Add(violation);
            }
        }

        private bool IsMatchingSearch(AssetNamingViolation violation)
        {
            if (string.IsNullOrWhiteSpace(_search))
                return true;

            return violation.AssetPath.Contains(_search, StringComparison.OrdinalIgnoreCase);
        }

        private void DetectConventions()
        {
            List<AssetNamingRule> detected = AssetConventionDetector.Detect(_ruleSet);

            if (detected.Count == 0)
            {
                CustomLogger.LogWarning("No clear convention found. Add the rules by hand instead.", _ruleSet);
                return;
            }

            bool isReplacing = EditorUtility.DisplayDialog(WindowTitle,
                $"Found conventions for {detected.Count} asset kind(s). Replace the current rules with them?",
                "Replace", "Cancel");

            if (!isReplacing)
                return;

            _ruleSet.ReplaceRules(detected);
            _ruleSet.Persist();
            showRules = true;
            _needsScan = true;
        }

        /// <summary>
        /// Applies everything that changes the number of drawn controls. Doing it after the layout
        /// pass keeps IMGUI from reporting a mismatch between its layout and its repaint.
        /// </summary>
        private void ApplyPending()
        {
            if (ApplyRuleEdits())
                return;

            if (ApplyStoreEdits())
                return;

            ApplyRenames();
        }

        private bool ApplyRuleEdits()
        {
            if (_isAddRulePending)
            {
                _isAddRulePending = false;
                _ruleSet.AddRule(new AssetNamingRule());
                _ruleSet.Persist();
                return true;
            }

            if (_pendingRuleRemoval != AssetNamingRuleGui.NoIndex)
            {
                _ruleSet.RemoveRuleAt(_pendingRuleRemoval);
                _pendingRuleRemoval = AssetNamingRuleGui.NoIndex;
                _ruleSet.Persist();
                return true;
            }

            if (_isAddFragmentPending)
            {
                _isAddFragmentPending = false;
                _ruleSet.AddIgnoredFragment(string.Empty);
                _ruleSet.Persist();
                return true;
            }

            if (_pendingFragmentRemoval != AssetNamingRuleGui.NoIndex)
            {
                _ruleSet.RemoveIgnoredFragmentAt(_pendingFragmentRemoval);
                _pendingFragmentRemoval = AssetNamingRuleGui.NoIndex;
                _ruleSet.Persist();
                return true;
            }

            return false;
        }

        private bool ApplyStoreEdits()
        {
            if (_pendingDismiss != null)
            {
                AssetNamingDismissStore.Dismiss(_pendingDismiss.Guid);
                _pendingDismiss = null;
                _dismissedPaths = null;
                RunQuery();
                Repaint();
                return true;
            }

            if (_pendingRestoreGuid.Length > 0)
            {
                AssetNamingDismissStore.Restore(_pendingRestoreGuid);
                _pendingRestoreGuid = string.Empty;
                _dismissedPaths = null;
                RunQuery();
                Repaint();
                return true;
            }

            if (_isClearDismissedPending)
            {
                _isClearDismissedPending = false;
                AssetNamingDismissStore.Clear();
                _dismissedPaths = null;
                RunQuery();
                Repaint();
                return true;
            }

            if (_isClearHistoryPending)
            {
                _isClearHistoryPending = false;
                AssetNamingHistoryStore.Clear();
                Repaint();
                return true;
            }

            return false;
        }

        private void ApplyRenames()
        {
            if (_isRenameAllPending)
            {
                _isRenameAllPending = false;
                CustomLogger.Log($"Renamed {AssetRenamer.RenameAll(_filtered)} asset(s).", _ruleSet);
                _needsScan = true;
                return;
            }

            if (_pendingRename == null)
                return;

            AssetNamingViolation violation = _pendingRename;
            _pendingRename = null;

            if (!AssetRenamer.Rename(violation))
                return;

            _all.Remove(violation);
            _filtered.Remove(violation);
            Repaint();
        }
    }
}
