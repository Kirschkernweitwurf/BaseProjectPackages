using System.Collections.Generic;
using Base.ToolsPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolsPackage.Editor.CodebaseGraph.Architecture;
using Base.ToolsPackage.Editor.CodebaseGraph.Editing;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolsPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Editor window that shows the project as a graph of namespaces, types and members, with the
    /// usages that connect them. Everything it reports is read out of compiled metadata, so reflection,
    /// SendMessage, inspector wired UnityEvents and asset references stay invisible. Treat findings as
    /// candidates worth a look, not as proof.
    /// </summary>
    internal sealed class CodebaseGraphWindow : EditorWindow
    {
        private const string ClearFocusLabel = "Clear focus";
        private const int DefaultDetailHeight = 300;
        private const int DefaultListWidth = 340;
        private const string EmptyFilterText = "Nothing matches the current filters.";

        private const string EmptyScanText = "No scan yet. Reading every compiled method body takes a few "
            + "seconds, so it does not run on its own.";

        private const string ExportScopeHelp = "Open a namespace first, or pick an assembly in the "
            + "toolbar. Then press this again.";

        private const string ExportScopeLabel = "Export scope";
        private const string MenuPath = "Tools/Base Packages/Code/Health/Codebase Graph";
        private const string MiniMapHiddenLabel = "Minimap";
        private const string MiniMapShownLabel = "Minimap, click to hide";
        private const float MinimumWindowHeight = 560f;
        private const float MinimumWindowWidth = 1100f;
        private const string MissingSheetMessage = "The codebase graph style sheet was not found, so the "
            + "window is drawn unstyled.";
        private const string ScanLabel = "Scan project";
        private const long SearchDebounceMilliseconds = 180;

        private const string WindowTitle = "Codebase Graph";

        [SerializeField] private EGraphScope savedScope;
        [SerializeField] private EEdgeMode savedEdgeMode;
        [SerializeField] private ELayoutMode savedLayoutMode;
        [SerializeField] private ESearchScope savedSearchScope;
        [SerializeField] private EFinding savedFinding;
        [SerializeField] private int savedHops = 1;
        [SerializeField] private string savedAssembly;
        [SerializeField] private string savedNamespace;
        [SerializeField] private string savedSearch;
        [SerializeField] private string savedTypeName;

        private bool IsSearching => !string.IsNullOrEmpty(_filter.Search)
            && _filter.SearchScope != ESearchScope.CurrentLevel;

        private readonly CodebaseGraphNavigation _navigation = new();
        private readonly GraphFilter _filter = new();

        private CodebaseGraphData _graph;
        private List<GraphEntry> _entries = new();
        private GraphEntry _selectedEntry;
        private CodebaseGraphToolbar _toolbar;
        private CodebaseGraphView _graphView;
        private CodebaseGraphListPane _listPane;
        private CodebaseGraphTabbedPane _tabbedPane;
        private CodebaseGraphDetailPane _detailPane;
        private CodebaseGraphBreadcrumb _breadcrumb;
        private Label _statusLabel;
        private VisualElement _emptyState;
        private Label _emptyLabel;
        private Button _miniMapButton;
        private bool _isMiniMapVisible = true;
        private IVisualElementScheduledItem _searchDebounce;
        private int _searchTotal;
        private int _typeTotal;

#region Unity Callbacks
        private void OnDisable() => DismissalStore.Changed -= ApplyFilter;

        private void CreateGUI()
        {
            rootVisualElement.AddToClassList(CodebaseGraphStyle.CodebaseGraphRootClass);

            _toolbar = new CodebaseGraphToolbar(_filter, BuildToolbarActions());
            rootVisualElement.Add(_toolbar);

            _breadcrumb = new CodebaseGraphBreadcrumb(OnBreadcrumbClicked);
            _breadcrumb.AddFocusControl(_toolbar.CreateNeighborField());
            _breadcrumb.AddFocusControl(new Button(ClearFocus)
            {
                text = ClearFocusLabel
            });

            rootVisualElement.Add(_breadcrumb);

            rootVisualElement.Add(BuildBody());
            rootVisualElement.Add(BuildStatusBar());

            // After the tree exists, so the first paint reaches every element rather than only the
            // root, and so the window follows the theme from here on.
            if (!CodebaseGraphStyle.Apply(rootVisualElement))
                CustomLogger.LogWarning(MissingSheetMessage, this);

            // The cache is dropped on every domain reload, so scanning here would mean a full project
            // wide IL walk behind a modal bar after every single script save. It waits to be asked.
            DismissalStore.Changed += ApplyFilter;
            _graph = CodebaseGraphCache.Get();

            if (_graph != null)
            {
                _toolbar.SetAssemblies(_graph.ScannedAssemblies);
                RestoreNavigation();
            }

            ApplyFilter();
        }
#endregion

        /// <summary>Opens the window.</summary>
        [DynamicMenuItem(MenuPath)]
        internal static void Open()
        {
            CodebaseGraphWindow window = GetWindow<CodebaseGraphWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(MinimumWindowWidth, MinimumWindowHeight);
        }

        private static bool ReportProgress(float progress, string status)
            => !EditorUtility.DisplayCancelableProgressBar(WindowTitle, status, progress);

        private static void OnQuickFixRequested(GraphEntry entry, EFinding finding)
        {
            if (CodebaseGraphQuickFix.Apply(entry.Type, entry.Member, finding))
                AssetDatabase.Refresh();
        }

        private static string ResolveIdentity(GraphEntry entry)
        {
            if (entry.Member != null && entry.Type != null)
                return GraphIdentity.ForMember(entry.Type, entry.Member);

            if (entry.Type != null)
                return GraphIdentity.ForType(entry.Type);

            return entry.Namespace == null
                ? null
                : GraphIdentity.ForNamespace(entry.Namespace.Name);
        }

        private static void OnOpenRequested(GraphEntry entry)
        {
            if (entry.Type == null)
                return;

            MemberSourceEditor.OpenAtMember(entry.Type, entry.Member);
        }

        private VisualElement BuildBody()
        {
            TwoPaneSplitView split = new(0, DefaultListWidth, TwoPaneSplitViewOrientation.Horizontal)
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            TwoPaneSplitView leftColumn = new(1, DefaultDetailHeight, TwoPaneSplitViewOrientation.Vertical);

            _listPane = new CodebaseGraphListPane(OnEntrySelected, OnEntryActivated, OnOnlyNewChanged);
            _tabbedPane = new CodebaseGraphTabbedPane(_listPane);
            leftColumn.Add(_tabbedPane);

            _detailPane = new CodebaseGraphDetailPane(OnFocusRequested,
                OnDrillDownRequested,
                OnOpenRequested,
                OnQuickFixRequested,
                OnDismissRequested,
                OnDismissFindingRequested,
                OnRestoreRequested);

            leftColumn.Add(_detailPane);
            split.Add(leftColumn);

            VisualElement graphHost = new()
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            _graphView = new CodebaseGraphView(OnEntrySelected,
                OnFocusRequested,
                OnDrillDownRequested,
                OnOpenRequested,
                OnDismissRequested);

            graphHost.Add(_graphView);

            graphHost.Add(BuildEmptyState());
            graphHost.Add(BuildMiniMapToggle());
            graphHost.Add(new CodebaseGraphLegend());

            split.Add(graphHost);
            return split;
        }

        private VisualElement BuildEmptyState()
        {
            _emptyState = new VisualElement();
            _emptyState.AddToClassList(CodebaseGraphStyle.EmptyStateClass);

            _emptyLabel = new Label(EmptyScanText);
            _emptyState.Add(_emptyLabel);
            _emptyState.Add(new Button(Rescan)
            {
                text = ScanLabel
            });

            return _emptyState;
        }

        private VisualElement BuildMiniMapToggle()
        {
            _miniMapButton = new Button(ToggleMiniMap)
            {
                text = MiniMapShownLabel
            };

            _miniMapButton.AddToClassList(CodebaseGraphStyle.MinimapToggleClass);

            return _miniMapButton;
        }

        private void ToggleMiniMap()
        {
            _isMiniMapVisible = !_isMiniMapVisible;

            _miniMapButton.text = _isMiniMapVisible
                ? MiniMapShownLabel
                : MiniMapHiddenLabel;

            _graphView.SetMiniMapVisible(_isMiniMapVisible);
        }

        private VisualElement BuildStatusBar()
        {
            _statusLabel = new Label(string.Empty);
            _statusLabel.AddToClassList(CodebaseGraphStyle.StatusBarClass);
            return _statusLabel;
        }

        private void Rescan()
        {
            CodebaseGraphData scanned;

            try
            {
                scanned = CodebaseGraphBuilder.Build(ReportProgress);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // A canceled scan leaves whatever was there before rather than emptying the window.
            if (scanned == null)
                return;

            // The comparison has to happen before the ids are replaced, since one is the baseline for
            // the other.
            FindingBaseline.Apply(scanned, FindingBaseline.Read());
            FindingBaseline.Write(FindingBaseline.Collect(scanned));

            _graph = scanned;
            CodebaseGraphCache.Set(_graph);
            _tabbedPane.SetGraph(_graph);

            _toolbar.SetAssemblies(_graph.ScannedAssemblies);
            RestoreNavigation();
            ApplyFilter();
        }

        /// <summary>
        /// Puts the window back where it was. Editor windows survive a domain reload but the scan does
        /// not, so after every recompile the graph is rebuilt and the saved path is walked again.
        /// </summary>
        private void RestoreNavigation()
        {
            ClearSelection();
            _navigation.ClearFocus();

            _navigation.CurrentNamespace = string.IsNullOrEmpty(savedNamespace)
                ? null
                : savedNamespace;

            _navigation.CurrentType = CodebaseGraphNavigation.FindType(_graph, savedTypeName);
            _navigation.Scope = savedScope;

            if (_navigation.Scope == EGraphScope.Member && _navigation.CurrentType == null)
                _navigation.Scope = EGraphScope.Type;

            RestoreFilter();
        }

        private void RestoreFilter()
        {
            _filter.EdgeMode = savedEdgeMode;
            _filter.LayoutMode = savedLayoutMode;
            _filter.SearchScope = savedSearchScope;
            _filter.Finding = savedFinding;
            _filter.Hops = Mathf.Clamp(savedHops, 1, CodebaseGraphToolbar.ReadNeighborMaximum());
            _filter.Search = savedSearch ?? string.Empty;
            _filter.AssemblyName = _toolbar.HasAssembly(savedAssembly)
                ? savedAssembly
                : null;

            _toolbar.Sync();
        }

        private void SaveNavigation()
        {
            savedScope = _navigation.Scope;
            savedNamespace = _navigation.CurrentNamespace;
            savedTypeName = _navigation.CurrentType?.FullName;
            savedAssembly = _filter.AssemblyName;
            savedEdgeMode = _filter.EdgeMode;
            savedLayoutMode = _filter.LayoutMode;
            savedSearchScope = _filter.SearchScope;
            savedFinding = _filter.Finding;
            savedHops = _filter.Hops;
            savedSearch = _filter.Search;
        }

        private void ClearSelection()
        {
            _selectedEntry = null;
            _detailPane.Show(null, _graph);
        }

        /// <summary>
        /// Redraws the detail pane for whatever is selected, using the entry the rebuild just produced.
        /// Dismissing something is a decision about the thing on screen, and losing the thing on screen
        /// as a result means checking the decision took effect is a hunt.
        /// </summary>
        private void RefreshSelection()
        {
            if (_selectedEntry == null)
                return;

            foreach (GraphEntry entry in _entries)
            {
                if (entry.Id != _selectedEntry.Id)
                    continue;

                _selectedEntry = entry;
                _detailPane.Show(entry, _graph);

                return;
            }

            ClearSelection();
        }

        private void ApplyFilter()
        {
            if (_graphView == null)
                return;

            if (_graph == null)
            {
                ShowEmptyState(EmptyScanText);
                return;
            }

            // Someone may have edited the dismissal file by hand since the last rebuild.
            DismissalStore.Refresh();

            List<GraphEntry> entries = BuildEntries();
            _entries = entries;

            string heading = _navigation.Heading(IsSearching, _filter.Search, entries.Count,
                _searchTotal, _typeTotal);

            _listPane.SetEntries(heading, entries);
            _graphView.SetEdgeMode(_filter.EdgeMode);
            _graphView.SetLayoutMode(_filter.LayoutMode);
            _graphView.Rebuild(entries, _navigation.FocusedId);
            SetEmptyStateVisible(entries.Count == 0, EmptyFilterText);

            _breadcrumb.SetPath(_navigation.Path(IsSearching, _filter.Search));
            _breadcrumb.SetFocus(_navigation.FocusNotice(_filter.Hops));

            _tabbedPane.SetCounts();
            UpdateStatus(entries.Count);
            SaveNavigation();
        }

        private void ShowEmptyState(string text)
        {
            _listPane.SetEntries(string.Empty, new List<GraphEntry>());
            _graphView.Rebuild(new List<GraphEntry>(), null);
            _breadcrumb.SetPath(_navigation.Path(IsSearching, _filter.Search));
            _breadcrumb.SetFocus(string.Empty);
            _statusLabel.text = string.Empty;
            SetEmptyStateVisible(true, text);
        }

        private void SetEmptyStateVisible(bool isVisible, string text)
        {
            _emptyLabel.text = text;
            _emptyState.style.display = isVisible
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private List<GraphEntry> BuildEntries()
        {
            if (IsSearching)
            {
                List<GraphEntry> found = GraphEntryFactory.BuildSearch(_graph, _filter, out int total);
                _searchTotal = total;
                return found;
            }

            switch (_navigation.Scope)
            {
                case EGraphScope.Type:
                    return GraphEntryFactory.BuildTypes(_graph,
                        _filter,
                        _navigation.CurrentNamespace,
                        _navigation.FocusedType,
                        out _typeTotal);

                case EGraphScope.Member:
                    return GraphEntryFactory.BuildMembers(_graph, _filter, _navigation.CurrentType,
                        _navigation.FocusedMember);

                default:
                    return GraphEntryFactory.BuildNamespaces(_graph, _filter, _navigation.FocusedNamespace);
            }
        }

        private void UpdateStatus(int shownCount) => _statusLabel.text =
            $"{shownCount} shown        {_graph.TypeCount} types, "
            + $"{_graph.MemberCount} members        {_graph.CountTypeIssues()} type findings, "
            + $"{_graph.CountMemberIssues()} member findings        scanned in {_graph.ScanSeconds:0.0}s, "
            + $"{_graph.UnresolvedTokenCount} tokens skipped, "
            + $"{_graph.FieldsCreditedByType} fields by type, "
            + $"{_graph.FieldsCreditedByNestedType} by name under a known script, "
            + $"{_graph.FieldsCreditedByUnknownScript} by name under an unresolved script";

        private void OnBreadcrumbClicked(int index)
        {
            _navigation.ClearFocus();
            ClearSelection();

            if (index == 0)
            {
                _navigation.Scope = EGraphScope.Namespace;
                _navigation.CurrentNamespace = null;
                _navigation.CurrentType = null;
            }
            else
            {
                _navigation.Scope = EGraphScope.Type;
                _navigation.CurrentType = null;
            }

            ApplyFilter();
        }

        private void GoBack()
        {
            _navigation.ClearFocus();
            ClearSelection();

            switch (_navigation.Scope)
            {
                case EGraphScope.Member:
                    _navigation.Scope = EGraphScope.Type;
                    _navigation.CurrentType = null;
                    break;

                case EGraphScope.Type:
                    _navigation.Scope = EGraphScope.Namespace;
                    _navigation.CurrentNamespace = null;
                    break;
            }

            ApplyFilter();
        }

        private void ClearFocus()
        {
            if (!_navigation.HasFocus)
                return;

            _navigation.ClearFocus();
            ApplyFilter();
        }

        private void OnEntrySelected(GraphEntry entry)
        {
            _selectedEntry = entry;
            _detailPane.Show(entry, _graph);
        }

        private void OnEntryActivated(GraphEntry entry)
        {
            if (entry.CanDrillDown)
            {
                OnDrillDownRequested(entry);
                return;
            }

            OnOpenRequested(entry);
        }

        private void OnDrillDownRequested(GraphEntry entry)
        {
            if (!entry.CanDrillDown)
                return;

            _navigation.ClearFocus();
            _toolbar.ClearSearch();

            if (entry.Namespace != null)
            {
                _navigation.Scope = EGraphScope.Type;
                _navigation.CurrentNamespace = entry.Namespace.Name;
            }
            else if (entry.Type != null)
            {
                _navigation.Scope = EGraphScope.Member;
                _navigation.CurrentType = entry.Type;
            }

            ClearSelection();
            ApplyFilter();
        }

        private void OnFocusRequested(GraphEntry entry)
        {
            if (!TrySetFocus(entry))
                return;

            ApplyFilter();
            OnEntrySelected(entry);
        }

        /// <summary>Points the graph at one entry, or clears it again when the same one is picked twice.</summary>
        /// <param name="entry">Entry to center the graph on.</param>
        /// <returns>True when the focus changed and the view needs a rebuild.</returns>
        private bool TrySetFocus(GraphEntry entry)
        {
            if (entry.Member != null)
            {
                _navigation.FocusedMember = _navigation.FocusedMember == entry.Member
                    ? null
                    : entry.Member;

                return true;
            }

            if (entry.Namespace != null)
            {
                _navigation.FocusedNamespace = _navigation.FocusedNamespace == entry.Namespace
                    ? null
                    : entry.Namespace;

                return true;
            }

            if (entry.Type == null)
                return false;

            _navigation.FocusedType = _navigation.FocusedType == entry.Type
                ? null
                : entry.Type;

            return true;
        }

        private void OnDismissRequested(GraphEntry entry, bool includeContents)
        {
            string id = ResolveIdentity(entry);
            if (id == null)
                return;

            DismissalStore.Dismiss(id, includeContents);
            ApplyFilter();
            RefreshSelection();
        }

        private void OnDismissFindingRequested(GraphEntry entry, EFinding finding)
        {
            string id = ResolveIdentity(entry);
            if (id == null)
                return;

            DismissalStore.Dismiss(GraphIdentity.ForFinding(id, finding), false);
            ApplyFilter();
            RefreshSelection();
        }

        private void ExportFindings() => CodebaseGraphReportIo.Export(_graph);

        private void ExportAssemblyEdgeReport() => AssemblyEdgeReportCommand.Export(_graph);

        /// <summary>
        /// Exports whatever is currently in view. The namespace you have opened wins, then the assembly
        /// the toolbar is filtered to, because that is the order in which they narrow what is on screen.
        /// </summary>
        private void ExportScope()
        {
            if (!string.IsNullOrEmpty(_navigation.CurrentNamespace))
            {
                CodebaseGraphReportIo.ExportScope(_graph, _navigation.CurrentNamespace, false);
                return;
            }

            if (!string.IsNullOrEmpty(_filter.AssemblyName))
            {
                CodebaseGraphReportIo.ExportScope(_graph, _filter.AssemblyName, true);
                return;
            }

            EditorUtility.DisplayDialog(ExportScopeLabel, ExportScopeHelp, "OK");
        }

        private void ImportDismissals()
        {
            if (CodebaseGraphReportIo.Import())
                ApplyFilter();
        }

        private void OnRestoreRequested(GraphEntry entry)
        {
            string id = ResolveIdentity(entry);
            if (id == null)
                return;

            DismissalStore.Restore(id);
            ApplyFilter();
            RefreshSelection();
        }

        private void OnNeighborChanged()
        {
            if (_navigation.HasFocus)
                ApplyFilter();
        }

        private void OnSearchChanged()
        {
            // Rebuilding the entries, the layout and the graph on every keystroke is far too expensive.
            _searchDebounce ??= rootVisualElement.schedule.Execute(ApplyFilter);
            _searchDebounce.Pause();
            _searchDebounce.ExecuteLater(SearchDebounceMilliseconds);
        }

        private void OnOnlyNewChanged(bool value)
        {
            _filter.OnlyNew = value;
            ApplyFilter();
        }

        private void OnEdgeModeChanged() => _graphView.SetEdgeMode(_filter.EdgeMode);

        /// <summary>
        /// Gathers what the toolbar is allowed to ask for. Everything here is a decision about cost:
        /// changing a filter rebuilds the view, changing the line mode does not, and a keystroke waits
        /// to see whether more are coming.
        /// </summary>
        private CodebaseGraphToolbarActions BuildToolbarActions() => new()
        {
            FilterChanged = ApplyFilter,
            EdgeModeChanged = OnEdgeModeChanged,
            NeighborChanged = OnNeighborChanged,
            SearchChanged = OnSearchChanged,
            Back = GoBack,
            Rescan = Rescan,
            Export = ExportFindings,
            Import = ImportDismissals,
            ExportScope = ExportScope,
            AssemblyEdgeReport = ExportAssemblyEdgeReport
        };
    }
}