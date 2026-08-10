using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Editing;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Editor window that shows the project as a graph of namespaces, types and members, with the
    /// usages that connect them. Everything it reports is read out of compiled metadata, so reflection,
    /// SendMessage, inspector wired UnityEvents and asset references stay invisible. Treat findings as
    /// candidates worth a look, not as proof.
    /// </summary>
    public sealed class CodebaseGraphWindow : EditorWindow
    {
        private const string AllAssembliesLabel = "All assemblies";
        private const string AllTypesSegment = "All types";
        private const string BackLabel = "Back";
        private const string ClearFocusLabel = "Clear focus";
        private const int DefaultDetailHeight = 300;
        private const int DefaultListWidth = 340;
        private const string DefaultReportName = "CodebaseGraphFindings.md";
        private const string DemoteFixTitle = "Make member internal";
        private const string DismissedFormat = "Dismissed ({0})";
        private const string DismissedTooltip = "Opens the list of everything set aside, where entries can "
            + "be brought back one at a time or together with their contents.";
        private const string EmptyFilterText = "Nothing matches the current filters.";

        private const string EmptyScanText = "No scan yet. Reading every compiled method body takes a few "
            + "seconds, so it does not run on its own.";

        private const string EmptyStateClass = "empty-state";
        private const string ExportExtension = "md";
        private const string ExportLabel = "Export findings";
        private const string ExportTitle = "Save findings report";
        private const string FocusNoticeFormat = "showing {0} and its neighbors, {1} step{2} out";
        private const string ImportCancel = "Cancel";
        private const string ImportFromClipboard = "Paste from clipboard";
        private const string ImportFromFile = "Load from file";
        private const string ImportLabel = "Update dismissals";

        private const string ImportMessage = "Reads dismissal instructions, the same block the findings "
            + "report writes at the end. One instruction per line:\n\n"
            + "  dismiss <id>\n  dismiss-tree <id>\n  restore <id>\n  restore-tree <id>\n\n"
            + "Lines you leave out change nothing, so only restore removes a dismissal.";

        private const string ImportOpenTitle = "Open dismissal instructions";

        private const string ImportResultFormat = "Applied {0} instructions.\n{1} were ignored because the "
            + "verb was unknown, the id was malformed, or the entry was already in that state.";

        private const string MembersHeadingFormat = "Members of {0}";
        private const float MinimumWindowHeight = 560f;
        private const float MinimumWindowWidth = 1100f;
        private const string NamespacesHeadingFormat = "Namespaces ({0})";
        private const string NamespacesSegment = "All namespaces";
        private const string NeighborFormat = "Neighbors: {0}";
        private const int NeighborMaximum = 3;

        private const string NeighborTooltip = "How many steps out from the focused entry the view "
            + "reaches. One shows what it touches directly, three follows those connections two steps "
            + "further.";

        private const string PluralSuffix = "s";
        private const string PrivateFixTitle = "Make member private";
        private const string ReadOnlyFixTitle = "Make field readonly";
        private const string RefreshLabel = "Rescan";
        private const string RootClass = "codebase-graph-root";
        private const string ScanLabel = "Scan project";
        private const long SearchDebounceMilliseconds = 180;
        private const string SearchPlaceholder = "Search";
        private const string ShowDataLabel = "Fields";
        private const string ShowPrivateLabel = "Private";
        private const string StatusBarClass = "status-bar";
        private const string StyleSheetFilter = "CodebaseGraph t:StyleSheet";
        private const string ToolbarRowClass = "top-bar";
        private const string TypesHeadingFormat = "Types in {0}";
        private const string VerboseSuffix = "-Verbose.md";
        private const string WindowTitle = "Codebase Graph";

        private readonly GraphFilter _filter = new();

        [SerializeField] private EGraphScope savedScope;
        [SerializeField] private EFinding savedFinding;
        [SerializeField] private int savedHops = 1;
        [SerializeField] private string savedAssembly;
        [SerializeField] private string savedNamespace;
        [SerializeField] private string savedSearch;
        [SerializeField] private string savedTypeName;

        private CodebaseGraphData _graph;
        private CodebaseGraphView _graphView;
        private CodebaseGraphListPane _listPane;
        private CodebaseGraphDetailPane _detailPane;
        private CodebaseGraphBreadcrumb _breadcrumb;
        private Label _statusLabel;
        private VisualElement _emptyState;
        private Label _emptyLabel;
        private Button _restoreButton;
        private PopupField<string> _assemblyField;
        private PopupField<string> _findingField;
        private PopupField<string> _neighborField;
        private ToolbarSearchField _searchField;
        private IVisualElementScheduledItem _searchDebounce;

        private EGraphScope _scope = EGraphScope.Namespace;
        private string _currentNamespace;
        private TypeNodeInfo _currentType;
        private TypeNodeInfo _focusedType;
        private MemberNodeInfo _focusedMember;

        private bool HasFocus => _focusedType != null || _focusedMember != null;

#region Unity Callbacks
        private void CreateGUI()
        {
            rootVisualElement.AddToClassList(RootClass);
            LoadStyleSheet();

            rootVisualElement.Add(BuildToolbar());

            _breadcrumb = new CodebaseGraphBreadcrumb(OnBreadcrumbClicked);
            _breadcrumb.AddFocusControl(BuildNeighborField());
            _breadcrumb.AddFocusControl(new Button(ClearFocus) { text = ClearFocusLabel });
            rootVisualElement.Add(_breadcrumb);

            rootVisualElement.Add(BuildBody());
            rootVisualElement.Add(BuildStatusBar());

            // The cache is dropped on every domain reload, so scanning here would mean a full project
            // wide IL walk behind a modal bar after every single script save. It waits to be asked.
            DismissalStore.Changed += ApplyFilter;
            _graph = CodebaseGraphCache.Get();

            if (_graph != null)
            {
                RefreshAssemblyChoices();
                RestoreNavigation();
            }

            ApplyFilter();
        }

        private void OnDisable() => DismissalStore.Changed -= ApplyFilter;
#endregion

        /// <summary>Opens the window.</summary>
        [DynamicMenuItem("Tools/Base Packages/Unity Editor/Project Health/Codebase Graph")]
        public static void Open()
        {
            CodebaseGraphWindow window = GetWindow<CodebaseGraphWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(MinimumWindowWidth, MinimumWindowHeight);
        }

        private static ToolbarToggle BuildToggle(string label, bool initialValue, Action<bool> onChanged)
        {
            ToolbarToggle toggle = new()
            {
                text = label,
                value = initialValue
            };

            toggle.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            return toggle;
        }

        private static List<string> BuildNeighborChoices()
        {
            List<string> choices = new(NeighborMaximum);

            for (int hops = 1; hops <= NeighborMaximum; hops++)
                choices.Add(string.Format(NeighborFormat, hops));

            return choices;
        }

        private VisualElement BuildToolbar()
        {
            Toolbar toolbar = new();
            toolbar.AddToClassList(ToolbarRowClass);

            toolbar.Add(new ToolbarButton(GoBack) { text = BackLabel });
            toolbar.Add(new ToolbarButton(Rescan) { text = RefreshLabel });
            toolbar.Add(BuildSpacer());

            _assemblyField = new PopupField<string>(new List<string> { AllAssembliesLabel }, 0);
            _assemblyField.RegisterValueChangedCallback(OnAssemblyChanged);
            toolbar.Add(_assemblyField);

            _findingField = new PopupField<string>(FindingCatalog.BuildChoices(), 0);
            _findingField.RegisterValueChangedCallback(OnFindingChanged);
            toolbar.Add(_findingField);

            toolbar.Add(BuildToggle(ShowPrivateLabel, _filter.ShowPrivate, OnShowPrivateChanged));
            toolbar.Add(BuildToggle(ShowDataLabel, _filter.ShowDataMembers, OnShowDataChanged));

            _searchField = new ToolbarSearchField();
            _searchField.tooltip = SearchPlaceholder;
            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            toolbar.Add(_searchField);

            _restoreButton = new ToolbarButton(CodebaseGraphDismissalsWindow.Open)
            {
                text = string.Format(DismissedFormat, 0),
                tooltip = DismissedTooltip
            };
            toolbar.Add(_restoreButton);

            toolbar.Add(new ToolbarButton(ExportFindings) { text = ExportLabel });
            toolbar.Add(new ToolbarButton(ImportDismissals) { text = ImportLabel });

            return toolbar;
        }

        private PopupField<string> BuildNeighborField()
        {
            _neighborField = new PopupField<string>(BuildNeighborChoices(), 0);
            _neighborField.tooltip = NeighborTooltip;
            _neighborField.RegisterValueChangedCallback(OnNeighborChanged);
            return _neighborField;
        }

        private VisualElement BuildSpacer()
        {
            VisualElement spacer = new();
            spacer.style.flexGrow = 1f;
            return spacer;
        }

        private VisualElement BuildBody()
        {
            TwoPaneSplitView split = new(0, DefaultListWidth, TwoPaneSplitViewOrientation.Horizontal);
            split.style.flexGrow = 1f;

            TwoPaneSplitView leftColumn = new(1, DefaultDetailHeight, TwoPaneSplitViewOrientation.Vertical);

            _listPane = new CodebaseGraphListPane(OnEntrySelected, OnEntryActivated);
            leftColumn.Add(_listPane);

            _detailPane = new CodebaseGraphDetailPane(OnFocusRequested,
                OnDrillDownRequested,
                OnOpenRequested,
                OnQuickFixRequested,
                OnDismissRequested);

            leftColumn.Add(_detailPane);
            split.Add(leftColumn);

            VisualElement graphHost = new();
            graphHost.style.flexGrow = 1f;

            _graphView = new CodebaseGraphView(OnEntrySelected,
                OnFocusRequested,
                OnDrillDownRequested,
                OnOpenRequested,
                OnDismissRequested);

            graphHost.Add(_graphView);

            graphHost.Add(BuildEmptyState());

            split.Add(graphHost);
            return split;
        }

        private VisualElement BuildEmptyState()
        {
            _emptyState = new VisualElement();
            _emptyState.AddToClassList(EmptyStateClass);

            _emptyLabel = new Label(EmptyScanText);
            _emptyState.Add(_emptyLabel);
            _emptyState.Add(new Button(Rescan) { text = ScanLabel });

            return _emptyState;
        }

        private VisualElement BuildStatusBar()
        {
            _statusLabel = new Label(string.Empty);
            _statusLabel.AddToClassList(StatusBarClass);
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

            // A cancelled scan leaves whatever was there before rather than emptying the window.
            if (scanned == null)
                return;

            _graph = scanned;
            CodebaseGraphCache.Set(_graph);

            RefreshAssemblyChoices();
            RestoreNavigation();
            ApplyFilter();
        }

        private bool ReportProgress(float progress, string status)
            => !EditorUtility.DisplayCancelableProgressBar(WindowTitle, status, progress);

        /// <summary>
        /// Puts the window back where it was. Editor windows survive a domain reload but the scan does
        /// not, so after every recompile the graph is rebuilt and the saved path is walked again.
        /// </summary>
        private void RestoreNavigation()
        {
            ClearSelection();
            ClearFocusState();

            _currentNamespace = string.IsNullOrEmpty(savedNamespace)
                ? null
                : savedNamespace;

            _currentType = FindTypeByFullName(savedTypeName);
            _scope = savedScope;

            if (_scope == EGraphScope.Member && _currentType == null)
                _scope = EGraphScope.Type;

            RestoreFilter();
        }

        private void RestoreFilter()
        {
            _filter.Finding = savedFinding;
            _filter.Hops = Mathf.Clamp(savedHops, 1, NeighborMaximum);
            _filter.Search = savedSearch ?? string.Empty;
            _filter.AssemblyName = _assemblyField.choices.Contains(savedAssembly)
                ? savedAssembly
                : null;

            _findingField.index = FindingCatalog.GetIndex(_filter.Finding);
            _neighborField.SetValueWithoutNotify(string.Format(NeighborFormat, _filter.Hops));
            _searchField.SetValueWithoutNotify(_filter.Search);
            _assemblyField.SetValueWithoutNotify(_filter.AssemblyName ?? AllAssembliesLabel);
        }

        private void SaveNavigation()
        {
            savedScope = _scope;
            savedNamespace = _currentNamespace;
            savedTypeName = _currentType?.FullName;
            savedAssembly = _filter.AssemblyName;
            savedFinding = _filter.Finding;
            savedHops = _filter.Hops;
            savedSearch = _filter.Search;
        }

        private TypeNodeInfo FindTypeByFullName(string fullName)
        {
            if (_graph == null || string.IsNullOrEmpty(fullName))
                return null;

            foreach (TypeNodeInfo type in _graph.Types.Values)
            {
                if (type.FullName == fullName)
                    return type;
            }

            return null;
        }

        private void ClearSelection() => _detailPane.Show(null, _graph);

        private void ClearFocusState()
        {
            _focusedType = null;
            _focusedMember = null;
        }

        private void RefreshAssemblyChoices()
        {
            List<string> choices = new() { AllAssembliesLabel };
            choices.AddRange(_graph.ScannedAssemblies);

            _assemblyField.choices = choices;
            _assemblyField.SetValueWithoutNotify(AllAssembliesLabel);
            _filter.AssemblyName = null;
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

            _listPane.SetEntries(BuildHeading(entries.Count), entries);
            _graphView.Rebuild(entries, ResolveFocusedId());
            SetEmptyStateVisible(entries.Count == 0, EmptyFilterText);

            _breadcrumb.SetPath(BuildPath());
            _breadcrumb.SetFocus(BuildFocusNotice());

            _restoreButton.text = string.Format(DismissedFormat, DismissalStore.Count);
            UpdateStatus(entries.Count);
            SaveNavigation();
        }

        private void ShowEmptyState(string text)
        {
            _listPane.SetEntries(string.Empty, new List<GraphEntry>());
            _graphView.Rebuild(new List<GraphEntry>(), null);
            _breadcrumb.SetPath(BuildPath());
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
            switch (_scope)
            {
                case EGraphScope.Type:
                    return GraphEntryFactory.BuildTypes(_graph, _filter, _currentNamespace, _focusedType);

                case EGraphScope.Member:
                    return GraphEntryFactory.BuildMembers(_graph, _filter, _currentType, _focusedMember);

                default:
                    return GraphEntryFactory.BuildNamespaces(_graph, _filter);
            }
        }

        private string BuildHeading(int shownCount)
        {
            switch (_scope)
            {
                case EGraphScope.Type:
                    return string.Format(TypesHeadingFormat, _currentNamespace ?? AllTypesSegment);

                case EGraphScope.Member:
                    return _currentType == null
                        ? string.Empty
                        : string.Format(MembersHeadingFormat, _currentType.ShortName);

                default:
                    return string.Format(NamespacesHeadingFormat, shownCount);
            }
        }

        private List<string> BuildPath()
        {
            List<string> path = new() { NamespacesSegment };

            if (_scope == EGraphScope.Namespace)
                return path;

            path.Add(_currentNamespace ?? AllTypesSegment);

            if (_scope == EGraphScope.Member && _currentType != null)
                path.Add(_currentType.ShortName);

            return path;
        }

        private string BuildFocusNotice()
        {
            string name = _focusedMember?.Name ?? _focusedType?.ShortName;
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            string plural = _filter.Hops == 1
                ? string.Empty
                : PluralSuffix;

            return string.Format(FocusNoticeFormat, name, _filter.Hops, plural);
        }

        private string ResolveFocusedId()
        {
            if (_focusedMember != null)
                return GraphEntryFactory.MakeMemberId(_focusedMember.Key);

            return _focusedType != null
                ? GraphEntryFactory.MakeTypeId(_focusedType.Key)
                : null;
        }

        private void UpdateStatus(int shownCount)
            => _statusLabel.text = $"{shownCount} shown        {_graph.TypeCount} types, "
                + $"{_graph.MemberCount} members        {_graph.CountTypeIssues()} type findings, "
                + $"{_graph.CountMemberIssues()} member findings        scanned in {_graph.ScanSeconds:0.0}s, "
                + $"{_graph.UnresolvedTokenCount} tokens skipped";

        private void OnBreadcrumbClicked(int index)
        {
            ClearFocusState();
            ClearSelection();

            if (index == 0)
            {
                _scope = EGraphScope.Namespace;
                _currentNamespace = null;
                _currentType = null;
            }
            else
            {
                _scope = EGraphScope.Type;
                _currentType = null;
            }

            ApplyFilter();
        }

        private void GoBack()
        {
            ClearFocusState();
            ClearSelection();

            switch (_scope)
            {
                case EGraphScope.Member:
                    _scope = EGraphScope.Type;
                    _currentType = null;
                    break;

                case EGraphScope.Type:
                    _scope = EGraphScope.Namespace;
                    _currentNamespace = null;
                    break;
            }

            ApplyFilter();
        }

        private void ClearFocus()
        {
            if (!HasFocus)
                return;

            ClearFocusState();
            ApplyFilter();
        }

        private void OnEntrySelected(GraphEntry entry)
        {
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

            ClearFocusState();

            if (entry.Namespace != null)
            {
                _scope = EGraphScope.Type;
                _currentNamespace = entry.Namespace.Name;
            }
            else if (entry.Type != null)
            {
                _scope = EGraphScope.Member;
                _currentType = entry.Type;
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
                _focusedMember = _focusedMember == entry.Member
                    ? null
                    : entry.Member;

                return true;
            }

            if (entry.Type == null || _scope != EGraphScope.Type)
                return false;

            _focusedType = _focusedType == entry.Type
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
            ClearSelection();
            ApplyFilter();
        }

        private string ResolveIdentity(GraphEntry entry)
        {
            if (entry.Member != null && entry.Type != null)
                return GraphIdentity.ForMember(entry.Type, entry.Member);

            if (entry.Type != null)
                return GraphIdentity.ForType(entry.Type);

            return entry.Namespace == null
                ? null
                : GraphIdentity.ForNamespace(entry.Namespace.Name);
        }

        private void ExportFindings()
        {
            if (_graph == null)
                return;

            string path = EditorUtility.SaveFilePanel(ExportTitle,
                string.Empty,
                DefaultReportName,
                ExportExtension);

            if (string.IsNullOrEmpty(path))
                return;

            File.WriteAllText(path, FindingReportWriter.BuildMain(_graph));

            // Low confidence findings outnumber the useful ones many times over, so they get their own
            // file rather than burying the report they sit next to.
            string verbosePath = Path.ChangeExtension(path, null) + VerboseSuffix;
            File.WriteAllText(verbosePath, FindingReportWriter.BuildVerbose(_graph));

            EditorUtility.RevealInFinder(path);
        }

        private void ImportDismissals()
        {
            int choice = EditorUtility.DisplayDialogComplex(ImportLabel,
                ImportMessage,
                ImportFromClipboard,
                ImportCancel,
                ImportFromFile);

            if (choice == 1)
                return;

            string text = choice == 0
                ? EditorGUIUtility.systemCopyBuffer
                : ReadInstructionFile();

            if (string.IsNullOrEmpty(text))
                return;

            DismissalTextFormat.Apply(text, out int applied, out int ignored);
            ApplyFilter();

            EditorUtility.DisplayDialog(ImportLabel,
                string.Format(ImportResultFormat, applied, ignored),
                "OK");
        }

        private string ReadInstructionFile()
        {
            string path = EditorUtility.OpenFilePanel(ImportOpenTitle, string.Empty, ExportExtension);

            return string.IsNullOrEmpty(path) || !File.Exists(path)
                ? string.Empty
                : File.ReadAllText(path);
        }

        private void OnOpenRequested(GraphEntry entry)
        {
            if (entry.Type == null)
                return;

            MemberSourceEditor.OpenAtMember(entry.Type, entry.Member);
        }

        private void OnQuickFixRequested(GraphEntry entry, EFinding finding)
        {
            if (entry.Member == null || entry.Type == null)
                return;

            bool confirmed = EditorUtility.DisplayDialog(BuildFixTitle(finding),
                BuildFixMessage(entry, finding),
                "Apply",
                "Cancel");

            if (!confirmed)
                return;

            if (ApplyFix(entry, finding))
                AssetDatabase.Refresh();
        }

        private bool ApplyFix(GraphEntry entry, EFinding finding)
        {
            switch (finding)
            {
                case EFinding.PrivateCandidate:
                    return MemberSourceEditor.DemoteToPrivate(entry.Type, entry.Member);

                case EFinding.PublicButInternalOnly:
                    return MemberSourceEditor.DemoteToInternal(entry.Type, entry.Member);

                default:
                    return MemberSourceEditor.AddReadOnly(entry.Type, entry.Member);
            }
        }

        private string BuildFixTitle(EFinding finding)
        {
            switch (finding)
            {
                case EFinding.PrivateCandidate:
                    return PrivateFixTitle;

                case EFinding.PublicButInternalOnly:
                    return DemoteFixTitle;

                default:
                    return ReadOnlyFixTitle;
            }
        }

        private string BuildFixMessage(GraphEntry entry, EFinding finding)
        {
            string change = BuildFixChange(finding);

            return $"This edits the source of {entry.Member.Name} in {entry.Type.ShortName}, {change}."
                + "\n\nThe declaration is matched by name, and the edit is refused when more than one "
                + "line matches or when the member declares its own accessor visibility. Commit your work "
                + "first, then let Unity recompile and check the console.";
        }

        private string BuildFixChange(EFinding finding)
        {
            switch (finding)
            {
                case EFinding.PrivateCandidate:
                    return "lowering it to private";

                case EFinding.PublicButInternalOnly:
                    return "changing public to internal";

                default:
                    return "adding readonly";
            }
        }

        private void OnAssemblyChanged(ChangeEvent<string> evt)
        {
            _filter.AssemblyName = evt.newValue == AllAssembliesLabel
                ? null
                : evt.newValue;

            ApplyFilter();
        }

        private void OnFindingChanged(ChangeEvent<string> evt)
        {
            _filter.Finding = FindingCatalog.GetAt(_findingField.index);
            ApplyFilter();
        }

        private void OnNeighborChanged(ChangeEvent<string> evt)
        {
            _filter.Hops = _neighborField.index + 1;

            if (HasFocus)
                ApplyFilter();
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _filter.Search = evt.newValue ?? string.Empty;

            // Rebuilding the entries, the layout and the graph on every keystroke is far too expensive.
            _searchDebounce ??= rootVisualElement.schedule.Execute(ApplyFilter);
            _searchDebounce.Pause();
            _searchDebounce.ExecuteLater(SearchDebounceMilliseconds);
        }

        private void OnShowPrivateChanged(bool value)
        {
            _filter.ShowPrivate = value;
            ApplyFilter();
        }

        private void OnShowDataChanged(bool value)
        {
            _filter.ShowDataMembers = value;
            ApplyFilter();
        }

        private void LoadStyleSheet()
        {
            foreach (string guid in AssetDatabase.FindAssets(StyleSheetFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (sheet == null)
                    continue;

                rootVisualElement.styleSheets.Add(sheet);
                return;
            }
        }
    }
}
