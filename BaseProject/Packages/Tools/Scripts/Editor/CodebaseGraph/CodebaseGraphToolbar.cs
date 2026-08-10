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
    /// The toolbar owns the widgets and the filter values behind them. It never rebuilds anything
    /// itself, it says that something changed and lets the window decide what that costs.
    /// </summary>
    internal sealed class CodebaseGraphToolbar : Toolbar
    {
        private const string AllAssembliesLabel = "All assemblies";
        private const string BackLabel = "Back";
        private const string BackTooltip = "Goes up one level.";
        private const int DataFlag = 2;
        private const string DismissedFormat = "Dismissed ({0})";
        private const string DismissedTooltip = "Opens the list of everything you dismissed.";
        private const string EdgeAllLabel = "Lines: All";
        private const string EdgeMutedLabel = "Lines: Muted";
        private const string EdgeNoneLabel = "Lines: None";
        private const string EdgeSelectedLabel = "Lines: Selected";

        private const string EdgeTooltip = "How many lines to draw. Muted keeps them faint until you "
            + "click something.";

        private const string ExportLabel = "Export findings";
        private const string ExportScopeLabel = "Export scope";

        private const string ExportScopeTooltip = "Writes one namespace or assembly to a file. Small "
            + "enough to hand to someone working on that part alone.";

        private const string ExportTooltip = "Writes the whole report to a file.";
        private const string ImportLabel = "Update dismissals";

        private const string ImportTooltip = "Reads a list of dismissals back in, from the clipboard or a "
            + "file.";

        private const string LayoutDependenciesLabel = "Layout: Dependencies";
        private const string LayoutGroupedLabel = "Layout: Grouped by name";

        private const string LayoutTooltip = "Dependencies to understand the code. Grouped to find "
            + "something by name.";

        private const int MembersFlag = 4;
        private const string NeighborFormat = "Neighbors: {0}";
        private const int NeighborMaximum = 3;

        private const string NeighborTooltip = "How many steps out from the focused entry the view "
            + "reaches. One shows what it touches directly, three follows those connections two steps "
            + "further.";

        private const string PopupTextClass = "unity-base-popup-field__text";
        private const int PrivateFlag = 1;
        private const string RefreshLabel = "Rescan";
        private const string RefreshTooltip = "Scans the project again.";
        private const string SearchCurrentLevelLabel = "Find: This level";
        private const string SearchEverywhereLabel = "Find: Everything";
        private const string SearchMembersLabel = "Find: Members";

        private const string SearchPlaceholder = "Search everything: namespaces, types and members, "
            + "wherever they live";

        private const string SearchScopeTooltip = "This level filters what is on screen. The others "
            + "search the whole project.";

        private const string SearchTypesLabel = "Find: Types";
        private const string ShowDataLabel = "Fields and consts";
        private const string ShowDataShort = "fields";
        private const string ShowMembersLabel = "Members on type nodes";
        private const string ShowMembersShort = "members";
        private const string ShowNoneLabel = "Show: nothing extra";
        private const string ShowPrefix = "Show: ";
        private const string ShowPrivateLabel = "Private members";
        private const string ShowPrivateShort = "private";
        private const string ShowSeparator = ", ";
        private const string ShowTooltip = "How much detail to show on each node.";
        private const string ToolbarRowClass = "top-bar";

        private readonly GraphFilter _filter;
        private readonly CodebaseGraphToolbarActions _actions;

        private PopupField<string> _assemblyField;
        private PopupField<string> _findingField;
        private PopupField<string> _layoutField;
        private PopupField<string> _edgeField;
        private PopupField<string> _searchScopeField;
        private PopupField<string> _neighborField;
        private MaskField _detailField;
        private ToolbarSearchField _searchField;
        private ToolbarButton _dismissedButton;

        /// <summary>Builds the toolbar and wires every control to the filter behind it.</summary>
        /// <param name="filter">Filter the controls write to.</param>
        /// <param name="actions">What the toolbar can ask the window to do.</param>
        public CodebaseGraphToolbar(GraphFilter filter, CodebaseGraphToolbarActions actions)
        {
            _filter = filter;
            _actions = actions;

            AddToClassList(ToolbarRowClass);
            Build();
        }

        /// <summary>The largest number of steps out the neighbor control offers.</summary>
        /// <returns>The maximum depth.</returns>
        public static int ReadNeighborMaximum() => NeighborMaximum;

        /// <summary>Replaces the assembly choices after a scan and selects them all.</summary>
        /// <param name="assemblies">Assemblies the scan covered.</param>
        public void SetAssemblies(IEnumerable<string> assemblies)
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
        public bool HasAssembly(string assembly) => _assemblyField.choices.Contains(assembly);

        /// <summary>Pushes the filter values into the controls, after restoring them from disk.</summary>
        public void Sync()
        {
            _findingField.index = FindingCatalog.GetIndex(_filter.Finding);
            _edgeField.index = (int)_filter.EdgeMode;
            _layoutField.index = (int)_filter.LayoutMode;
            _searchScopeField.index = (int)_filter.SearchScope;

            _detailField.SetValueWithoutNotify(ReadDetailMask());
            UpdateDetailText();

            _neighborField.SetValueWithoutNotify(string.Format(NeighborFormat, _filter.Hops));
            _searchField.SetValueWithoutNotify(_filter.Search);
            _assemblyField.SetValueWithoutNotify(_filter.AssemblyName ?? AllAssembliesLabel);
        }

        /// <summary>Shows how many entries are currently dismissed.</summary>
        /// <param name="count">Number of dismissals.</param>
        public void SetDismissedCount(int count) => _dismissedButton.text = string.Format(DismissedFormat, count);

        /// <summary>Clears the search box without raising a change.</summary>
        public void ClearSearch()
        {
            _filter.Search = string.Empty;
            _searchField.SetValueWithoutNotify(string.Empty);
        }

        /// <summary>
        /// Builds the neighbor depth control. It belongs to the filter but is shown beside the focus
        /// notice rather than in the toolbar, because it only means anything while something is focused.
        /// </summary>
        /// <returns>The control, for the caller to place.</returns>
        public VisualElement CreateNeighborField()
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

        private static List<string> BuildLayoutChoices() => new()
        {
            LayoutDependenciesLabel,
            LayoutGroupedLabel
        };

        private static List<string> BuildEdgeChoices() => new()
        {
            EdgeMutedLabel,
            EdgeAllLabel,
            EdgeSelectedLabel,
            EdgeNoneLabel
        };

        private static List<string> BuildSearchScopeChoices() => new()
        {
            SearchEverywhereLabel,
            SearchCurrentLevelLabel,
            SearchTypesLabel,
            SearchMembersLabel
        };

        private static List<string> BuildDetailChoices() => new()
        {
            ShowPrivateLabel,
            ShowDataLabel,
            ShowMembersLabel
        };

        private static VisualElement BuildSpacer()
        {
            VisualElement spacer = new();
            spacer.style.flexGrow = 1f;

            return spacer;
        }

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

            Add(BuildSpacer());
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

            _layoutField = new PopupField<string>(BuildLayoutChoices(), 0)
            {
                tooltip = LayoutTooltip
            };

            _layoutField.RegisterValueChangedCallback(OnLayoutModeChanged);
            Add(_layoutField);

            _edgeField = new PopupField<string>(BuildEdgeChoices(), 0)
            {
                tooltip = EdgeTooltip
            };

            _edgeField.RegisterValueChangedCallback(OnEdgeModeChanged);
            Add(_edgeField);

            _detailField = new MaskField(BuildDetailChoices(), ReadDetailMask())
            {
                tooltip = ShowTooltip
            };

            _detailField.RegisterValueChangedCallback(OnDetailChanged);
            UpdateDetailText();
            Add(_detailField);

            _searchScopeField = new PopupField<string>(BuildSearchScopeChoices(), 0)
            {
                tooltip = SearchScopeTooltip
            };

            _searchScopeField.RegisterValueChangedCallback(OnSearchScopeChanged);
            Add(_searchScopeField);

            _searchField = new ToolbarSearchField
            {
                tooltip = SearchPlaceholder
            };

            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            Add(_searchField);
        }

        private void AddActions()
        {
            _dismissedButton = new ToolbarButton(() => _actions.OpenDismissals?.Invoke())
            {
                text = string.Format(DismissedFormat, 0),
                tooltip = DismissedTooltip
            };

            Add(_dismissedButton);

            Add(new ToolbarButton(() => _actions.Export?.Invoke())
            {
                text = ExportLabel,
                tooltip = ExportTooltip
            });

            Add(new ToolbarButton(() => _actions.Import?.Invoke())
            {
                text = ImportLabel,
                tooltip = ImportTooltip
            });

            Add(new ToolbarButton(() => _actions.ExportScope?.Invoke())
            {
                text = ExportScopeLabel,
                tooltip = ExportScopeTooltip
            });
        }

        private int ReadDetailMask()
        {
            int mask = 0;

            if (_filter.ShowPrivate)
                mask |= PrivateFlag;

            if (_filter.ShowDataMembers)
                mask |= DataFlag;

            if (_filter.ShowMembersOnTypes)
                mask |= MembersFlag;

            return mask;
        }

        /// <summary>
        /// Writes what is actually switched on into the field. A mask field summarizes itself as
        /// Nothing, Mixed or Everything, none of which tells you which three things are meant.
        /// </summary>
        private void UpdateDetailText()
        {
            Label text = _detailField.Q<Label>(className: PopupTextClass);
            if (text == null)
                return;

            List<string> parts = new();

            if (_filter.ShowPrivate)
                parts.Add(ShowPrivateShort);

            if (_filter.ShowDataMembers)
                parts.Add(ShowDataShort);

            if (_filter.ShowMembersOnTypes)
                parts.Add(ShowMembersShort);

            text.text = parts.Count == 0
                ? ShowNoneLabel
                : $"{ShowPrefix}{string.Join(ShowSeparator, parts)}";
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

        private void OnLayoutModeChanged(ChangeEvent<string> evt)
        {
            _filter.LayoutMode = (ELayoutMode)_layoutField.index;
            _actions.FilterChanged?.Invoke();
        }

        private void OnEdgeModeChanged(ChangeEvent<string> evt)
        {
            _filter.EdgeMode = (EEdgeMode)_edgeField.index;
            _actions.EdgeModeChanged?.Invoke();
        }

        private void OnDetailChanged(ChangeEvent<int> evt)
        {
            _filter.ShowPrivate = (evt.newValue & PrivateFlag) != 0;
            _filter.ShowDataMembers = (evt.newValue & DataFlag) != 0;
            _filter.ShowMembersOnTypes = (evt.newValue & MembersFlag) != 0;

            UpdateDetailText();
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