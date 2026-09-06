using System.Collections.Generic;
using System.IO;
using Base.ToolsPackage.Editor.NamingConventions.Data;
using Base.ToolsPackage.Editor.NamingConventions.Scanning;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.NamingConventions.Window
{
    /// <summary>
    /// Lists every asset that breaks the project naming conventions and renames it on the spot.
    /// The rules live in an <see cref="AssetNamingRuleSet"/> asset, so they are versioned with the
    /// project, they can be read from the assets that already exist with a single button, and they
    /// stay editable in the rule table afterwards. Rules, Dismissed, Scan Results and History are
    /// collapsible sections inside one shared scroll view, each with its own accent color. Every
    /// rename, dismiss and restore lands in the clearable History.
    /// <para/>
    /// The window only draws. <see cref="AssetNamingQuery"/> owns the results and the filters,
    /// <see cref="AssetNamingEdits"/> owns every change that is deferred past the layout pass.
    /// </summary>
    internal sealed class AssetNamingWindow : EditorWindow
    {
        private const float ButtonWidth = 58f;
        private const float DetectWidth = 84f;
        private const float DismissWidth = 56f;
        private const float FieldInset = 2f;
        private const float FilterWidth = 110f;
        private const float GoToWidth = 46f;
        private const float MinimumHeight = 300f;
        private const float MinimumWidth = 460f;
        private const string PrefsKey = "Base.AssetNaming.ResultTable";
        private const float RenameAllWidth = 76f;
        private const float RenameWidth = 60f;
        private const float RestoreWidth = 60f;
        private const float SearchWidth = 130f;
        private const float SmallIconSize = 16f;
        private const float SortWidth = 108f;
        private const string SuggestionControlPrefix = "AssetNamingSuggestion";
        private const float TimeWidth = 110f;
        private const float UndoWidth = 46f;
        private const string WindowTitle = "Asset Naming";
        private static readonly AssetNamingColumnLayout Columns = new(PrefsKey, 170f, 190f, 85f, 150f, 150f);
        private static readonly GUIContent ClearDismissedContent = new("Clear",
            "Bring every dismissed asset back into the scan");

        private static readonly GUIContent ClearHistoryContent = new("Clear", "Drop the whole history");

        private static readonly GUIContent CreateContent = new("Create Rule Set",
            "Create the rule set asset so the conventions are versioned with the project");

        private static readonly GUIContent DetectContent = new("Auto-Detect",
            "Read the conventions from the assets that already exist and overwrite the rules");

        private static readonly GUIContent DismissContent = new("Dismiss",
            "Take this asset out of the scan. It moves to the Dismissed section and can be restored.");

        private static readonly GUIContent GoToContent = new("Go To", "Ping and select the asset in the Project view");

        private static readonly GUIContent[] Headers =
        {
            new("Asset", "Current file name. Press Go To to find it in the Project view."),
            new("Path", "Folder the asset lives in"),
            new("Rule", "The rule the asset was checked against"),
            new("Reason", "Why the current name was rejected"),
            new("New Name", "Suggested replacement. Edit it freely, then press Rename or Enter.")
        };

        private static readonly GUIContent RenameAllContent = new("Rename All",
            "Apply every suggestion in the current list");

        private static readonly GUIContent RenameContent = new("Rename", "Apply the suggested file name");
        private static readonly GUIContent RestoreContent = new("Restore", "Bring the asset back into the scan");
        private static readonly GUIContent ScanContent = new("Scan", "Scan the project for violations");

        private static readonly string[] SortLabels =
        {
            "Group: Folder",
            "Flat: Name",
            "Group: Rule"
        };

        private static readonly GUIContent UndoContent = new("Undo",
            "Take this back. A rename is renamed again, a dismiss is restored and the entry disappears.");

        [SerializeField] private AssetNamingQuery query = new();
        [SerializeField] private bool showDismissed;
        [SerializeField] private bool showFragments;
        [SerializeField] private bool showHistory;
        [SerializeField] private bool showResults = true;
        [SerializeField] private bool showRules = true;

        private readonly AssetNamingEdits _edits = new();
        private readonly Dictionary<string, bool> _collapsedGroups = new();

        private AssetNamingRuleSet _ruleSet;
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
            AssetNamingGui.EnsureFresh();

            DrawToolbar();

            if (_ruleSet == null)
            {
                DrawMissingRuleSet();
                return;
            }

            if (_needsScan)
                Rescan();

            EditorGUIUtility.SetIconSize(new Vector2(SmallIconSize, SmallIconSize));

            // One scroll view for everything, so a tall rule table pushes the results down instead
            // of fighting them for the available height.
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawRulesSection();
            DrawDismissedSection();
            DrawResultsSection();
            DrawHistorySection();

            EditorGUILayout.EndScrollView();
            EditorGUIUtility.SetIconSize(Vector2.zero);

            ApplyPending();
        }

        private void OnFocus() => query.InvalidateDismissed();
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

        private static GUIContent NameContent(AssetNamingViolation violation) => new(violation.CurrentName,
            AssetDatabase.GetCachedIcon(violation.AssetPath), violation.AssetPath);

        private static string DescribeAction(AssetNamingHistoryEntry entry) => entry.action switch
        {
            EAssetNamingAction.Renamed => $"{entry.oldName} was renamed to {entry.newName}",
            EAssetNamingAction.Dismissed => $"{entry.oldName} was dismissed",
            _ => $"{entry.oldName} was restored"
        };

        private static bool IsSubmitted(string controlName)
        {
            if (Event.current.type != EventType.KeyDown)
                return false;

            if (Event.current.keyCode != KeyCode.Return
                && Event.current.keyCode != KeyCode.KeypadEnter)
                return false;

            return GUI.GetNameOfFocusedControl() == controlName;
        }

        /// <summary>Folder of an asset, shown in its own column so grouping is not the only hint.</summary>
        private static string FolderOf(string assetPath)
        {
            string folder = Path.GetDirectoryName(assetPath);

            return string.IsNullOrEmpty(folder)
                ? string.Empty
                : folder.Replace('\\', '/');
        }

        private static Rect ReserveRows(int count)
            => GUILayoutUtility.GetRect(0f, count * AssetNamingGui.RowHeight, GUILayout.ExpandWidth(true));

        private static Rect RowRect(Rect area, int index) => new(area.x, area.y + index * AssetNamingGui.RowHeight,
            area.width, AssetNamingGui.RowHeight);

        private static Rect ButtonRect(Rect row, float x, float width)
            => new(x, row.y + FieldInset, width, row.height - FieldInset * 2f);

        private void Rescan()
        {
            _needsScan = false;
            query.Scan(_ruleSet);
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

                    using (new EditorGUI.DisabledScope(query.Filtered.Count == 0))
                    {
                        if (GUILayout.Button(RenameAllContent, EditorStyles.toolbarButton,
                                GUILayout.Width(RenameAllWidth)))
                            _edits.RequestRenameAll();
                    }

                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginChangeCheck();

                    query.Sort = (EAssetNamingSort)EditorGUILayout.Popup((int)query.Sort, SortLabels,
                        EditorStyles.toolbarPopup, GUILayout.Width(SortWidth));

                    DrawRuleFilter();

                    query.Search = GUILayout.TextField(query.Search, EditorStyles.toolbarSearchField,
                        GUILayout.MinWidth(SearchWidth));

                    if (EditorGUI.EndChangeCheck())
                        query.Run();
                }
            }
        }

        private void DrawRuleFilter()
        {
            string[] labels = BuildRuleFilterLabels();
            int current = IndexOfRuleFilter(labels);
            int selected = EditorGUILayout.Popup(current, labels, EditorStyles.toolbarPopup,
                GUILayout.Width(FilterWidth));

            query.RuleFilter = selected <= 0
                ? string.Empty
                : labels[selected];
        }

        private string[] BuildRuleFilterLabels()
        {
            string[] labels = new string[_ruleSet.Rules.Count + 1];
            labels[0] = "All rules";

            for (int index = 0; index < _ruleSet.Rules.Count; index++)
                labels[index + 1] = _ruleSet.Rules[index].Label;

            return labels;
        }

        private int IndexOfRuleFilter(string[] labels)
        {
            if (query.RuleFilter.Length == 0)
                return 0;

            for (int index = 1; index < labels.Length; index++)
            {
                if (labels[index] == query.RuleFilter)
                    return index;
            }

            return 0;
        }

        private void DrawMissingRuleSet()
        {
            EditorGUILayout.HelpBox("No asset rule set found. Create one so the conventions are versioned with "
                + "the project, then press Auto-Detect to read the conventions the assets already follow.",
                MessageType.Info);

            if (GUILayout.Button(CreateContent, GUILayout.Width(200f)))
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
                    _edits.RequestAddFragment();

                if (fragmentRemovalIndex != AssetNamingRuleGui.NoIndex)
                    _edits.RequestFragmentRemoval(fragmentRemovalIndex);

                EditorGUILayout.Space(4f);

                int ruleRemovalIndex = AssetNamingRuleGui.DrawRules(_ruleSet);

                if (ruleRemovalIndex != AssetNamingRuleGui.NoIndex)
                    _edits.RequestRuleRemoval(ruleRemovalIndex);

                EditorGUILayout.Space(2f);

                if (AssetNamingRuleGui.DrawAddButton())
                    _edits.RequestAddRule();
            }
        }

        private void DrawDismissedSection()
        {
            if (query.DismissedCount == 0)
                return;

            IReadOnlyList<string> visible = query.GetVisibleDismissed();

            showDismissed = AssetNamingGui.DrawSectionHeader(showDismissed, "Dismissed", visible.Count,
                AssetNamingGui.DismissedAccent);

            if (!showDismissed)
                return;

            Rect area = ReserveRows(visible.Count);

            for (int index = 0; index < visible.Count; index++)
                DrawDismissedRow(RowRect(area, index), index, visible[index]);

            if (GUILayout.Button(ClearDismissedContent, GUILayout.Width(ButtonWidth)))
                _edits.RequestClearDismissed();
        }

        private void DrawDismissedRow(Rect row, int index, string path)
        {
            AssetNamingGui.DrawRowBackground(row, index);

            float reserved = GoToWidth + RestoreWidth + AssetNamingGui.Padding * 2f;
            Rect pathCell = Columns.Cell(row, 1);
            float pathStart = pathCell.x;
            Rect pathRect = new(pathStart, row.y, Mathf.Max(AssetNamingGui.Padding,
                row.xMax - pathStart - reserved), row.height);

            Rect goToRect = ButtonRect(row, pathRect.xMax + AssetNamingGui.Padding, GoToWidth);
            Rect restoreRect = ButtonRect(row, goToRect.xMax + AssetNamingGui.Padding, RestoreWidth);

            GUIContent name = new(Path.GetFileNameWithoutExtension(path), AssetDatabase.GetCachedIcon(path), path);

            GUI.Label(Columns.Cell(row, 0), name, AssetNamingGui.NameStyle);
            GUI.Label(pathRect, path, AssetNamingGui.DetailStyle);

            if (GUI.Button(goToRect, GoToContent, EditorStyles.miniButton))
                PingAsset(path);

            if (GUI.Button(restoreRect, RestoreContent, EditorStyles.miniButton))
                _edits.RequestRestore(AssetDatabase.AssetPathToGUID(path));
        }

        private void DrawResultsSection()
        {
            showResults = AssetNamingGui.DrawSectionHeader(showResults, "Scan Results", query.Filtered.Count,
                AssetNamingGui.ResultsAccent);

            if (!showResults)
                return;

            if (_ruleSet.Rules.Count == 0)
            {
                EditorGUILayout.HelpBox("The rule set is empty. Press Auto-Detect to read the conventions from "
                    + "the project, or add a rule by hand.", MessageType.Info);

                return;
            }

            if (!query.HasScanned)
            {
                EditorGUILayout.HelpBox("Press Scan to check the project.", MessageType.None);
                return;
            }

            if (query.ScannedCount == 0)
            {
                AssetNamingGui.DrawSuccess("Every asset follows the rules",
                    "Nothing left to rename. Your project is perfectly named.");

                return;
            }

            if (query.Filtered.Count == 0)
            {
                DrawEmptyResults();
                return;
            }

            DrawResultsHeader();

            foreach (AssetNamingGroup group in query.Groups)
                DrawGroup(group);
        }

        private void DrawGroup(AssetNamingGroup group)
        {
            if (group.Key.Length > 0)
            {
                bool expanded = AssetNamingGui.DrawGroupHeader(!_collapsedGroups.ContainsKey(group.Key), group.Key,
                    group.Violations.Count, AssetNamingGui.ResultsAccent);

                if (!expanded)
                {
                    _collapsedGroups[group.Key] = true;
                    return;
                }

                _collapsedGroups.Remove(group.Key);
            }

            Rect area = ReserveRows(group.Violations.Count);

            GetVisibleRange(area, group.Violations.Count, out int first, out int last);

            for (int index = first; index < last; index++)
                DrawRow(RowRect(area, index), index, group.Violations[index]);
        }

        /// <summary>
        /// Explains why the list is empty. Everything left being dismissed is a success, a filter
        /// hiding the rest is not, so the two states do not share a message.
        /// </summary>
        private void DrawEmptyResults()
        {
            if (query.IsFilterActive)
            {
                EditorGUILayout.HelpBox("No violation matches the current search or rule filter.",
                    MessageType.Info);

                return;
            }

            AssetNamingGui.DrawSuccess("Every asset follows the rules",
                "The rest is dismissed. Nothing left to rename.");
        }

        private void DrawResultsHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, AssetNamingGui.RowHeight, GUILayout.ExpandWidth(true));

            AssetNamingGui.DrawHeaderBackground(rect);

            if (Columns.DrawHeader(rect, Headers))
                Repaint();
        }

        private void DrawRow(Rect row, int index, AssetNamingViolation violation)
        {
            AssetNamingGui.DrawRowBackground(row, index);

            Rect suggestionRect = Columns.Field(row, 4);
            Rect renameRect = ButtonRect(row, row.x + Columns.TotalWidth, RenameWidth);
            Rect goToRect = ButtonRect(row, renameRect.xMax + AssetNamingGui.Padding, GoToWidth);
            Rect dismissRect = ButtonRect(row, goToRect.xMax + AssetNamingGui.Padding, DismissWidth);

            GUI.Label(Columns.Cell(row, 0), NameContent(violation), AssetNamingGui.NameStyle);
            GUI.Label(Columns.Cell(row, 1), new GUIContent(FolderOf(violation.AssetPath), violation.AssetPath),
                AssetNamingGui.DetailStyle);

            GUI.Label(Columns.Cell(row, 2), violation.RuleLabel, AssetNamingGui.DetailStyle);
            GUI.Label(Columns.Cell(row, 3), violation.Reason, AssetNamingGui.DetailStyle);

            string controlName = SuggestionControlPrefix + violation.AssetPath;

            GUI.SetNextControlName(controlName);
            violation.Suggestion = EditorGUI.TextField(suggestionRect, violation.Suggestion);

            if (GUI.Button(goToRect, GoToContent, EditorStyles.miniButton))
                PingAsset(violation.AssetPath);

            if (GUI.Button(dismissRect, DismissContent, EditorStyles.miniButton))
                _edits.RequestDismiss(violation);

            bool isRenameClicked = GUI.Button(renameRect, RenameContent, EditorStyles.miniButton);

            if (!isRenameClicked
                && !IsSubmitted(controlName))
                return;

            _edits.RequestRename(violation);
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

            Rect area = ReserveRows(entries.Count);
            GetVisibleRange(area, entries.Count, out int first, out int last);

            for (int index = first; index < last; index++)
                DrawHistoryRow(RowRect(area, index), index, entries[index]);

            if (GUILayout.Button(ClearHistoryContent, GUILayout.Width(ButtonWidth)))
                _edits.RequestClearHistory();
        }

        private void DrawHistoryRow(Rect row, int index, AssetNamingHistoryEntry entry)
        {
            AssetNamingGui.DrawRowBackground(row, index);

            float padding = AssetNamingGui.Padding;
            float reserved = TimeWidth + GoToWidth + UndoWidth + padding * 3f;
            float width = Mathf.Max(padding, row.xMax - row.x - padding - reserved);
            Rect textRect = new(row.x + padding, row.y, width, row.height);
            Rect timeRect = new(textRect.xMax + padding, row.y, TimeWidth, row.height);
            Rect undoRect = ButtonRect(row, timeRect.xMax + padding, UndoWidth);
            Rect goToRect = ButtonRect(row, undoRect.xMax + padding, GoToWidth);

            GUI.Label(textRect, new GUIContent(DescribeAction(entry), AssetNamingHistoryStore.PathOf(entry)),
                AssetNamingGui.NameStyle);

            GUI.Label(timeRect, entry.time, AssetNamingGui.DetailStyle);

            if (GUI.Button(undoRect, UndoContent, EditorStyles.miniButton))
                _edits.RequestUndo(entry);

            if (GUI.Button(goToRect, GoToContent, EditorStyles.miniButton))
                PingAsset(AssetNamingHistoryStore.PathOf(entry));
        }

        /// <summary>
        /// Range of rows that can be inside the shared scroll view, so long lists stay responsive
        /// without a scroll view of their own.
        /// </summary>
        private void GetVisibleRange(Rect area, int count, out int first, out int last)
        {
            float rowHeight = AssetNamingGui.RowHeight;
            int above = Mathf.FloorToInt((_scroll.y - area.y) / rowHeight) - 1;

            first = Mathf.Clamp(above, 0, Mathf.Max(0, count - 1));
            last = Mathf.Min(count, first + Mathf.CeilToInt(position.height / rowHeight) + 2);
        }

        /// <summary>
        /// Folds a detection run into the rule set. Rules and single fields that were created or
        /// changed by hand stay untouched, everything else is refreshed or dropped, so running the
        /// detection twice is safe.
        /// </summary>
        private void DetectConventions()
        {
            List<AssetNamingRule> detected = AssetConventionDetector.Detect(_ruleSet);
            AssetRuleMergeResult preview = AssetRuleMerger.Preview(_ruleSet, detected);

            if (preview.IsEmpty)
            {
                CustomLogger.Log("The rules already match what the project does.", _ruleSet);
                return;
            }

            if (!ConfirmDetection(preview))
                return;

            AssetRuleMergeResult result = AssetRuleMerger.Merge(_ruleSet, detected);

            _ruleSet.Persist();
            CustomLogger.Log($"Auto-detect: {result}.", _ruleSet);
            showRules = true;
            _needsScan = true;
        }

        private bool ConfirmDetection(AssetRuleMergeResult preview)
        {
            string message = $"Add {preview.Added} rule(s), refresh {preview.Updated} and remove "
                + $"{preview.Removed}. Rules and fields you changed by hand are kept as they are.";

            return EditorUtility.DisplayDialog(WindowTitle, message, "Apply", "Cancel");
        }

        /// <summary>
        /// Applies everything that changes the number of drawn controls. Doing it after the layout
        /// pass keeps IMGUI from reporting a mismatch between its layout and its repaint.
        /// </summary>
        private void ApplyPending()
        {
            EAssetNamingEditOutcome outcome = _edits.Apply(_ruleSet, query);

            if (outcome == EAssetNamingEditOutcome.None)
                return;

            if (outcome == EAssetNamingEditOutcome.Rescan)
                _needsScan = true;

            Repaint();
        }
    }
}