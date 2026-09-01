using System;
using System.Collections.Generic;
using System.Linq;
using Base.ToolPackage.Editor.AudioRules.Model;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AudioRules.Window
{
    /// <summary>
    /// The results table. Columns are resizable, reorderable and sortable because an audio
    /// designer works biggest win first, and the sort is what turns a flat list of a few thousand
    /// clips into a work queue. Every row carries a play button, since nobody should approve a
    /// change to a clip they have not heard.
    /// </summary>
    internal sealed class AudioClipTableView : VisualElement
    {
        private const string ColumnBuild = "build";
        private const string ColumnChannels = "channels";
        private const string ColumnCurrent = "current";
        private const string ColumnFindings = "findings";
        private const string ColumnLength = "length";
        private const string ColumnName = "name";
        private const string ColumnRate = "rate";
        private const string ColumnRule = "rule";
        private const string ColumnRuntime = "runtime";
        private const string ColumnTarget = "target";
        private const string PlayGlyph = "\u25b6";
        private const int SortStepsPerColumn = 2;
        private const float RowHeight = 21f;
        private const string TargetArrow = "\u2192 ";

        /// <summary>Raised when the selection changes, with the row that is now current.</summary>
        internal event Action<AudioClipPlan> SelectionChanged;

        /// <summary>Raised whenever the number of selected rows changed.</summary>
        internal event Action SelectionCountChanged;

        /// <summary>The rows the user has selected.</summary>
        internal IReadOnlyList<AudioClipPlan> Selection => _selection;

        private readonly MultiColumnListView _list = new();
        private readonly List<AudioClipPlan> _items = new();
        private readonly List<AudioClipPlan> _scanOrder = new();
        private readonly List<AudioClipPlan> _selection = new();

        private string _sortColumn;
        private int _sortStep;
        private bool _isResettingSort;

        /// <summary>Builds the table and its columns.</summary>
        public AudioClipTableView()
        {
            style.flexGrow = 1f;

            _list.itemsSource = _items;
            _list.fixedItemHeight = RowHeight;
            _list.selectionType = SelectionType.Multiple;
            _list.sortingMode = ColumnSortingMode.Custom;
            _list.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            _list.style.flexGrow = 1f;

            BuildColumns();

            _list.columnSortingChanged += OnSortingChanged;
            _list.selectionChanged += OnSelectionChanged;

            Add(_list);
        }

        /// <summary>Replaces the rows and keeps the current sort.</summary>
        /// <param name="plans">The plans to show.</param>
        internal void SetItems(IEnumerable<AudioClipPlan> plans)
        {
            _items.Clear();
            _items.AddRange(plans);

            _scanOrder.Clear();
            _scanOrder.AddRange(plans);

            _selection.Clear();

            ApplySorting();
        }

        /// <summary>Redraws the visible rows without touching the data.</summary>
        internal void Refresh() => _list.RefreshItems();

        private static Label MakeLabel()
        {
            Label label = new();

            label.AddToClassList(AudioRulesStyle.CellClass);

            return label;
        }

        private static VisualElement MakePills()
        {
            VisualElement row = new();

            row.AddToClassList(AudioRulesStyle.PillsClass);

            return row;
        }

        private static string Short(EAudioFinding finding) => finding switch
        {
            EAudioFinding.Clipping => "Clipping",
            EAudioFinding.DcOffset => "DC offset",
            EAudioFinding.FakeStereo => "Fake stereo",
            EAudioFinding.LeadingSilence => "Head silence",
            EAudioFinding.LowPeak => "Quiet",
            _ => "Tail silence"
        };

        private static int CompareText(string first, string second)
            => string.Compare(first, second, StringComparison.OrdinalIgnoreCase);

        private static void Tint(Label label, long delta)
        {
            label.EnableInClassList(AudioRulesStyle.CellGoodClass, delta > 0L);
            label.EnableInClassList(AudioRulesStyle.CellBadClass, delta < 0L);
            label.EnableInClassList(AudioRulesStyle.CellDimClass, delta == 0L);
        }

        private void BuildColumns()
        {
            AddColumn(ColumnName, "Clip", 252f, 150f, true, MakeClipCell, BindName);
            AddColumn(ColumnLength, "Length", 64f, 52f, false, MakeNumber, BindLength);
            AddColumn(ColumnChannels, "Ch", 34f, 30f, false, MakeNumber, BindChannels);
            AddColumn(ColumnRate, "Rate", 70f, 58f, false, MakeNumber, BindRate);
            AddColumn(ColumnCurrent, "Current", 140f, 90f, false, MakeLabel, BindCurrent);
            AddColumn(ColumnTarget, "Target", 150f, 90f, false, MakeLabel, BindTarget);
            AddColumn(ColumnBuild, "Build", 78f, 60f, false, MakeNumber, BindBuild);
            AddColumn(ColumnRuntime, "Memory", 78f, 60f, false, MakeNumber, BindRuntime);
            AddColumn(ColumnRule, "Decided by", 130f, 80f, false, MakeLabel, BindRule);
            AddColumn(ColumnFindings, "Findings", 190f, 90f, true, MakePills, BindFindings);
        }

        private Label MakeNumber()
        {
            Label label = MakeLabel();

            label.AddToClassList(AudioRulesStyle.CellNumberClass);

            return label;
        }

        private void AddColumn(string name, string title, float width, float minWidth, bool stretchable,
            Func<VisualElement> make, Action<VisualElement, int> bind)
            => _list.columns.Add(new Column
            {
                name = name,
                title = title,
                width = width,
                minWidth = minWidth,
                stretchable = stretchable,
                sortable = true,
                resizable = true,
                makeCell = make,
                bindCell = bind
            });

        private VisualElement MakeClipCell()
        {
            VisualElement cell = new();

            cell.AddToClassList(AudioRulesStyle.ClipCellClass);

            Button button = new()
            {
                text = PlayGlyph,
                tooltip = "Play this clip."
            };

            button.AddToClassList(AudioRulesStyle.PlayClass);
            button.clicked += () => Play(button);

            Label label = new();

            label.AddToClassList(AudioRulesStyle.CellClass);
            label.AddToClassList(AudioRulesStyle.ClipNameClass);

            cell.Add(button);
            cell.Add(label);

            return cell;
        }

        private void Play(Button button)
        {
            if (button.userData is AudioClipPlan plan)
                AudioPreviewPlayer.Play(plan.Info.AssetPath);
        }

        private void BindName(VisualElement element, int index)
        {
            AudioClipPlan plan = _items[index];
            Label label = element.Q<Label>(className: AudioRulesStyle.ClipNameClass);

            element.Q<Button>().userData = plan;

            label.text = plan.Info.Name;
            label.tooltip = plan.Info.AssetPath;
        }

        private void BindLength(VisualElement element, int index)
            => ((Label)element).text = AudioRulesFormat.Seconds(_items[index].Info.LengthSeconds);

        private void BindChannels(VisualElement element, int index)
            => ((Label)element).text = _items[index].Info.Channels.ToString();

        private void BindRate(VisualElement element, int index)
            => ((Label)element).text = AudioRulesFormat.Kilohertz(_items[index].Info.SampleRate);

        private void BindCurrent(VisualElement element, int index)
        {
            Label label = (Label)element;

            label.text = AudioRulesFormat.Summary(_items[index].Info.Current);
            label.EnableInClassList(AudioRulesStyle.CellDimClass, !_items[index].HasChanges);
        }

        private void BindTarget(VisualElement element, int index)
        {
            AudioClipPlan plan = _items[index];
            Label label = (Label)element;

            label.text = plan.HasChanges
                ? TargetArrow + AudioRulesFormat.Summary(plan.Target)
                : string.Empty;

            label.EnableInClassList(AudioRulesStyle.CellTargetClass, plan.HasChanges);
            label.tooltip = plan.HasChanges
                ? string.Join(", ", plan.Changes.Select(change => change.ToString()))
                : string.Empty;
        }

        private void BindBuild(VisualElement element, int index)
        {
            Label label = (Label)element;

            label.text = AudioRulesFormat.Delta(_items[index].BuildDelta);
            Tint(label, _items[index].BuildDelta);
        }

        private void BindRuntime(VisualElement element, int index)
        {
            Label label = (Label)element;

            label.text = AudioRulesFormat.Delta(_items[index].RuntimeDelta);
            Tint(label, _items[index].RuntimeDelta);
        }

        private void BindRule(VisualElement element, int index)
        {
            AudioClipPlan plan = _items[index];
            Label label = (Label)element;

            label.text = plan.PrimaryRule;
            label.tooltip = string.Join(" -> ", plan.MatchedRules);
            label.AddToClassList(AudioRulesStyle.CellDimClass);
        }

        private void BindFindings(VisualElement element, int index)
        {
            element.Clear();

            foreach (EAudioFinding finding in _items[index].Findings)
            {
                Label pill = new(Short(finding));

                pill.AddToClassList(AudioRulesStyle.PillClass);
                element.Add(pill);
            }
        }

        private void OnSelectionChanged(IEnumerable<object> selected)
        {
            _selection.Clear();

            foreach (object item in selected)
            {
                if (item is AudioClipPlan plan)
                    _selection.Add(plan);
            }

            SelectionChanged?.Invoke(_selection.Count > 0
                ? _selection[0]
                : null);

            SelectionCountChanged?.Invoke();
        }

        // Unity cycles a column between ascending and descending forever. A third press should
        // mean "never mind", so the sort is dropped and the scan order comes back.
        private void OnSortingChanged()
        {
            if (_isResettingSort)
                return;

            List<SortColumnDescription> sorted = _list.sortedColumns.ToList();
            string column = sorted.Count > 0
                ? sorted[0].columnName
                : null;

            if (column != _sortColumn)
            {
                _sortColumn = column;
                _sortStep = 1;
            }
            else
            {
                _sortStep++;
            }

            if (_sortStep > SortStepsPerColumn)
            {
                ResetSorting();
                return;
            }

            ApplySorting();
        }

        private void ResetSorting()
        {
            _isResettingSort = true;

            _list.sortColumnDescriptions.Clear();

            _sortColumn = null;
            _sortStep = 0;

            RestoreScanOrder();

            _isResettingSort = false;
        }

        private void RestoreScanOrder()
        {
            _items.Clear();
            _items.AddRange(_scanOrder);

            _list.RefreshItems();
        }

        private void ApplySorting()
        {
            List<SortColumnDescription> sorted = _list.sortedColumns.ToList();

            if (sorted.Count == 0)
            {
                RestoreScanOrder();
                return;
            }

            _items.Sort((first, second) => Compare(first, second, sorted));
            _list.RefreshItems();
        }

        private int Compare(AudioClipPlan first, AudioClipPlan second, List<SortColumnDescription> sorted)
        {
            foreach (SortColumnDescription description in sorted)
            {
                int result = CompareColumn(first, second, description.columnName);

                if (result == 0)
                    continue;

                return description.direction == SortDirection.Ascending
                    ? result
                    : -result;
            }

            return 0;
        }

        private int CompareColumn(AudioClipPlan first, AudioClipPlan second, string column) => column switch
        {
            ColumnBuild => first.BuildDelta.CompareTo(second.BuildDelta),
            ColumnChannels => first.Info.Channels.CompareTo(second.Info.Channels),
            ColumnCurrent => CompareText(AudioRulesFormat.Summary(first.Info.Current),
                AudioRulesFormat.Summary(second.Info.Current)),
            ColumnFindings => first.Findings.Count.CompareTo(second.Findings.Count),
            ColumnLength => first.Info.LengthSeconds.CompareTo(second.Info.LengthSeconds),
            ColumnRate => first.Info.SampleRate.CompareTo(second.Info.SampleRate),
            ColumnRule => CompareText(first.PrimaryRule, second.PrimaryRule),
            ColumnRuntime => first.RuntimeDelta.CompareTo(second.RuntimeDelta),
            ColumnTarget => first.Changes.Count.CompareTo(second.Changes.Count),
            _ => CompareText(first.Info.Name, second.Info.Name)
        };
    }
}