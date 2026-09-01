using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Editing;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// The findings as a plain list, worst first, with nothing else on screen. The graph is for
    /// understanding how the code is wired; this is for working through what it found, which is a
    /// different job and wants a different window.
    /// <br/><br/>
    /// It shares the scan and the dismissals with the graph window, so a decision made in either shows
    /// up in the other. Two lists that disagree about what is left to do would be worse than one.
    /// </summary>
    internal sealed class CodebaseGraphIssuesPane : VisualElement
    {
        private const string ClearText = "Nothing was found. Either the code is clean or everything "
            + "found has been dismissed.";

        private const string ClearTitle = "All clear";
        private const string DismissLabel = "Dismiss";
        private const string DismissTooltip = "Hides this one finding. Nothing about the code changes.";
        private const string EmptyScanText = "Press Scan to read the project. It takes a few seconds.";
        private const string EmptyScanTitle = "No scan yet";
        private const int FilterMinimumWidth = 170;
        private const string HeadingFormat = "{0} findings: {1} high, {2} medium, {3} low";
        private const string NeutralMark = "\u2013";

        private const string NoMatchText = "Everything found is filtered out. Clear the filter or the "
            + "search to see it.";

        private const string NoMatchTitle = "Nothing matches";
        private const string OpenLabel = "Open";
        private const string OpenTooltip = "Opens the script at the declaration.";
        private const int RowHeight = 52;
        private const int SearchMinimumWidth = 160;
        private const string SearchTooltip = "Filters by name, file or finding.";
        private const string SuccessMark = "\u2713";

        /// <summary>How many findings are showing after the current filter.</summary>
        internal int VisibleCount => _entries.Count;

        private readonly List<FindingEntry> _all = new();
        private readonly List<FindingEntry> _entries = new();
        private readonly ListView _list;
        private readonly Label _heading;

        private CodebaseGraphData _graph;
        private ToolbarSearchField _searchField;
        private VisualElement _emptyState;
        private Label _emptyMark;
        private Label _emptyTitle;
        private Label _emptyText;
        private PopupField<string> _findingField;
        private string _search = string.Empty;

        /// <summary>Builds the pane, its filter row and its list.</summary>
        public CodebaseGraphIssuesPane()
        {
            AddToClassList(CodebaseGraphStyle.PaneClass);
            Add(BuildToolbar());

            _heading = GraphLabel.Build(string.Empty, CodebaseGraphStyle.IssueHeadingClass);
            Add(_heading);
            Add(BuildEmptyState());

            _list = new ListView
            {
                fixedItemHeight = RowHeight,
                makeItem = MakeRow,
                bindItem = BindRow
            };

            _list.AddToClassList(CodebaseGraphStyle.IssueListClass);
            _list.style.flexGrow = 1f;
            Add(_list);

            DismissalStore.Changed += Recollect;
            RegisterCallback<DetachFromPanelEvent>(_ => DismissalStore.Changed -= Recollect);
        }

        /// <summary>Points the pane at a freshly built graph, or at nothing when the scan was cleared.</summary>
        /// <param name="graph">Graph to read, or null.</param>
        internal void SetGraph(CodebaseGraphData graph)
        {
            _graph = graph;
            Recollect();
        }

        private static VisualElement MakeRow()
        {
            VisualElement row = new();
            row.AddToClassList(CodebaseGraphStyle.IssueRowClass);

            VisualElement text = new()
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            text.Add(GraphLabel.Build(string.Empty, CodebaseGraphStyle.IssueTitleClass));
            text.Add(GraphLabel.Build(string.Empty, CodebaseGraphStyle.IssueDetailClass));

            row.Add(GraphLabel.Build(string.Empty, CodebaseGraphStyle.IssueSeverityClass));
            row.Add(text);
            Button open = new()
            {
                text = OpenLabel,
                tooltip = OpenTooltip
            };

            Button dismiss = new()
            {
                text = DismissLabel,
                tooltip = DismissTooltip
            };

            dismiss.AddToClassList(CodebaseGraphStyle.DismissButtonClass);

            row.Add(open);
            row.Add(dismiss);

            return row;
        }

        private static int Count(List<FindingEntry> entries, ESeverity severity)
        {
            int count = 0;

            foreach (FindingEntry entry in entries)
            {
                if (entry.Severity == severity)
                    count++;
            }

            return count;
        }

        private static void BindButton(Button button, Action action) => button.clickable = new Clickable(action);

        private static string ReadName(FindingEntry entry)
        {
            if (entry.Member != null && entry.Type != null)
                return $"{entry.Type.ShortName}.{entry.Member.Name}";

            return entry.Type == null
                ? entry.Id
                : entry.Type.FullName;
        }

        private VisualElement BuildEmptyState()
        {
            _emptyState = new VisualElement();
            _emptyState.AddToClassList(CodebaseGraphStyle.EmptyStateClass);

            _emptyMark = GraphLabel.Build(SuccessMark, CodebaseGraphStyle.EmptyMarkClass);
            _emptyTitle = GraphLabel.Build(string.Empty, CodebaseGraphStyle.EmptyTitleClass);
            _emptyText = GraphLabel.Build(string.Empty, CodebaseGraphStyle.PanePlaceholderClass);

            _emptyState.Add(_emptyMark);
            _emptyState.Add(_emptyTitle);
            _emptyState.Add(_emptyText);

            return _emptyState;
        }

        private VisualElement BuildToolbar()
        {
            Toolbar toolbar = new();
            toolbar.AddToClassList(CodebaseGraphStyle.TopBarClass);

            _findingField = new PopupField<string>(FindingCatalog.BuildChoices(), 0)
            {
                style =
                {
                    minWidth = FilterMinimumWidth
                }
            };

            _findingField.RegisterValueChangedCallback(_ => Refresh());
            toolbar.Add(_findingField);

            _searchField = new ToolbarSearchField
            {
                tooltip = SearchTooltip,
                style =
                {
                    flexGrow = 1f,
                    minWidth = SearchMinimumWidth
                }
            };

            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            toolbar.Add(_searchField);

            return toolbar;
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _search = evt.newValue ?? string.Empty;
            Refresh();
        }

        /// <summary>
        /// Rereads the graph. Only worth doing when the scan or the dismissals changed, since walking
        /// every member of every type to gather findings is far more than a filter change is worth.
        /// </summary>
        private void Recollect()
        {
            _all.Clear();

            if (_graph != null)
                _all.AddRange(FindingCollector.Collect(_graph));

            Refresh();
        }

        private void Refresh()
        {
            _entries.Clear();

            if (_graph == null)
            {
                ShowEmptyState(EmptyScanTitle, EmptyScanText, false);
                return;
            }

            EFinding finding = FindingCatalog.GetAt(_findingField.index);

            foreach (FindingEntry entry in _all)
            {
                if (IsMatch(entry, finding))
                    _entries.Add(entry);
            }

            if (_entries.Count == 0)
            {
                ShowEmptyState(_all.Count == 0
                        ? ClearTitle
                        : NoMatchTitle,
                    _all.Count == 0
                        ? ClearText
                        : NoMatchText,
                    _all.Count == 0);

                return;
            }

            _emptyState.style.display = DisplayStyle.None;
            _list.style.display = DisplayStyle.Flex;

            _heading.text = string.Format(HeadingFormat,
                _entries.Count,
                Count(_entries, ESeverity.High),
                Count(_entries, ESeverity.Medium),
                Count(_entries, ESeverity.Low));

            _list.itemsSource = _entries;
            _list.Rebuild();
        }

        /// <summary>
        /// Says that there is nothing to do, and says it as good news when it is. A list that empties
        /// out to a blank panel reads as broken rather than as finished.
        /// </summary>
        private void ShowEmptyState(string title, string text, bool isSuccess)
        {
            _heading.text = string.Empty;
            _entries.Clear();

            _list.itemsSource = _entries;
            _list.Rebuild();
            _list.style.display = DisplayStyle.None;

            _emptyMark.text = isSuccess
                ? SuccessMark
                : NeutralMark;

            _emptyMark.EnableInClassList(CodebaseGraphStyle.IsSuccessClass, isSuccess);
            _emptyTitle.text = title;
            _emptyText.text = text;
            _emptyState.style.display = DisplayStyle.Flex;
        }

        private bool IsMatch(FindingEntry entry, EFinding finding)
        {
            if (finding != EFinding.None && finding != EFinding.Any && entry.Finding != finding)
                return false;

            if (string.IsNullOrEmpty(_search))
                return true;

            return entry.Id.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0
                || entry.Location.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void BindRow(VisualElement row, int index)
        {
            FindingEntry entry = _entries[index];
            FindingDescriptor descriptor = FindingCatalog.Describe(entry.Finding);

            List<Label> labels = row.Query<Label>().ToList();
            labels[0].text = entry.Severity.ToString();
            labels[0].EnableInClassList(CodebaseGraphStyle.IsHighClass, entry.Severity == ESeverity.High);
            labels[0].EnableInClassList(CodebaseGraphStyle.IsMediumClass, entry.Severity == ESeverity.Medium);

            // Striping and the left accent both come from the row, so a long list stays readable
            // without anyone having to follow a line across it.
            row.EnableInClassList(CodebaseGraphStyle.IsOddClass, index % 2 == 1);
            row.EnableInClassList(CodebaseGraphStyle.IsHighClass, entry.Severity == ESeverity.High);
            row.EnableInClassList(CodebaseGraphStyle.IsMediumClass, entry.Severity == ESeverity.Medium);
            labels[1].text = $"{descriptor.Title}: {ReadName(entry)}";
            labels[2].text = entry.Location;

            List<Button> buttons = row.Query<Button>().ToList();
            BindButton(buttons[0], action: () => MemberSourceEditor.OpenAtMember(entry.Type, entry.Member));
            BindButton(buttons[1], action: () => Dismiss(entry));

            buttons[0].SetEnabled(entry.Type != null);
        }

        private void Dismiss(FindingEntry entry)
        {
            DismissalStore.Dismiss(entry.Id, false);
            Recollect();
        }
    }
}