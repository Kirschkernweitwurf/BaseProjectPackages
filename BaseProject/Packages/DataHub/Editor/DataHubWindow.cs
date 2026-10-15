using System;
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
        private const string AddToCollectionItem = "Add to Collection";
        private const string DeleteLabel = "Delete";
        private const string DuplicateLabel = "Duplicate";
        private const string EmptyHintFormat = "Add a class that implements {0} to list a data type here, or {1} "
            + "for a custom page.";
        private const string EmptyTitle = "No data types registered";
        private const int InspectorDivider = 1;
        private const string InspectorWidthKey = "Base.DataHubPackage.InspectorWidth";
        private const string MenuPath = "Tools/Base Packages/Assets/Data Hub";
        private const string NewLabel = "New";
        private const string NoCreatableTypeFormat = "{0} has no concrete type to create.";
        private const string PingItem = "Ping";
        private const string ProblemsLabel = "Problems only";
        private const string ProblemsTooltip = "Show only assets with a red badge.";
        private const string RefreshLabel = "Refresh";
        private const string RenameLabel = "Rename";
        private const string SearchPrompt = "Search names and values";
        private const string SummaryFormat = "{0} of {1}";
        private const string SummaryProblemsFormat = "{0} of {1}, {2} with problems";
        private const int TreeDivider = 0;
        private const string TreeWidthKey = "Base.DataHubPackage.TreeWidth";
        private const string WindowTitle = "Data Hub";

        private static readonly GUIContent ProblemsContent = new(ProblemsLabel, ProblemsTooltip);

        [SerializeField] private bool problemsOnly;
        [SerializeField] private string search;
        [SerializeField] private string selectedPath;

        // None of these survive a domain reload, so EnsureInitialized creates them from the GUI pass too.
        private IDataHubPage _activePage;
        private DataCollections _collections;
        private EditorColumnDividers _dividers;
        private DataAssetIndex _index;
        private Object[] _inspected;
        private DataInspectorPanel _inspector;
        private float _inspectorWidth;
        private bool _isInitialized;
        private bool _isRenameRequested;
        private bool _needsRebuild;
        private bool _needsRevalidate;
        private bool _needsViewSwap;
        private Object _pendingSelection;
        private DataHubRegistry _registry;
        private Rect _renameButton;
        private DataHubStyles _styles;
        private DataHubTree _tree;
        private float _treeWidth;
        private DataValidation _validation;
        private DataTableView _view;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
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

            float toolbarHeight = EditorStyles.toolbar.fixedHeight;

            DrawToolbar(new Rect(0f, 0f, position.width, toolbarHeight));
            DrawPanes(new Rect(0f, toolbarHeight, position.width, position.height - toolbarHeight));
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
            _inspector?.Dispose();
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

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            _registry = new DataHubRegistry();
            _index = new DataAssetIndex();
            _collections = new DataCollections(_index);
            _validation = new DataValidation(_registry, _collections);
            _tree = new DataHubTree();
            _inspector = new DataInspectorPanel();
            _styles = new DataHubStyles();
            _dividers = new EditorColumnDividers();
            _inspected = Array.Empty<Object>();

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

        private void DrawToolbar(Rect area)
        {
            GUILayout.BeginArea(area, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            if (_view != null)
                DrawTableTools();

            GUILayout.FlexibleSpace();

            if (_view != null)
                DrawSummary();

            if (GUILayout.Button(RefreshLabel, EditorStyles.toolbarButton,
                    GUILayout.Width(EditorTableStyles.ToolbarButtonWidth)))
                _needsRebuild = true;

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawTableTools()
        {
            bool hasSelection = _view.SelectedAssets.Length > 0;

            if (GUILayout.Button(NewLabel, EditorStyles.toolbarButton))
                ShowNewMenu();

            using (new EditorGUI.DisabledScope(!hasSelection))
            {
                // Deferred, so the asset changes and the dialogs they may open never run halfway
                // through a GUI pass.
                if (GUILayout.Button(DuplicateLabel, EditorStyles.toolbarButton))
                    EditorApplication.delayCall += DuplicateSelection;

                using (new EditorGUI.DisabledScope(_view.SelectedAssets.Length != 1))
                {
                    _renameButton = GUILayoutUtility.GetRect(new GUIContent(RenameLabel), EditorStyles.toolbarButton);

                    if (GUI.Button(_renameButton, RenameLabel, EditorStyles.toolbarButton)
                        || (_isRenameRequested && Event.current.type == EventType.Repaint))
                        ShowRenamePopup();
                }

                if (GUILayout.Button(DeleteLabel, EditorStyles.toolbarButton))
                    EditorApplication.delayCall += DeleteSelection;
            }

            GUILayout.Space(EditorTableStyles.OuterMargin);

            EditorGUI.BeginChangeCheck();

            Rect searchRect = GUILayoutUtility.GetRect(EditorTableStyles.SearchWidth, EditorStyles.toolbar.fixedHeight,
                GUILayout.Width(EditorTableStyles.SearchWidth));

            search = EditorSearchField.Draw(_styles, searchRect, search, SearchPrompt);
            problemsOnly = GUILayout.Toggle(problemsOnly, ProblemsContent, EditorStyles.toolbarButton);

            if (EditorGUI.EndChangeCheck())
                _view.SetFilter(search, problemsOnly);
        }

        private void DrawSummary()
        {
            string summary = _view.ProblemCount > 0
                ? string.Format(SummaryProblemsFormat, _view.FilteredCount, _view.RowCount, _view.ProblemCount)
                : string.Format(SummaryFormat, _view.FilteredCount, _view.RowCount);

            GUILayout.Label(summary, _styles.Footer);
        }

        private void DrawPanes(Rect area)
        {
            bool hasInspector = _view != null;

            float inspectorWidth = hasInspector
                ? ClampPane(_inspectorWidth, area.width - _treeWidth - DataHubStyles.MinPaneWidth)
                : 0f;

            float treeWidth = ClampPane(_treeWidth, area.width - inspectorWidth - DataHubStyles.MinPaneWidth);

            Rect tree = new(area.x, area.y, treeWidth, area.height);
            Rect content = new(tree.xMax, area.y, area.width - treeWidth - inspectorWidth, area.height);
            Rect inspector = new(content.xMax, area.y, inspectorWidth, area.height);

            if (_tree.Draw(tree, _styles))
                OnTreeSelectionChanged();

            DrawContent(content, inspector);
            HandlePaneDividers(area, tree.xMax, inspector.x, hasInspector);
        }

        private void DrawContent(Rect content, Rect inspector)
        {
            if (_activePage != null)
            {
                GUILayout.BeginArea(content);
                _activePage.Draw();
                GUILayout.EndArea();

                return;
            }

            if (_view == null)
                return;

            if (_view.Draw(content, _styles, _validation, ShowRowMenu))
                Repaint();

            if (_inspector.Draw(inspector, _inspected, _styles))
                _needsRevalidate = true;

            // A sort click or an edit is only applied on the next layout, so that pass has to come.
            if (Event.current.type == EventType.Used)
                Repaint();
        }

        private void HandlePaneDividers(Rect area, float treeEdge, float inspectorEdge, bool hasInspector)
        {
            EEditorDividerAction treeAction = _dividers.Handle(TreeDivider, treeEdge, area, out float treeMouse);

            if (treeAction != EEditorDividerAction.None)
                _treeWidth = treeMouse - area.x;

            if (treeAction == EEditorDividerAction.Released)
                EditorPrefs.SetFloat(TreeWidthKey, _treeWidth);

            if (!hasInspector)
                return;

            EEditorDividerAction inspectorAction = _dividers.Handle(InspectorDivider, inspectorEdge, area,
                out float inspectorMouse);

            if (inspectorAction != EEditorDividerAction.None)
                _inspectorWidth = area.xMax - inspectorMouse;

            if (inspectorAction == EEditorDividerAction.Released)
                EditorPrefs.SetFloat(InspectorWidthKey, _inspectorWidth);
        }

        private void OnTreeSelectionChanged()
        {
            selectedPath = _tree.Selected?.Path;
            _needsViewSwap = true;

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

        // The popup has to open from inside a GUI pass, which a context menu callback is not, so the
        // menu only asks for it and the toolbar opens it under its own button.
        private void RequestRename()
        {
            _isRenameRequested = true;
            Repaint();
        }

        private void ShowRenamePopup()
        {
            _isRenameRequested = false;

            if (_view.SelectedAssets.Length != 1)
                return;

            Object asset = _view.SelectedAssets[0];

            PopupWindow.Show(_renameButton, new DataRenamePopup(asset, name =>
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
            menu.ShowAsContext();
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