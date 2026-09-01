using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Every control that changes what the view shows. They were spread through the window alongside
    /// navigation, selection and export, and a class that owns four unrelated jobs grows without anyone
    /// deciding to grow it. Here the rule is simple: if it writes to the filter, it lives here.
    /// <br/><br/>
    /// Thirteen controls in a row did not fit on a laptop, so the ones that answer the same question are
    /// gathered behind two menus. View holds everything about how the graph is drawn, Report holds
    /// everything that reads or writes a file. What is left on the bar is what changes minute to minute.
    /// <br/><br/>
    /// The toolbar owns the widgets and the filter values behind them. It never rebuilds anything
    /// itself, it says that something changed and lets the window decide what that costs.
    /// </summary>
    internal sealed class CodebaseGraphToolbar : Toolbar
    {
        private const string AllAssembliesLabel = "All assemblies";
        private const string AssemblyEdgeLabel = "Assembly edge report";
        private const string BackLabel = "Back";
        private const string BackTooltip = "Goes up one level.";
        private const string EdgeAllLabel = "Lines: All";
        private const string EdgeMutedLabel = "Lines: Muted";
        private const string EdgeNoneLabel = "Lines: None";
        private const string EdgeSelectedLabel = "Lines: Selected";
        private const string ExportLabel = "Export findings";
        private const string ExportScopeLabel = "Export scope";
        private const string ImportLabel = "Update dismissals";

        private const string LayoutDependenciesLabel = "Layout: Dependencies";
        private const string LayoutGroupedLabel = "Layout: Grouped by name";
        private const string NeighborFormat = "Neighbors: {0}";
        private const int NeighborMaximum = 3;

        private const string NeighborTooltip = "How many steps out from the focused entry the view "
            + "reaches. One shows what it touches directly, three follows those connections two steps "
            + "further.";

        private const string RefreshLabel = "Rescan";
        private const string RefreshTooltip = "Scans the project again.";
        private const string ReportLabel = "Report";
        private const string ReportTooltip = "Writing a report out, and reading dismissals back in.";
        private const string SearchCurrentLevelLabel = "Find: This level";
        private const string SearchEverywhereLabel = "Find: Everything";
        private const string SearchMembersLabel = "Find: Members";

        private const string SearchPlaceholder = "Search everything: namespaces, types and members, "
            + "wherever they live";

        private const string SearchScopeTooltip = "This level filters what is on screen. The others "
            + "search the whole project.";

        private const string SearchTypesLabel = "Find: Types";
        private const string ShowDataLabel = "Fields and consts";
        private const string ShowMembersLabel = "Members on type nodes";
        private const string ShowPrivateLabel = "Private members";
        private const string ViewLabel = "View";

        private const string ViewTooltip = "How the graph is drawn: layout, lines and how much detail "
            + "each node carries.";

        private readonly GraphFilter _filter;
        private readonly CodebaseGraphToolbarActions _actions;

        private PopupField<string> _assemblyField;
        private PopupField<string> _findingField;
        private PopupField<string> _searchScopeField;
        private PopupField<string> _neighborField;
        private ToolbarSearchField _searchField;

        /// <summary>Builds the toolbar and wires every control to the filter behind it.</summary>
        /// <param name="filter">Filter the controls write to.</param>
        /// <param name="actions">What the toolbar can ask the window to do.</param>
        public CodebaseGraphToolbar(GraphFilter filter, CodebaseGraphToolbarActions actions)
        {
            _filter = filter;
            _actions = actions;

            AddToClassList(CodebaseGraphStyle.TopBarClass);
            Build();
        }

        /// <summary>The largest number of steps out the neighbor control offers.</summary>
        /// <returns>The maximum depth.</returns>
        internal static int ReadNeighborMaximum() => NeighborMaximum;

        /// <summary>Replaces the assembly choices after a scan and selects them all.</summary>
        /// <param name="assemblies">Assemblies the scan covered.</param>
        internal void SetAssemblies(IEnumerable<string> assemblies)
        {
            List<string> choices = new()
            {
                AllAssembliesLabel
            };

            choices.AddRange(assemblies);

            _assemblyField.choices = choices;
            _assemblyField.SetValueWithoutNotify(AllAssembliesLabel);
            _filter.AssemblyName = null;
        }

        /// <summary>True when an assembly is among the current choices.</summary>
        /// <param name="assembly">Assembly name to look for.</param>
        /// <returns>True when it can be selected.</returns>
        internal bool HasAssembly(string assembly) => _assemblyField.choices.Contains(assembly);

        /// <summary>
        /// Pushes the filter values into the controls, after restoring them from disk. The two menus
        /// need nothing here: they read the filter every time they open, so they cannot fall out of step
        /// with it.
        /// </summary>
        internal void Sync()
        {
            _findingField.index = FindingCatalog.GetIndex(_filter.Finding);
            _searchScopeField.index = (int)_filter.SearchScope;

            _neighborField.SetValueWithoutNotify(string.Format(NeighborFormat, _filter.Hops));
            _searchField.SetValueWithoutNotify(_filter.Search);
            _assemblyField.SetValueWithoutNotify(_filter.AssemblyName ?? AllAssembliesLabel);
        }

        /// <summary>Clears the search box without raising a change.</summary>
        internal void ClearSearch()
        {
            _filter.Search = string.Empty;
            _searchField.SetValueWithoutNotify(string.Empty);
        }

        /// <summary>
        /// Builds the neighbor depth control. It belongs to the filter but is shown beside the focus
        /// notice rather than in the toolbar, because it only means anything while something is focused.
        /// </summary>
        /// <returns>The control, for the caller to place.</returns>
        internal VisualElement CreateNeighborField()
        {
            _neighborField = new PopupField<string>(BuildNeighborChoices(), 0)
            {
                tooltip = NeighborTooltip
            };

            _neighborField.RegisterValueChangedCallback(OnNeighborChanged);

            return _neighborField;
        }

        private static List<string> BuildNeighborChoices()
        {
            List<string> choices = new();

            for (int hops = 1; hops <= NeighborMaximum; hops++)
                choices.Add(string.Format(NeighborFormat, hops));

            return choices;
        }

        private static List<string> BuildSearchScopeChoices() => new()
        {
            SearchEverywhereLabel,
            SearchCurrentLevelLabel,
            SearchTypesLabel,
            SearchMembersLabel
        };

        private static DropdownMenuAction.Status ReadStatus(bool isOn) => isOn
            ? DropdownMenuAction.Status.Checked
            : DropdownMenuAction.Status.Normal;

        private void Build()
        {
            Add(new ToolbarButton(() => _actions.Back?.Invoke())
            {
                text = BackLabel,
                tooltip = BackTooltip
            });

            Add(new ToolbarButton(() => _actions.Rescan?.Invoke())
            {
                text = RefreshLabel,
                tooltip = RefreshTooltip
            });

            AddFilters();
            AddActions();
        }

        private void AddFilters()
        {
            _assemblyField = new PopupField<string>(new List<string>
            {
                AllAssembliesLabel
            }, 0);

            _assemblyField.RegisterValueChangedCallback(OnAssemblyChanged);
            Add(_assemblyField);

            _findingField = new PopupField<string>(FindingCatalog.BuildChoices(), 0);
            _findingField.RegisterValueChangedCallback(OnFindingChanged);
            Add(_findingField);

            Add(BuildViewMenu());

            _searchScopeField = new PopupField<string>(BuildSearchScopeChoices(), 0)
            {
                tooltip = SearchScopeTooltip
            };

            _searchScopeField.RegisterValueChangedCallback(OnSearchScopeChanged);
            Add(_searchScopeField);

            _searchField = new ToolbarSearchField
            {
                tooltip = SearchPlaceholder,
                style =
                {
                    flexGrow = 1f
                }
            };

            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            Add(_searchField);
        }

        private void AddActions() => Add(BuildReportMenu());

        /// <summary>
        /// Gathers everything about how the graph is drawn. Layout and lines are one choice each, the
        /// three detail switches are independent, and a separator between them says which is which
        /// without a word of explanation.
        /// </summary>
        private ToolbarMenu BuildViewMenu()
        {
            ToolbarMenu menu = new()
            {
                text = ViewLabel,
                tooltip = ViewTooltip
            };

            AppendLayout(menu, LayoutDependenciesLabel, ELayoutMode.Dependencies);
            AppendLayout(menu, LayoutGroupedLabel, ELayoutMode.Grouped);
            menu.menu.AppendSeparator();

            AppendEdge(menu, EdgeMutedLabel, EEdgeMode.Muted);
            AppendEdge(menu, EdgeAllLabel, EEdgeMode.All);
            AppendEdge(menu, EdgeSelectedLabel, EEdgeMode.SelectedOnly);
            AppendEdge(menu, EdgeNoneLabel, EEdgeMode.None);
            menu.menu.AppendSeparator();

            menu.menu.AppendAction(ShowPrivateLabel,
                action: _ => ToggleShowPrivate(),
                actionStatusCallback: _ => ReadStatus(_filter.ShowPrivate));

            menu.menu.AppendAction(ShowDataLabel,
                action: _ => ToggleShowData(),
                actionStatusCallback: _ => ReadStatus(_filter.ShowDataMembers));

            menu.menu.AppendAction(ShowMembersLabel,
                action: _ => ToggleShowMembers(),
                actionStatusCallback: _ => ReadStatus(_filter.ShowMembersOnTypes));

            return menu;
        }

        private ToolbarMenu BuildReportMenu()
        {
            ToolbarMenu menu = new()
            {
                text = ReportLabel,
                tooltip = ReportTooltip
            };

            menu.menu.AppendAction(ExportLabel, action: _ => _actions.Export?.Invoke());
            menu.menu.AppendAction(ExportScopeLabel, action: _ => _actions.ExportScope?.Invoke());
            menu.menu.AppendAction(AssemblyEdgeLabel, action: _ => _actions.AssemblyEdgeReport?.Invoke());
            menu.menu.AppendSeparator();
            menu.menu.AppendAction(ImportLabel, action: _ => _actions.Import?.Invoke());

            return menu;
        }

        private void AppendLayout(ToolbarMenu menu, string label, ELayoutMode mode) => menu.menu.AppendAction(label,
            action: _ => SetLayout(mode),
            actionStatusCallback: _ => ReadStatus(_filter.LayoutMode == mode));

        private void AppendEdge(ToolbarMenu menu, string label, EEdgeMode mode) => menu.menu.AppendAction(label,
            action: _ => SetEdge(mode),
            actionStatusCallback: _ => ReadStatus(_filter.EdgeMode == mode));

        private void SetLayout(ELayoutMode mode)
        {
            _filter.LayoutMode = mode;
            _actions.FilterChanged?.Invoke();
        }

        private void SetEdge(EEdgeMode mode)
        {
            _filter.EdgeMode = mode;
            _actions.EdgeModeChanged?.Invoke();
        }

        private void ToggleShowPrivate()
        {
            _filter.ShowPrivate = !_filter.ShowPrivate;
            _actions.FilterChanged?.Invoke();
        }

        private void ToggleShowData()
        {
            _filter.ShowDataMembers = !_filter.ShowDataMembers;
            _actions.FilterChanged?.Invoke();
        }

        private void ToggleShowMembers()
        {
            _filter.ShowMembersOnTypes = !_filter.ShowMembersOnTypes;
            _actions.FilterChanged?.Invoke();
        }

        private void OnAssemblyChanged(ChangeEvent<string> evt)
        {
            _filter.AssemblyName = evt.newValue == AllAssembliesLabel
                ? null
                : evt.newValue;

            _actions.FilterChanged?.Invoke();
        }

        private void OnFindingChanged(ChangeEvent<string> evt)
        {
            _filter.Finding = FindingCatalog.GetAt(_findingField.index);
            _actions.FilterChanged?.Invoke();
        }

        private void OnSearchScopeChanged(ChangeEvent<string> evt)
        {
            _filter.SearchScope = (ESearchScope)_searchScopeField.index;
            _actions.FilterChanged?.Invoke();
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _filter.Search = evt.newValue ?? string.Empty;
            _actions.SearchChanged?.Invoke();
        }

        private void OnNeighborChanged(ChangeEvent<string> evt)
        {
            _filter.Hops = _neighborField.index + 1;
            _actions.NeighborChanged?.Invoke();
        }
    }
}