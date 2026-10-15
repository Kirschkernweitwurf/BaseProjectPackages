using System;
using System.Collections.Generic;
using System.Reflection;
using Base.DataHubPackage.Editor.AssetActions;
using Base.DataHubPackage.Editor.Extensions;
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
        private const string AllFilterFormat = "All  {0}";
        private const string AllFilterTooltip = "Show every asset of this type.";
        private const string AssetsFormat = "{0} assets";
        private const string ChecksPassedText = "All checks pass";
        private const string ChecksPassedTooltip = "No asset of this type has a problem in the Check column.";
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
        private const float FilterWidth = 200f;
        private const string FindCommand = "Find";
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
        private const string OneAssetText = "1 asset";
        private const string OtherActionKey = "Ctrl";
        private const string PageSubtitle = "Custom page";
        private const string PageTooltip = "A page the project adds to the hub, like an overview over several "
            + "tables.";
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
        private const string SearchControlName = "DataHubSearch";
        private const string SearchPrompt = "Search names and values";
        private const string SearchTooltip = "Shows only the rows whose name or any cell contains the text, upper or "
            + "lower case alike. ({0}+F)";
        private const string SelectAllCommand = "SelectAll";
        private const string SelectedFormat = "  ·  {0} selected";
        private const string ShownFormat = "{0} of {1} shown";
        private const string ShownTooltip = "How many assets the search and the filter leave, out of all assets of "
            + "this type, and how many of them are selected.";
        private const string SoftDeleteCommand = "SoftDelete";
        private const string TableSubtitleFormat = "{0}  ·  {1}";
        private const string TableTooltipFormat = "Every asset of the type {0} in the project, one row each, with "
            + "one column per field. Hover a column title to see what it holds.";
        private const string TablesFormat = "{0} tables";
        private const string TablesTooltip = "Every data type the project lists in the hub. Pick one in the tree "
            + "below to see its assets.";
        private const int TreeDivider = 0;
        private const string TreeWidthKey = "Base.DataHubPackage.TreeWidth";
        private const string WindowTitle = "Data Hub";

        [SerializeField] private EDataPanelTab panelTab;

        [SerializeField] private bool problemsOnly;
        [SerializeField] private string search;
        [SerializeField] private string selectedPath;

        // None of these survive a domain reload, so EnsureInitialized creates them from the GUI pass too.
        private readonly List<MethodInfo> _actions = new();
        private IDataHubPage _activePage;
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
        private EDataPanelTab _shownTab;
        private DataSidePanel _sidePanel;
        private DataHubStyles _styles;
        private DataHubTree _tree;
        private float _treeWidth;
        private DataValidation _validation;
        private DataTableView _view;

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

            // Outside every pane, so the box opens at the window coordinates of the name it renames.
            if (_isRenameRequested
                && Event.current.type == EventType.Repaint)
                ShowRenamePopup();
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
            _validation = new DataValidation(_registry, _collections);
            _tree = new DataHubTree();
            _sidePanel = new DataSidePanel();
            _styles = new DataHubStyles();
            _dividers = new EditorColumnDividers();
            _inspected = Array.Empty<Object>();

            _hooks = new DataTableHooks
            {
                ClearFilter = ClearFilter,
                Create = ShowNewMenu,
                Rename = RequestRename,
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
            _view.SetFilter(search, problemsOnly);
        }

        private void RefreshContent()
        {
            _tree.Selected?.Page?.Refresh();

            if (_view == null)
                return;

            _view.Rebuild(_index, _validation);

            if (_pendingSelection == null)
                return;

            _view.Select(_pendingSelection);
            _pendingSelection = null;
        }

        private void Revalidate()
        {
            // A collection may have been edited by hand, so its cached members are read again.
            _collections.Clear();
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
                new(string.Format(ProblemsFilterFormat, _view.ProblemCount), ProblemsFilterTooltip)
            };

            int filter = problemsOnly
                ? 1
                : 0;

            EditorGUI.BeginChangeCheck();

            GUI.SetNextControlName(SearchControlName);
            search = EditorSearchField.Draw(_styles, searchRect, search, SearchPrompt);

            // The field has no tooltip of its own, and a label takes no clicks away from it.
            GUI.Label(searchRect, new GUIContent(string.Empty, string.Format(SearchTooltip, ActionKey)));
            problemsOnly = DataHubGui.Segmented(filterRect, filter, filters, _styles) == 1;

            if (EditorGUI.EndChangeCheck())
                _view.SetFilter(search, problemsOnly);
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

            GUIContent result = _view.ProblemCount > 0
                ? new GUIContent(string.Format(ProblemsFormat, _view.ProblemCount), EditorIcons.Error,
                    ProblemsStatusTooltip)
                : new GUIContent(ChecksPassedText, EditorIcons.Success, ChecksPassedTooltip);

            float width = _styles.Footer.CalcSize(result).x;
            GUI.Label(new Rect(line.xMax - width, line.y, width, line.height), result, _styles.Footer);
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
            problemsOnly = false;
            GUIUtility.keyboardControl = 0;

            _view?.SetFilter(search, problemsOnly);
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

            if (selected.Length == 1)
                menu.AddItem(new GUIContent(PingItem), false, func: () => EditorGUIUtility.PingObject(selected[0]));

            if (HasMissingCollection(selected))
                menu.AddItem(new GUIContent(AddToCollectionItem), false, func: () => AddToCollections(selected));

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(DuplicateLabel), false, func: DuplicateSelection);

            if (selected.Length == 1)
                menu.AddItem(new GUIContent(RenameLabel), false, func: RequestRename);

            menu.AddItem(new GUIContent(DeleteLabel), false, func: DeleteSelection);
            AddActions(menu, ActionsMenuPrefix);
            menu.ShowAsContext();
        }

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