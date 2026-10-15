using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Base.DataHubPackage.Editor.AssetActions;
using Base.DataHubPackage.Editor.BulkEdit;
using Base.DataHubPackage.Editor.Extensions;
using Base.DataHubPackage.Editor.Sheets;
using Base.DataHubPackage.Editor.Table;
using Base.DataHubPackage.Editor.Validation;
using Base.EditorUIPackage.Editor;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// One window for every data asset of the project: the type tree on the left, the table of the
    /// selected type in the middle and the inspector of the selected rows on the right. What shows up
    /// is declared by the project through the interfaces in <see cref="Extensions"/>.
    /// </summary>
    internal sealed class DataHubWindow : EditorWindow
    {
        private const string ActionsLabel = "Actions  \u25BE";
        private const string ActionsMenuPrefix = "Actions/";
        private const string ActionsTooltip = "Run a [Button] method of the selected assets on every one of them, "
            + "in one undo step. Only the methods all selected assets share are listed. Also on right click.";
        private const string AddToCollectionItem = "Add to Collection";
        private const string AdjustAllFormat = "Adjust {0} for All {1} Shown Rows...";
        private const string AdjustOneRowFormat = "Adjust {0}...";
        private const string AdjustRowsFormat = "Adjust {0} for {1} Rows...";
        private const string AdjustSelectedFormat = "Adjust {0} for the {1} Selected Rows...";
        private const string AllFilterFormat = "All  {0}";
        private const string AllFilterTooltip = "Show every asset of this type.";
        private const string AssetsFormat = "{0} assets";
        private const string ChecksPassedText = "All checks pass";
        private const string ChecksPassedTooltip = "No asset of this type has a problem in the Check column.";
        private const string ClipboardEmptyText = "The clipboard is empty. Copy the rows in your spreadsheet first.";
        private const string ClipboardSource = "From the clipboard";
        private const string CopiedFormat = "Copied {0} rows. Paste them into a spreadsheet.";
        private const string CopyCommand = "Copy";
        private const string CopyItemFormat = "Copy Rows  ({0}+C)";
        private const string CsvExtension = "csv";
        private const string DeleteCommand = "Delete";
        private const string DeleteLabel = "Delete";
        private const string DeleteTooltip = "Move the selected assets to the trash. Every list in the project that "
            + "holds them is cleaned up first, so no empty slot is left behind. You confirm before anything is "
            + "deleted. (Delete)";
        private const string DuplicateCommand = "Duplicate";
        private const string DuplicateLabel = "Duplicate";
        private const string DuplicateTooltipFormat = "Copy every selected asset next to its original with the next "
            + "free number, and add the copies to their collection. ({0}+D)";
        private const string EmptyHintFormat = "Add a class that implements {0} to list a data type here, or {1} "
            + "for a custom page.";
        private const string EmptyTitle = "No data types registered";
        private const string ExportedFormat = "Saved {0} rows to {1}.";
        private const string ExportItem = "Export CSV...";
        private const string ExportTitleFormat = "Export {0}";
        private const string FileSourceFormat = "From {0}";
        private const float FilterWidth = 300f;
        private const string FindCommand = "Find";
        private const string ImportItem = "Import CSV...";
        private const string ImportTitleFormat = "Import into {0}";
        private const int InspectorDivider = 1;
        private const string InspectorWidthKey = "Base.DataHubPackage.InspectorWidth";
        private const string MacActionKey = "Cmd";
        private const float MaxSearchWidth = 420f;
        private const string MenuPath = "Tools/Base Packages/Assets/Data Hub";
        private const string NewLabel = "New";
        private const string NewTooltip = "Create a new asset of this type. Its folder, name and collection come "
            + "from the project's create rules, and it is selected right away. Pick the exact type first when the "
            + "table lists several.";
        private const string NoCreatableTypeFormat = "{0} has no concrete type to create.";
        private const string NotAdjustableItem = "Only Number Columns Can Be Adjusted";
        private const string OneAssetText = "1 asset";
        private const string OtherActionKey = "Ctrl";
        private const string PageSubtitle = "Custom page";
        private const string PageTooltip = "A page the project adds to the hub, like an overview over several "
            + "tables.";
        private const string PasteCommand = "Paste";
        private const string PasteItemFormat = "Paste into Table...  ({0}+V)";
        private const string PingItem = "Ping";
        private const string ProblemsFilterFormat = "Problems  {0}";
        private const string ProblemsFilterTooltip = "Show only the assets with at least one problem in the Check "
            + "column.";
        private const string ProblemsFormat = "{0} with problems";
        private const string ProblemsStatusTooltip = "Assets of this type with at least one problem, counted over the "
            + "whole table. Switch to Problems next to the search to see only those.";
        private const string RefreshTooltip = "Read every data asset of the project again and rerun every check. "
            + "Rarely needed, the hub refreshes on its own when assets change.";
        private const string RenameLabel = "Rename";
        private const string RenameTooltip = "Rename the selected asset. The box shows the final name after the "
            + "naming conventions before anything is changed. (F2, or double-click the name)";
        private const string ReplaceItem = "Replace Everywhere...";
        private const string SearchControlName = "DataHubSearch";
        private const string SearchPrompt = "Search names and values";
        private const string SearchTooltip = "Shows only the rows whose name or any cell contains the text, upper or "
            + "lower case alike. ({0}+F)";
        private const string SelectAllCommand = "SelectAll";
        private const string SelectedFormat = "  ·  {0} selected";
        private const string SheetDirectoryKey = "Base.DataHubPackage.SheetDirectory";
        private const string SheetLabel = "Sheet  \u25BE";
        private const string SheetTooltip = "Copy the shown rows to the clipboard or a CSV file, and bring edits made "
            + "in a spreadsheet back. Rows are matched by name, every change is shown before it is applied.";
        private const string ShowMenuPath = "Assets/Show in Data Hub";
        private const string ShownFormat = "{0} of {1} shown";
        private const string ShownTooltip = "How many assets the search and the filter leave, out of all assets of "
            + "this type, and how many of them are selected.";
        private const string SoftDeleteCommand = "SoftDelete";
        private const string TablesFormat = "{0} tables";
        private const string TablesTooltip = "Every data type the project lists in the hub. Pick one in the tree "
            + "below to see its assets.";
        private const string TableSubtitleFormat = "{0}  ·  {1}";
        private const string TableTooltipFormat = "Every asset of the type {0} in the project, one row each, with "
            + "one column per field. Hover a column title to see what it holds.";
        private const int TreeDivider = 0;
        private const string TreeWidthKey = "Base.DataHubPackage.TreeWidth";
        private const string WarningsFilterFormat = "Warnings  {0}";
        private const string WarningsFilterTooltip = "Show only the assets with an amber mark in the Check column, "
            + "like ones nothing uses yet.";
        private const string WarningsFormat = "{0} with warnings";
        private const string WarningsStatusTooltip = "Assets of this type worth a look, like ones nothing uses yet. "
            + "Nothing is broken. Switch to Warnings next to the search to see only those.";
        private const string WindowTitle = "Data Hub";

        [SerializeField] private EDataPanelTab panelTab;

        [SerializeField] private EDataRowFilter rowFilter;
        [SerializeField] private string search;
        [SerializeField] private string selectedPath;

        // None of these survive a domain reload, so EnsureInitialized creates them from the GUI pass too.
        private readonly List<MethodInfo> _actions = new();
        private IDataHubPage _activePage;
        private Rect _bulkAnchor;
        private DataColumn _bulkColumn;
        private Object[] _bulkTargets;
        private DataCollections _collections;
        private EditorColumnDividers _dividers;
        private DataTableHooks _hooks;
        private DataAssetIndex _index;
        private Object[] _inspected;
        private float _inspectorWidth;
        private bool _isRenameRequested;
        private bool _needsRebuild;
        private bool _needsRevalidate;
        private bool _needsViewSwap;
        private Object _pendingSelection;
        private DataHubRegistry _registry;
        private Object _replaceAsset;
        private EDataPanelTab _shownTab;
        private DataSidePanel _sidePanel;
        private DataHubStyles _styles;
        private DataHubTree _tree;
        private DataUsageIndex _usage;
        private float _treeWidth;
        private DataValidation _validation;
        private DataTableView _view;

        // For the menu check only, which runs whenever a menu opens and needs no window.
        private static DataHubRegistry _menuRegistry;

        private static string ActionKey => Application.platform == RuntimePlatform.OSXEditor
            ? MacActionKey
            : OtherActionKey;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle, DataHubIcons.Of(typeof(ScriptableObject)));
            minSize = new Vector2(DataHubStyles.MinWindowWidth, DataHubStyles.MinWindowHeight);

            // Row hover is only drawn if the window repaints while the mouse moves over it.
            wantsMouseMove = true;

            EditorApplication.projectChanged += OnProjectChanged;
            Undo.undoRedoPerformed += OnUndoRedo;

            EnsureInitialized();
        }

        private void OnFocus()
        {
            // An edit made in the Inspector window or to a collection while the hub was in the
            // background changes the badges without changing the project.
            _needsRevalidate = true;
        }

        private void OnGUI()
        {
            EnsureInitialized();
            _styles.EnsureBuilt();

            // Rows and views only change on the layout event, so the repaint after it draws the same controls.
            if (Event.current.type == EventType.Layout)
                ApplyPendingWork();

            if (Event.current.type == EventType.MouseMove)
                Repaint();

            if (_tree.IsEmpty)
            {
                DrawEmptyHub();
                return;
            }

            HandleCommands();
            DrawPanes(new Rect(0f, 0f, position.width, position.height));

            // Outside every pane, so the boxes open at the window coordinates of the cell they belong to.
            if (Event.current.type != EventType.Repaint)
                return;

            if (_isRenameRequested)
                ShowRenamePopup();

            if (_bulkColumn != null)
                ShowBulkEditPopup();

            if (_replaceAsset != null)
                ShowReplacePopup();
        }

        private void OnLostFocus()
        {
            // A button released outside the window never reports back, which would leave a drag hanging.
            _dividers?.Cancel();
            _view?.CancelDrag();
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;

            // Plain C# objects, so a window torn down before it ever drew has real nulls here.
            _view?.Dispose();
            _sidePanel?.Dispose();
            _styles?.Dispose();
        }
