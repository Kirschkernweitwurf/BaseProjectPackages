using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// The sortable list next to the graph. A graph stops being readable well before a namespace runs
    /// out of types, so the list is what you scan, and the graph shows the neighborhood of what you pick.
    /// </summary>
    internal sealed class CodebaseGraphListPane : VisualElement
    {
        private const string ActiveSortClass = "is-active";
        private const string BadgeClass = "row-badge";
        private const string ClearClass = "row-clear";
        private const string ClearText = "Clear";
        private const string DetailName = "row-detail";
        private const string DismissedClass = "row-dismissed";
        private const string DismissedText = "Dismissed";
        private const string HeadingClass = "pane-heading";
        private const string OnlyNewLabel = "New only";

        private const string OnlyNewTooltip = "Shows only what this scan found and the last one did not. "
            + "Needs two scans to mean anything.";

        private const string PaneClass = "pane";
        private const string RowClass = "list-row";
        private const int RowHeight = 42;
        private const string RowMetaClass = "list-row-meta";
        private const string RowTitleClass = "list-row-title";
        private const string SortByFanInLabel = "Used by";
        private const string SortByFanOutLabel = "Uses";
        private const string SortByFindingsLabel = "Findings";
        private const string SortByNameLabel = "Name";
        private const string SortRowClass = "sort-row";
        private const string TitleName = "row-title";

        private readonly Action<GraphEntry> _onSelected;
        private readonly Action<GraphEntry> _onActivated;
        private readonly Action<bool> _onOnlyNewChanged;
        private readonly Dictionary<ESortMode, Button> _sortButtons = new();
        private readonly ListView _listView;
        private readonly Label _headingLabel;

        private Button _onlyNewButton;
        private bool _isOnlyNew;

        private List<GraphEntry> _entries = new();
        private ESortMode _sortMode = ESortMode.Name;
        private bool _isRebuilding;

        /// <summary>Builds the list pane, its heading and its sort header.</summary>
        /// <param name="onSelected">Raised when a row is clicked.</param>
        /// <param name="onActivated">Raised when a row is double-clicked.</param>
        /// <param name="onOnlyNewChanged">Raised when the new only filter is switched.</param>
        public CodebaseGraphListPane(Action<GraphEntry> onSelected,
            Action<GraphEntry> onActivated,
            Action<bool> onOnlyNewChanged)
        {
            _onSelected = onSelected;
            _onActivated = onActivated;
            _onOnlyNewChanged = onOnlyNewChanged;

            AddToClassList(PaneClass);

            _headingLabel = new Label(string.Empty);
            _headingLabel.AddToClassList(HeadingClass);
            Add(_headingLabel);

            Add(BuildSortRow());

            _listView = new ListView
            {
                fixedItemHeight = RowHeight,
                selectionType = SelectionType.Single,
                makeItem = MakeRow,
                bindItem = BindRow,
                style =
                {
                    flexGrow = 1f
                }
            };

            _listView.selectionChanged += OnSelectionChanged;
            _listView.itemsChosen += OnItemsChosen;
            Add(_listView);

            HighlightSortButton();
        }

        /// <summary>Replaces the shown entries and reapplies the current sort.</summary>
        /// <param name="heading">Heading describing what is being listed.</param>
        /// <param name="entries">Entries to show.</param>
        public void SetEntries(string heading, List<GraphEntry> entries)
        {
            _headingLabel.text = heading;
            _entries = entries;
            ApplySort();
        }

        private static Label BuildRowLabel(string name, string styleClass)
        {
            Label label = new()
            {
                name = name
            };

            label.AddToClassList(styleClass);
            return label;
        }

        private static int CountFindings(GraphEntry entry) => entry.Findings.Count + entry.NestedFindingCount;

        private static VisualElement MakeRow()
        {
            VisualElement row = new();
            row.AddToClassList(RowClass);

            row.Add(BuildRowLabel(TitleName, RowTitleClass));
            row.Add(BuildRowLabel(DetailName, RowMetaClass));

            return row;
        }

        private VisualElement BuildSortRow()
        {
            VisualElement row = new();
            row.AddToClassList(SortRowClass);

            row.Add(BuildSortButton(SortByNameLabel, ESortMode.Name));
            row.Add(BuildSortButton(SortByFanInLabel, ESortMode.FanIn));
            row.Add(BuildSortButton(SortByFanOutLabel, ESortMode.FanOut));
            row.Add(BuildSortButton(SortByFindingsLabel, ESortMode.Findings));

            _onlyNewButton = new Button(ToggleOnlyNew)
            {
                text = OnlyNewLabel,
                tooltip = OnlyNewTooltip,
                style =
                {
                    flexGrow = 1f
                }
            };

            row.Add(_onlyNewButton);

            return row;
        }

        private void ToggleOnlyNew()
        {
            _isOnlyNew = !_isOnlyNew;
            _onlyNewButton.EnableInClassList(ActiveSortClass, _isOnlyNew);
            _onOnlyNewChanged?.Invoke(_isOnlyNew);
        }

        private Button BuildSortButton(string label, ESortMode mode)
        {
            Button button = new(() => SetSort(mode))
            {
                text = label,
                style =
                {
                    flexGrow = 1f
                }
            };

            _sortButtons[mode] = button;
            return button;
        }

        private void SetSort(ESortMode mode)
        {
            _sortMode = mode;
            HighlightSortButton();
            ApplySort();
        }

        private void HighlightSortButton()
        {
            foreach (KeyValuePair<ESortMode, Button> pair in _sortButtons)
                pair.Value.EnableInClassList(ActiveSortClass, pair.Key == _sortMode);
        }

        private void ApplySort()
        {
            _entries.Sort(Compare);
            _isRebuilding = true;

            try
            {
                _listView.itemsSource = _entries;
                _listView.Rebuild();
            }
            finally
            {
                _isRebuilding = false;
            }
        }

        private int Compare(GraphEntry left, GraphEntry right)
        {
            switch (_sortMode)
            {
                case ESortMode.FanIn:
                    return right.FanIn.CompareTo(left.FanIn);

                case ESortMode.FanOut:
                    return right.FanOut.CompareTo(left.FanOut);

                case ESortMode.Findings:
                    return CountFindings(right).CompareTo(CountFindings(left));

                default:
                    return string.Compare(left.Title, right.Title, StringComparison.OrdinalIgnoreCase);
            }
        }

        private void BindRow(VisualElement element, int index)
        {
            GraphEntry entry = _entries[index];

            Label title = element.Q<Label>(TitleName);
            Label detail = element.Q<Label>(DetailName);

            title.text = entry.Title;
            title.tooltip = entry.Subtitle;

            int findings = CountFindings(entry);
            bool isClear = findings == 0 && !entry.HasDismissals;

            string meta = $"Used by {entry.FanIn}   \u00b7   Uses {entry.FanOut}";

            if (findings > 0)
                meta = $"{meta}   \u00b7   {findings} findings";

            if (entry.HasDismissals)
                meta = $"{meta}   \u00b7   {DismissedText}";

            // Nothing found is worth saying out loud. A list where only problems are colored reads as
            // a list of problems, and the work that cleared the rest leaves no trace.
            if (isClear)
                meta = $"{meta}   \u00b7   {ClearText}";

            detail.text = meta;
            detail.EnableInClassList(BadgeClass, findings > 0);
            detail.EnableInClassList(ClearClass, isClear);
            detail.EnableInClassList(DismissedClass, findings == 0 && entry.HasDismissals);
        }

        private void OnSelectionChanged(IEnumerable<object> selection)
        {
            // Rebuilding the list clears and restores the selection, which must not loop back here.
            if (_isRebuilding)
                return;

            foreach (object item in selection)
            {
                if (item is GraphEntry entry)
                    _onSelected?.Invoke(entry);

                return;
            }
        }

        private void OnItemsChosen(IEnumerable<object> selection)
        {
            if (_isRebuilding)
                return;

            foreach (object item in selection)
            {
                if (item is GraphEntry entry)
                    _onActivated?.Invoke(entry);

                return;
            }
        }
    }
}