#endregion

        /// <summary>Opens or focuses the Data Hub.</summary>
        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            DataHubWindow window = GetWindow<DataHubWindow>(WindowTitle);
            window._needsRebuild = true;
            window.Show();
        }

        /// <summary>
        /// Opens the Data Hub on the table of the selected asset with its row selected, from the Project window.
        /// </summary>
        [DynamicMenuItem(ShowMenuPath, nameof(CanShowSelection))]
        private static void ShowSelection()
        {
            DataHubWindow window = GetWindow<DataHubWindow>(WindowTitle);
            window.Reveal(Selection.activeObject);
            window.Show();
        }

        private static bool CanShowSelection()
        {
            Object selected = Selection.activeObject;

            if (selected == null
                || !AssetDatabase.Contains(selected))
                return false;

            _menuRegistry ??= new DataHubRegistry();

            return _menuRegistry.FindTable(selected.GetType()) != null;
        }

        private static float ClampPane(float width, float available)
            => Mathf.Clamp(width, DataHubStyles.MinPaneWidth, Mathf.Max(DataHubStyles.MinPaneWidth, available));

        // Laid out from the right edge inwards, so the buttons keep their place whatever the title is.
        private static bool HeaderButton(ref float right, float y, GUIContent content, GUIStyle style)
        {
            float width = style.CalcSize(content).x;
            right -= width;

            bool isClicked = GUI.Button(new Rect(right, y, width, DataHubStyles.ButtonHeight), content, style);
            right -= EditorMetrics.TightGap;

            return isClicked;
        }

        private static bool HeaderIconButton(ref float right, float y, Texture icon, string tooltip,
            DataHubStyles styles)
        {
            right -= DataHubStyles.IconButtonWidth;

            Rect button = new(right, y, DataHubStyles.IconButtonWidth, DataHubStyles.ButtonHeight);
            bool isClicked = DataHubGui.IconButton(button, icon, tooltip, styles);

            right -= EditorMetrics.TightGap;

            return isClicked;
        }

        // One line under the headers of all three panes, so they read as one band across the window.
        private static void DrawHeaderLine(Rect area) => EditorRows.DrawSeparator(new Rect(area.x,
            area.y + DataHubStyles.PaneHeaderHeight - EditorMetrics.SeparatorThickness, area.width,
            EditorMetrics.SeparatorThickness));

        private void EnsureInitialized()
        {
            // Unity restores the plain fields of a window after a domain reload but not these objects,
            // so their absence, not a flag, is what says the window has to set itself up again.
            if (_registry != null)
                return;

            _registry = new DataHubRegistry();
            _index = new DataAssetIndex();
            _collections = new DataCollections(_index);
            _usage = new DataUsageIndex(_index, _registry);
            _validation = new DataValidation(_registry, _collections, _usage);
            _tree = new DataHubTree();
            _sidePanel = new DataSidePanel(_registry);
            _styles = new DataHubStyles();
            _dividers = new EditorColumnDividers();
            _inspected = Array.Empty<Object>();

            _hooks = new DataTableHooks
            {
                ClearFilter = ClearFilter,
                Create = ShowNewMenu,
                Rename = RequestRename,
                ShowColumnMenu = ShowColumnMenu,
                ShowContextMenu = ShowRowMenu
            };

            _treeWidth = EditorPrefs.GetFloat(TreeWidthKey, DataHubStyles.DefaultTreeWidth);
            _inspectorWidth = EditorPrefs.GetFloat(InspectorWidthKey, DataHubStyles.DefaultInspectorWidth);

            search ??= string.Empty;

            _tree.Build(_registry);
            _needsRebuild = true;
            _needsViewSwap = true;
        }

        private void ApplyPendingWork()
        {
            if (_needsRebuild)
            {
                _index.Rebuild();
                _collections.Clear();
                _usage.ClearAll();
                _sidePanel.Invalidate();
                _tree.RefreshCounts(_index);
            }

            if (_needsViewSwap)
                SwapView();

            if (_needsRebuild
                || _needsViewSwap)
                RefreshContent();
            else if (_needsRevalidate)
                Revalidate();

            _needsRebuild = false;
            _needsViewSwap = false;
            _needsRevalidate = false;

            _view?.ApplyPending();
            SelectPending();

            // The page and the inspector draw with GUILayout, which needs the same controls in the
            // repaint as in the layout before it. A click that changes the selection mid pass only
            // reaches them on the next layout.
            _activePage = _view == null
                ? _tree.Selected?.Page
                : null;

            _inspected = _view?.SelectedAssets ?? Array.Empty<Object>();
            _shownTab = panelTab;

            _actions.Clear();
            DataButtonActions.Collect(_inspected, _actions);
        }

        private void SwapView()
        {
            _tree.Select(selectedPath);
            selectedPath = _tree.Selected?.Path;

            _view?.Dispose();
            _view = null;

            IDataTable table = _tree.Selected?.Table;

            if (table == null)
                return;

            _view = new DataTableView(table, _registry);
            _view.SetFilter(search, rowFilter);
        }

        private void RefreshContent()
        {
            _tree.Selected?.Page?.Refresh();

            if (_view == null)
                return;

            _view.Rebuild(_index, _validation);
        }

        // A new or revealed asset hidden by the search or the problem filter would leave the user looking at
        // nothing, so the filter is cleared and the selection tried again on the next layout.
        private void SelectPending()
        {
            if (_pendingSelection == null
                || _view == null)
                return;

            if (_view.Select(_pendingSelection))
            {
                _pendingSelection = null;
                return;
            }

            bool isFiltered = search.Length > 0
                || rowFilter != EDataRowFilter.All;

            if (isFiltered
                && _view.Contains(_pendingSelection))
            {
                ClearFilter();
                return;
            }

            _pendingSelection = null;
        }

        private void Reveal(Object asset)
        {
            EnsureInitialized();

            IDataTable table = asset == null
                ? null
                : _registry.FindTable(asset.GetType());

            if (table == null)
                return;

            selectedPath = table.Path;
            _pendingSelection = asset;
            _needsViewSwap = true;

            Repaint();
        }

        private void Revalidate()
        {
            // A collection or a deck may have been edited, so what they hold is read again.
            _collections.Clear();
            _usage.Clear();
            _view?.Revalidate(_validation);
        }

        private void DrawEmptyHub()
        {
            string hint = string.Format(EmptyHintFormat, nameof(IDataTable), nameof(IDataHubPage));
            EditorWindowChrome.DrawEmptyState(_styles, EditorIcons.Script, EmptyTitle, hint);
        }

        // The menu commands Unity sends for its own shortcuts, so Duplicate, Delete, Select All and Find
        // work in the table the same way they do in the Project window.
        private void HandleCommands()
        {
            Event current = Event.current;

            if (_view == null)
                return;

            if (current.type != EventType.ValidateCommand
                && current.type != EventType.ExecuteCommand)
                return;

            bool isExecute = current.type == EventType.ExecuteCommand;
            bool isTyping = GUIUtility.keyboardControl != 0;
            bool hasSelection = _view.SelectedAssets.Length > 0;

            switch (current.commandName)
            {
                case FindCommand:
                    if (isExecute)
                        EditorGUI.FocusTextInControl(SearchControlName);

                    break;

                case DuplicateCommand when !isTyping && hasSelection:
                    if (isExecute)
                        EditorApplication.delayCall += DuplicateSelection;

                    break;

                case DeleteCommand when !isTyping && hasSelection:
                case SoftDeleteCommand when !isTyping && hasSelection:
                    if (isExecute)
                        EditorApplication.delayCall += DeleteSelection;

                    break;

                case SelectAllCommand when !isTyping:
                    if (isExecute)
                        _view.SelectAll();

                    break;

                // The same keys as in a spreadsheet: copy rows out, paste edited rows back in.
                case CopyCommand when !isTyping:
                    if (isExecute)
                        CopyTable();

                    break;

                case PasteCommand when !isTyping:
                    if (isExecute)
                        EditorApplication.delayCall += PasteTable;

                    break;

                default:
                    return;
            }

            current.Use();
        }

        private void DrawPanes(Rect area)
        {
            bool hasSidePanel = _view != null;

            float sideWidth = hasSidePanel
                ? ClampPane(_inspectorWidth, area.width - _treeWidth - DataHubStyles.MinPaneWidth)
                : 0f;

            float treeWidth = ClampPane(_treeWidth, area.width - sideWidth - DataHubStyles.MinPaneWidth);

            Rect sidebar = new(area.x, area.y, treeWidth, area.height);
            Rect content = new(sidebar.xMax, area.y, area.width - treeWidth - sideWidth, area.height);
            Rect side = new(content.xMax, area.y, sideWidth, area.height);

            DrawSidebar(sidebar);
            DrawContent(content);

            if (hasSidePanel
                && _sidePanel.Draw(side, _inspected, _shownTab, ref panelTab, _index, _styles))
                _needsRevalidate = true;

            DrawHeaderLine(area);
            HandlePaneDividers(area, sidebar.xMax, side.x, hasSidePanel);

            // A sort click or an edit is only applied on the next layout, so that pass has to come.
            if (Event.current.type == EventType.Used)
                Repaint();
        }

        private void DrawSidebar(Rect area)
        {
            DataHubGui.Fill(area, DataHubStyles.SidebarColor);

            Rect header = new(area.x, area.y, area.width, DataHubStyles.PaneHeaderHeight);
            float right = header.xMax - DataHubStyles.PanelPadding;
            float y = header.y + (header.height - DataHubStyles.ButtonHeight) * 0.5f;

            if (HeaderIconButton(ref right, y, DataHubIcons.Refresh, RefreshTooltip, _styles))
                _needsRebuild = true;

            DataHubGui.PaneTitle(new Rect(header.x, header.y, right - header.x, header.height), null, null,
                WindowTitle, string.Format(TablesFormat, _registry.Tables.Count), _styles, TablesTooltip);

            Rect tree = new(area.x, header.yMax, area.width, Mathf.Max(0f, area.height - header.height));

            if (_tree.Draw(tree, _styles))
                OnTreeSelectionChanged();
        }

        private void DrawContent(Rect area)
        {
            Rect header = new(area.x, area.y, area.width, DataHubStyles.PaneHeaderHeight);

            if (_activePage != null)
            {
                DrawPage(area, header);
                return;
            }

            if (_view == null)
                return;

            DrawTableHeader(header);

            Rect filter = new(area.x, header.yMax, area.width, DataHubStyles.FilterBarHeight);
            DrawFilterBar(filter);

            float top = filter.yMax;
            float tableHeight = Mathf.Max(0f, area.yMax - top - DataHubStyles.StatusBarHeight);

            if (_view.Draw(new Rect(area.x, top, area.width, tableHeight), _styles, _validation, _hooks))
                Repaint();

            DrawStatusBar(new Rect(area.x, top + tableHeight, area.width, DataHubStyles.StatusBarHeight));
        }

        private void DrawPage(Rect area, Rect header)
        {
            DataHubGui.PaneTitle(header, DataHubIcons.Page, _tree.SelectedParentPath(), _tree.Selected.Label,
                PageSubtitle, _styles, PageTooltip);

            float padding = DataHubStyles.PanelPadding;

            Rect page = new(area.x + padding, header.yMax + padding, Mathf.Max(0f, area.width - padding * 2f),
                Mathf.Max(0f, area.height - header.height - padding * 2f));

            GUILayout.BeginArea(page);
            _activePage.Draw();
            GUILayout.EndArea();
        }

        private void DrawTableHeader(Rect area)
        {
            float right = area.xMax - DataHubStyles.PanelPadding;
            float y = area.y + (area.height - DataHubStyles.ButtonHeight) * 0.5f;

            Object[] selected = _view.SelectedAssets;
            bool hasSelection = selected.Length > 0;

            using (new EditorGUI.DisabledScope(_actions.Count == 0))
            {
                GUIContent actions = new(ActionsLabel, ActionsTooltip);

                if (HeaderButton(ref right, y, actions, _styles.HeaderButton))
                    ShowActionsMenu(string.Empty);
            }

            if (HeaderButton(ref right, y, new GUIContent(SheetLabel, SheetTooltip), _styles.HeaderButton))
                ShowSheetMenu();

            right -= EditorMetrics.ItemGap;

            // Deferred, so the asset changes and the dialogs they may open never run halfway through a
            // GUI pass.
            using (new EditorGUI.DisabledScope(!hasSelection))
            {
                if (HeaderButton(ref right, y, new GUIContent(DeleteLabel, DeleteTooltip), _styles.HeaderButton))
                    EditorApplication.delayCall += DeleteSelection;
            }

            using (new EditorGUI.DisabledScope(selected.Length != 1))
            {
                if (HeaderButton(ref right, y, new GUIContent(RenameLabel, RenameTooltip), _styles.HeaderButton))
                    RequestRename();
            }

            using (new EditorGUI.DisabledScope(!hasSelection))
            {
                GUIContent duplicate = new(DuplicateLabel, string.Format(DuplicateTooltipFormat, ActionKey));

                if (HeaderButton(ref right, y, duplicate, _styles.HeaderButton))
                    EditorApplication.delayCall += DuplicateSelection;
            }

            right -= EditorMetrics.ItemGap;

            GUIContent create = new(NewLabel, NewTooltip);

            if (HeaderButton(ref right, y, create, _styles.HeaderPrimaryButton))
                ShowNewMenu();

            string count = _view.RowCount == 1
                ? OneAssetText
                : string.Format(AssetsFormat, _view.RowCount);

            string subtitle = string.Format(TableSubtitleFormat, count,
                ObjectNames.NicifyVariableName(_view.Table.DataType.Name));

            string tooltip = string.Format(TableTooltipFormat, _view.Table.DataType.FullName);

            DataHubGui.PaneTitle(new Rect(area.x, area.y, Mathf.Max(0f, right - area.x), area.height),
                DataHubIcons.Of(_view.Table.DataType), _tree.SelectedParentPath(), _tree.Selected.Label, subtitle,
                _styles, tooltip);
        }

        private void DrawFilterBar(Rect area)
        {
            float padding = DataHubStyles.PanelPadding;
            float y = area.y + (area.height - DataHubStyles.ButtonHeight) * 0.5f;
            float available = Mathf.Max(0f, area.width - padding * 2f - FilterWidth - EditorMetrics.ItemGap);

            Rect searchRect = new(area.x + padding, y, Mathf.Min(MaxSearchWidth, available),
                DataHubStyles.ButtonHeight);

            Rect filterRect = new(searchRect.xMax + EditorMetrics.ItemGap, y, FilterWidth, DataHubStyles.ButtonHeight);

            GUIContent[] filters =
            {
                new(string.Format(AllFilterFormat, _view.RowCount), AllFilterTooltip),
                new(string.Format(ProblemsFilterFormat, _view.ProblemCount), ProblemsFilterTooltip),
                new(string.Format(WarningsFilterFormat, _view.WarningCount), WarningsFilterTooltip)
            };

            EditorGUI.BeginChangeCheck();

            GUI.SetNextControlName(SearchControlName);
            search = EditorSearchField.Draw(_styles, searchRect, search, SearchPrompt);

            // The field has no tooltip of its own, and a label takes no clicks away from it.
            GUI.Label(searchRect, new GUIContent(string.Empty, string.Format(SearchTooltip, ActionKey)));
            rowFilter = (EDataRowFilter)DataHubGui.Segmented(filterRect, (int)rowFilter, filters, _styles);

            if (EditorGUI.EndChangeCheck())
                _view.SetFilter(search, rowFilter);
        }

        private void DrawStatusBar(Rect area)
        {
            EditorRows.DrawSeparator(area);

            Rect line = new(area.x + DataHubStyles.PanelPadding, area.y,
                Mathf.Max(0f, area.width - DataHubStyles.PanelPadding * 2f), area.height);

            string shown = string.Format(ShownFormat, _view.FilteredCount, _view.RowCount);

            if (_view.SelectedAssets.Length > 0)
                shown += string.Format(SelectedFormat, _view.SelectedAssets.Length);

            GUI.Label(line, new GUIContent(shown, ShownTooltip), _styles.Footer);

            float right = line.xMax;

            if (_view.ProblemCount == 0
                && _view.WarningCount == 0)
            {
                DrawStatus(ref right, line, new GUIContent(ChecksPassedText, EditorIcons.Success, ChecksPassedTooltip));
                return;
            }

            if (_view.WarningCount > 0)
            {
                DrawStatus(ref right, line, new GUIContent(string.Format(WarningsFormat, _view.WarningCount),
                    EditorIcons.Warning, WarningsStatusTooltip));
            }

            if (_view.ProblemCount > 0)
            {
                DrawStatus(ref right, line, new GUIContent(string.Format(ProblemsFormat, _view.ProblemCount),
                    EditorIcons.Error, ProblemsStatusTooltip));
            }
        }

        // Laid out from the right, problems left of warnings, so the more serious one is read first.
        private void DrawStatus(ref float right, Rect line, GUIContent content)
        {
            float width = _styles.Footer.CalcSize(content).x;
            right -= width;

            GUI.Label(new Rect(right, line.y, width, line.height), content, _styles.Footer);
            right -= EditorMetrics.ItemGap * 2f;
        }

        private void HandlePaneDividers(Rect area, float treeEdge, float sideEdge, bool hasSidePanel)
        {
            EEditorDividerAction treeAction = _dividers.Handle(TreeDivider, treeEdge, area, out float treeMouse);

            if (treeAction != EEditorDividerAction.None)
                _treeWidth = treeMouse - area.x;

            if (treeAction == EEditorDividerAction.Released)
                EditorPrefs.SetFloat(TreeWidthKey, _treeWidth);

            if (!hasSidePanel)
                return;

            EEditorDividerAction sideAction = _dividers.Handle(InspectorDivider, sideEdge, area, out float sideMouse);

            if (sideAction != EEditorDividerAction.None)
                _inspectorWidth = area.xMax - sideMouse;

            if (sideAction == EEditorDividerAction.Released)
                EditorPrefs.SetFloat(InspectorWidthKey, _inspectorWidth);
        }

        private void OnTreeSelectionChanged()
        {
            selectedPath = _tree.Selected?.Path;
            _needsViewSwap = true;

            Repaint();
        }

        private void ClearFilter()
        {
            search = string.Empty;
            rowFilter = EDataRowFilter.All;
            GUIUtility.keyboardControl = 0;

            _view?.SetFilter(search, rowFilter);
            Repaint();
        }

        private void ShowNewMenu()
        {
            Type dataType = _view.Table.DataType;

            if (!dataType.IsAbstract)
            {
                CreateAsset(dataType);
                return;
            }

            GenericMenu menu = new();

            foreach (Type type in TypeCache.GetTypesDerivedFrom(dataType))
            {
                if (type.IsAbstract
                    || type.ContainsGenericParameters
                    || !typeof(ScriptableObject).IsAssignableFrom(type))
                    continue;

                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false,
                    func: () => CreateAsset(type));
            }

            if (menu.GetItemCount() == 0)
            {
                CustomLogger.LogWarning(string.Format(NoCreatableTypeFormat, dataType.Name), null);
                return;
            }

            menu.ShowAsContext();
        }

        private void CreateAsset(Type type)
        {
            Object asset = DataAssetActions.Create(type, _registry.FindCreateRule(type), FallbackFolder(),
                _collections);

            if (asset == null)
                return;

            _pendingSelection = asset;
            _needsRebuild = true;

            Repaint();
        }

        // Without a create rule a new asset lands next to the one selected, or next to any of its kind.
        private string FallbackFolder()
        {
            Object reference = _view.SelectedAssets.Length > 0
                ? _view.SelectedAssets[0]
                : _view.FirstAsset;

            return reference == null
                ? DataAssetActions.AssetsRoot
                : DataAssetActions.FolderOf(AssetDatabase.GetAssetPath(reference));
        }

        private void DuplicateSelection()
        {
            foreach (Object source in _view.SelectedAssets)
            {
                Object copy = DataAssetActions.Duplicate(source, _registry.FindCreateRule(source.GetType()),
                    _collections);

                if (copy != null)
                    _pendingSelection = copy;
            }

            _needsRebuild = true;
            Repaint();
        }

        // The popup has to open from inside a GUI pass, which a menu or a key handler is not, so they
        // only ask for it and the next repaint opens it on the row's name.
        private void RequestRename()
        {
            _isRenameRequested = true;
            Repaint();
        }

        private void ShowRenamePopup()
        {
            _isRenameRequested = false;

            if (_view == null
                || !_view.TryGetNameRect(out Rect anchor))
                return;

            Object asset = _view.SelectedAssets[0];

            PopupWindow.Show(anchor, new DataRenamePopup(asset, _styles, name =>
            {
                DataAssetActions.Rename(asset, name);
                _needsRebuild = true;
                Repaint();
            }));
        }

        private void DeleteSelection()
        {
            if (!DataAssetActions.Delete(_view.SelectedAssets, _index))
                return;

            _needsRebuild = true;
            Repaint();
        }

        private void ShowRowMenu()
        {
            Object[] selected = _view.SelectedAssets;
            GenericMenu menu = new();

            // The cell that was right-clicked comes first, because that is what the click was about.
            DataColumn column = _view.ContextColumn;

            if (column != null
                && column.IsNumber)
            {
                string label = selected.Length == 1
                    ? string.Format(AdjustOneRowFormat, column.Header)
                    : string.Format(AdjustRowsFormat, column.Header, selected.Length);

                Rect anchor = _view.ContextRect;
                menu.AddItem(new GUIContent(label), false, func: () => RequestBulkEdit(column, selected, anchor));
                menu.AddSeparator(string.Empty);
            }

            if (selected.Length == 1)
                menu.AddItem(new GUIContent(PingItem), false, func: () => EditorGUIUtility.PingObject(selected[0]));

            if (HasMissingCollection(selected))
                menu.AddItem(new GUIContent(AddToCollectionItem), false, func: () => AddToCollections(selected));

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(DuplicateLabel), false, func: DuplicateSelection);

            if (selected.Length == 1)
            {
                menu.AddItem(new GUIContent(RenameLabel), false, func: RequestRename);
                menu.AddItem(new GUIContent(ReplaceItem), false, func: () => RequestReplace(selected[0]));
            }

            menu.AddItem(new GUIContent(DeleteLabel), false, func: DeleteSelection);
            AddActions(menu, ActionsMenuPrefix);
            menu.ShowAsContext();
        }

        private void ShowColumnMenu(DataColumn column, Rect anchor)
        {
            GenericMenu menu = new();

            if (!column.IsNumber)
            {
                menu.AddDisabledItem(new GUIContent(NotAdjustableItem));
                menu.ShowAsContext();

                return;
            }

            Object[] selected = _view.SelectedAssets;
            Object[] shown = _view.FilteredAssets;

            if (selected.Length > 1)
            {
                menu.AddItem(new GUIContent(string.Format(AdjustSelectedFormat, column.Header, selected.Length)), false,
                    func: () => RequestBulkEdit(column, selected, anchor));
            }

            menu.AddItem(new GUIContent(string.Format(AdjustAllFormat, column.Header, shown.Length)), false,
                func: () => RequestBulkEdit(column, shown, anchor));

            menu.ShowAsContext();
        }

        // Like rename, the box can only open from a GUI pass, so the menu only asks for it.
        private void RequestBulkEdit(DataColumn column, Object[] targets, Rect anchor)
        {
            _bulkColumn = column;
            _bulkTargets = targets;
            _bulkAnchor = anchor;

            Repaint();
        }

        private void ShowBulkEditPopup()
        {
            DataColumn column = _bulkColumn;
            Object[] targets = _bulkTargets;
            _bulkColumn = null;
            _bulkTargets = null;

            if (targets.Length == 0)
                return;

            PopupWindow.Show(GUIUtility.ScreenToGUIRect(_bulkAnchor), new DataBulkEditPopup(column, targets, _styles,
                onApplied: _ =>
                {
                    _needsRevalidate = true;
                    Repaint();
                }));
        }

        private void RequestReplace(Object asset)
        {
            _replaceAsset = asset;
            Repaint();
        }

        // Opens on the row of the asset, which is still the selected one, so the box sits next to its name.
        private void ShowReplacePopup()
        {
            Object asset = _replaceAsset;
            _replaceAsset = null;

            if (asset == null
                || !_view.TryGetNameRect(out Rect anchor))
                return;

            Type pickType = _registry.FindTable(asset.GetType())?.DataType ?? asset.GetType();
            List<DataReplaceLine> lines = DataReplace.Plan(asset, _index, _registry);

            PopupWindow.Show(anchor, new DataReplacePopup(asset, pickType, lines, _styles, onApplied: () =>
            {
                _needsRebuild = true;
                Repaint();
            }));
        }

        private void ShowSheetMenu()
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent(string.Format(CopyItemFormat, ActionKey)), false, func: CopyTable);
            menu.AddItem(new GUIContent(string.Format(PasteItemFormat, ActionKey)), false, func: PasteTable);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(ExportItem), false, func: ExportCsv);
            menu.AddItem(new GUIContent(ImportItem), false, func: ImportCsv);

            menu.ShowAsContext();
        }

        // What is shown is what is copied, so a search or a filter picks the rows for the spreadsheet.
        private string SheetText(char separator) => DataSheetFormat.Write(
            DataSheetTable.Export(_view.Columns, _view.ShownRows), separator);

        // The selected rows when there are any, like copying cells in a spreadsheet, every shown row otherwise.
        private void CopyTable()
        {
            IReadOnlyList<DataRow> rows = _view.SelectedAssets.Length > 0
                ? _view.SelectedRows
                : _view.ShownRows;

            EditorGUIUtility.systemCopyBuffer = DataSheetFormat.Write(DataSheetTable.Export(_view.Columns, rows),
                DataSheetFormat.Tab);

            ShowNotification(new GUIContent(string.Format(CopiedFormat, rows.Count)));
        }

        private void PasteTable()
        {
            string text = EditorGUIUtility.systemCopyBuffer;

            if (string.IsNullOrWhiteSpace(text))
            {
                ShowNotification(new GUIContent(ClipboardEmptyText));
                return;
            }

            ShowImportPreview(text, ClipboardSource);
        }

        private void ExportCsv()
        {
            string label = _tree.Selected.Label;
            string path = EditorUtility.SaveFilePanel(string.Format(ExportTitleFormat, label), SheetDirectory(),
                label, CsvExtension);

            if (string.IsNullOrEmpty(path))
                return;

            // With a byte order mark, or Excel reads umlauts and accents as two odd characters each.
            File.WriteAllText(path, SheetText(DataSheetFormat.Comma), new UTF8Encoding(true));
            RememberSheetDirectory(path);

            ShowNotification(new GUIContent(string.Format(ExportedFormat, _view.ShownRows.Count,
                Path.GetFileName(path))));
        }

        private void ImportCsv()
        {
            string path = EditorUtility.OpenFilePanel(string.Format(ImportTitleFormat, _tree.Selected.Label),
                SheetDirectory(), CsvExtension);

            if (string.IsNullOrEmpty(path))
                return;

            RememberSheetDirectory(path);
            ShowImportPreview(File.ReadAllText(path), string.Format(FileSourceFormat, Path.GetFileName(path)));
        }

        private void ShowImportPreview(string text, string source)
        {
            List<List<string>> sheet = DataSheetFormat.Read(text, DataSheetFormat.Detect(text));
            DataImportPlan plan = DataSheetTable.Plan(_view.Columns, _view.Rows, sheet);

            DataImportWindow.Show(plan, _tree.Selected.Label, source, onApplied: () =>
            {
                _needsRevalidate = true;
                Repaint();
            });
        }

        private static string SheetDirectory() => EditorPrefs.GetString(SheetDirectoryKey,
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        private static void RememberSheetDirectory(string path)
            => EditorPrefs.SetString(SheetDirectoryKey, Path.GetDirectoryName(path));

        private void ShowActionsMenu(string prefix)
        {
            GenericMenu menu = new();
            AddActions(menu, prefix);
            menu.ShowAsContext();
        }

        // Read from the selection the menu was opened for, because the actions list is refreshed on the
        // next layout pass and may change before an item is picked.
        private void AddActions(GenericMenu menu, string prefix)
        {
            Object[] targets = _view.SelectedAssets;
            List<MethodInfo> actions = new();
            DataButtonActions.Collect(targets, actions);

            foreach (MethodInfo action in actions)
            {
                menu.AddItem(new GUIContent(prefix + DataButtonActions.Label(action)), false, func: () =>
                {
                    DataButtonActions.Run(action, targets);
                    _needsRebuild = true;
                    Repaint();
                });
            }
        }

        private bool HasMissingCollection(Object[] assets)
        {
            foreach (Object asset in assets)
            {
                if (_collections.IsMissingFrom(_registry.FindCreateRule(asset.GetType()), asset, out _))
                    return true;
            }

            return false;
        }

        private void AddToCollections(Object[] assets)
        {
            foreach (Object asset in assets)
            {
                IDataCreateRule rule = _registry.FindCreateRule(asset.GetType());

                if (_collections.IsMissingFrom(rule, asset, out _))
                    _collections.Add(rule, asset);
            }

            _needsRevalidate = true;
            Repaint();
        }

        private void OnProjectChanged()
        {
            _needsRebuild = true;
            Repaint();
        }

        private void OnUndoRedo()
        {
            _needsRevalidate = true;
            Repaint();
        }
    }
}