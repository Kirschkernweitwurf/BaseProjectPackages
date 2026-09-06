using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using Base.ToolsPackage.Editor.TodoOverview.Scanning;
using Base.ToolsPackage.Editor.TodoOverview.Settings;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.TodoOverview
{
    /// <summary>
    /// Lists every TODO, BUG, FIXME and whatever else the project marks its open work with. The items
    /// are searched, filtered by keyword, owner and date, grouped, and opened at the exact line with a
    /// double click.
    /// <para>
    /// Only the rows the scroll view actually shows are drawn, so a project with thousands of open
    /// items stays as responsive as one with ten.
    /// </para>
    /// </summary>
    internal sealed class TodoOverviewWindow : EditorWindow
    {
        private const string CountFormat = "{0} of {1}";
        private const string EmptyFiltered = "Nothing matches the current filter.";
        private const string EmptyProject = "No open items found.";
        private const string EmptyScanning = "Scanning the project...";
        private const float FilterStripHeight = 26f;
        private const string KeywordFormat = "{0}  {1}";
        private const string KeywordTooltip = "Show or hide every {0} item";
        private const string MenuPath = "Tools/Base Packages/Code/Health/Todo Overview";
        private const float MinimumHeight = 340f;
        private const float MinimumWidth = 760f;
        private const string MultiLineSuffix = "  ...";
        private const float PillPadding = 14f;
        private const string WindowTitle = "Todos";

        // None of these are created where they are declared. A window Unity restores after a
        // domain reload can reach its first GUI pass without any field initializer having run, and
        // then every one of them is null.
        private Dictionary<string, int> _counts;
        private HashSet<string> _collapsed;
        private List<TodoEntry> _entries;
        private List<TodoGroup> _groups;
        private List<TodoRow> _rows;
        private List<string> _owners;
        private TodoColumns _columns;
        private TodoFilter _filter;
        private TodoEntry _selected;
        private TodoPalette _palette;
        private Rect _searchRect;
        private Vector2 _scroll;

        // Read on every scan rather than created once, so changing what a date means on the settings
        // page reaches the list. Its default reading is the same one a fresh project starts on, which
        // is what a restored window draws with until the first scan replaces it.
        private TodoDateRules _dateRules;

        private float _listWidth;
        private float _viewHeight;
        private int _alertCount;
        private int _selectedRow = -1;
        private int _visibleCount;
        private bool _needsQuery;
        private bool _needsRows;
        private bool _needsScan;
        private bool _scanned;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            wantsMouseMove = true;

            EnsureInitialized();

            _needsScan = true;
        }

        private void OnGUI()
        {
            EnsureInitialized();

            // Before the style guard rather than after it, so a pass that gives up still leaves the
            // window in the theme's color instead of flashing the editor's own grey.
            DrawBackground();

            // Unity hands out no editor styles while a dropdown owns the GUI, so such a pass draws
            // nothing rather than throwing on every style it touches.
            if (!TodoStyles.EnsureBuilt())
                return;

            if (_needsScan)
                QueueScan();

            if (_needsQuery)
                RunQuery();

            if (_needsRows)
                RebuildRows();

            HandleFocusRelease();
            HandleKeyboard();

            _searchRect = TodoToolbar.Draw(_filter, _owners, onFilterChanged: () => _needsQuery = true,
                onRescan: () => _needsScan = true);

            DrawFilterStrip();

            if (_rows.Count > 0)
                DrawColumnTitles();

            DrawList();
            DrawDetail();

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            TodoOverviewWindow window = GetWindow<TodoOverviewWindow>(WindowTitle);
            window.minSize = new Vector2(MinimumWidth, MinimumHeight);
            window.Show();
        }

        private static float PillWidth(GUIContent content)
            => Mathf.Max(TodoStyles.ChipWidth, TodoStyles.Chip.CalcSize(content).x + PillPadding);

        private static GUIContent MessageContent(TodoEntry entry)
        {
            string tooltip = entry.Details.Length == 0
                ? entry.Message
                : entry.Message + Environment.NewLine + entry.Details;

            return entry.LineCount > 1
                ? new GUIContent(entry.Message + MultiLineSuffix, tooltip)
                : new GUIContent(entry.Message, tooltip);
        }

        // Called from the first GUI pass as well, because a restored window can get there without
        // OnEnable having run and with every field still null.
        private void EnsureInitialized()
        {
            _collapsed ??= new HashSet<string>(StringComparer.Ordinal);
            _columns ??= new TodoColumns();
            _counts ??= new Dictionary<string, int>();
            _entries ??= new List<TodoEntry>();
            _filter ??= new TodoFilter();
            _groups ??= new List<TodoGroup>();
            _owners ??= new List<string>();
            _rows ??= new List<TodoRow>();
            _palette ??= new TodoPalette(TodoSettings.instance.Tags);
        }

        // A modal progress bar cannot be opened in the middle of a layout pass without Unity losing
        // track of the layout groups, so the scan itself waits until the pass is over.
        private void QueueScan()
        {
            _needsScan = false;

            EditorApplication.delayCall += Rescan;
        }

        private void Rescan()
        {
            if (this == null)
                return;

            TodoSettings settings = TodoSettings.instance;

            _palette = new TodoPalette(settings.Tags);

            _entries.Clear();
            _entries.AddRange(TodoScanner.Scan(settings));

            _dateRules = new TodoDateRules(settings.DateMeaning, settings.AgingAfterDays,
                settings.StaleAfterDays);

            _counts = TodoQuery.CountKeywords(_entries);
            _owners = TodoQuery.CollectOwners(_entries);
            _alertCount = TodoQuery.CountAlerts(_entries, _dateRules);

            // The person a filter points at can be gone after a rescan, which would leave the list
            // empty with no visible reason.
            if (_filter.Owner != TodoFilter.AnyOwner
                && !_owners.Contains(_filter.Owner))
                _filter.Owner = TodoFilter.AnyOwner;

            _selected = null;
            _selectedRow = -1;
            _scanned = true;
            _needsQuery = true;

            Repaint();
        }

        private void RunQuery()
        {
            _groups = TodoQuery.Build(_entries, _filter, _palette, _dateRules);
            _needsQuery = false;

            RebuildRows();
        }

        // Headers and items are flattened into one list of equally tall rows, which is what makes it
        // cheap to work out which rows the scroll view currently shows.
        private void RebuildRows()
        {
            bool grouped = _filter.Grouping != ETodoGrouping.None;

            _rows.Clear();
            _needsRows = false;
            _visibleCount = 0;

            for (int i = 0; i < _groups.Count; i++)
            {
                TodoGroup group = _groups[i];
                _visibleCount += group.Entries.Count;

                if (grouped)
                    _rows.Add(TodoRow.Header(i));

                if (grouped && _collapsed.Contains(group.Label))
                    continue;

                for (int j = 0; j < group.Entries.Count; j++)
                    _rows.Add(new TodoRow(i, j));
            }

            SyncSelection();
        }

        private void SyncSelection()
        {
            _selectedRow = -1;

            if (_selected == null)
                return;

            for (int i = 0; i < _rows.Count; i++)
            {
                TodoRow row = _rows[i];

                if (row.IsHeader || _groups[row.Group].Entries[row.Entry] != _selected)
                    continue;

                _selectedRow = i;

                return;
            }

            _selected = null;
        }

        // Nothing under the toolbar had a fill of its own, so the rows, the striping and the space
        // below the last row were all sitting on whatever grey the editor paints a window. The stripe
        // is a three percent white wash, which is a tint of what is behind it rather than a color, so
        // it only reads as the theme once the theme is what it is washing over.
        private void DrawBackground()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(new Rect(0f, 0f, position.width, position.height), EditorPalette.Background);
        }

        // One pill per keyword with its count, plus the overdue pill. Clicking a pill takes that
        // keyword out of the list, which is the fastest way to look at one kind of item only.
        private void DrawFilterStrip()
        {
            Rect strip = GUILayoutUtility.GetRect(0f, FilterStripHeight, GUILayout.ExpandWidth(true));
            float x = strip.x + TodoStyles.RowInset;
            float y = strip.y + (strip.height - TodoStyles.ChipHeight) * 0.5f;

            foreach (TodoTag tag in _palette.Tags)
            {
                _counts.TryGetValue(tag.Keyword, out int count);

                GUIContent content = new(string.Format(KeywordFormat, tag.Keyword, count),
                    string.Format(KeywordTooltip, tag.Keyword));

                Rect pill = new(x, y, PillWidth(content), TodoStyles.ChipHeight);
                x = pill.xMax + TodoStyles.Gap;

                if (!TodoChrome.DrawFilterPill(pill, content, tag.Color, _filter.IsKeywordVisible(tag.Keyword)))
                    continue;

                _filter.ToggleKeyword(tag.Keyword);
                _needsQuery = true;
            }

            DrawAlertPill(x, y);
            DrawCount(strip);

            TodoChrome.DrawSeparator(new Rect(strip.x, strip.yMax - TodoStyles.SeparatorThickness, strip.width,
                TodoStyles.SeparatorThickness));
        }

        // Labelled from what the project's dates mean, because a pill reading Overdue over notes that
        // record when they were written would be red on every item in the project and mean nothing.
        private void DrawAlertPill(float x, float y)
        {
            if (_alertCount == 0)
                return;

            ETodoDateMeaning meaning = _dateRules.DefaultMeaning;

            GUIContent content = new(string.Format(KeywordFormat, TodoDateWords.Filter(meaning), _alertCount),
                TodoDateWords.FilterTooltip(meaning));

            Rect pill = new(x, y, PillWidth(content), TodoStyles.ChipHeight);

            if (!TodoChrome.DrawFilterPill(pill, content, TodoStyles.DateColor(ETodoDateState.Alert),
                    _filter.AlertsOnly))
                return;

            _filter.AlertsOnly = !_filter.AlertsOnly;
            _needsQuery = true;
        }

        private void DrawCount(Rect strip)
        {
            Rect count = new(strip.xMax - TodoStyles.RowInset - TodoStyles.LocationWidth, strip.y,
                TodoStyles.LocationWidth, strip.height);

            GUI.Label(count, string.Format(CountFormat, _visibleCount, _entries.Count), TodoStyles.Counter);
        }

        // The scroll view takes a slice off the right for its scrollbar, so the titles are laid out
        // against the width the rows themselves reported rather than the width of the whole window.
        private void DrawColumnTitles()
        {
            Rect row = GUILayoutUtility.GetRect(0f, TodoStyles.HeaderHeight, GUILayout.ExpandWidth(true));

            float width = _listWidth > 0f
                ? _listWidth
                : row.width;

            Rect area = new(row.x, row.y, width, row.height);

            _columns.Recalculate(width);
            _columns.ProcessDividers(area);

            if (_columns.TryDrawTitles(area, _filter, _dateRules.DefaultMeaning, out ETodoSort sort))
            {
                _filter.ApplySortClick(sort);
                _needsQuery = true;
            }

            TodoChrome.DrawSeparator(new Rect(row.x, row.yMax - TodoStyles.SeparatorThickness, row.width,
                TodoStyles.SeparatorThickness));
        }

        private void DrawList()
        {
            if (_rows.Count == 0)
            {
                DrawEmpty();
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            Rect content = GUILayoutUtility.GetRect(0f, _rows.Count * TodoStyles.RowHeight,
                GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
                _listWidth = content.width;

            int first = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / TodoStyles.RowHeight) - 1);
            int visible = Mathf.CeilToInt(position.height / TodoStyles.RowHeight) + 2;
            int last = Mathf.Min(_rows.Count, first + visible);

            for (int i = first; i < last; i++)
            {
                DrawRow(new Rect(content.x, content.y + i * TodoStyles.RowHeight, content.width,
                    TodoStyles.RowHeight), i);
            }

            _columns.DrawGuides(content);

            EditorGUILayout.EndScrollView();

            if (Event.current.type == EventType.Repaint)
                _viewHeight = GUILayoutUtility.GetLastRect().height;
        }

        private void DrawEmpty()
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label(EmptyMessage(), TodoStyles.Empty);

            GUILayout.FlexibleSpace();
        }

        private string EmptyMessage()
        {
            if (!_scanned)
                return EmptyScanning;

            return _entries.Count == 0
                ? EmptyProject
                : EmptyFiltered;
        }

        private void DrawRow(Rect row, int index)
        {
            TodoRow item = _rows[index];
            TodoGroup group = _groups[item.Group];

            if (item.IsHeader)
            {
                DrawHeaderRow(row, group);
                return;
            }

            TodoEntry entry = group.Entries[item.Entry];
            bool hovered = row.Contains(Event.current.mousePosition);

            TodoChrome.DrawRowBackground(row, _selectedRow == index, hovered, index % 2 == 0);

            Color color = _palette.Of(entry.Keyword);

            TodoChrome.DrawBand(_columns.BandRect(row), color);
            TodoChrome.DrawPill(_columns.KeywordRect(row), new GUIContent(entry.Keyword), color,
                TodoStyles.ChipStyle(color));

            GUI.Label(_columns.MessageRect(row), MessageContent(entry), TodoStyles.Message);
            GUI.Label(_columns.OwnerRect(row), entry.Owner, TodoStyles.Owner);
            GUI.Label(_columns.LocationRect(row), new GUIContent(entry.Location, entry.AssetPath),
                TodoStyles.Location);

            DrawDate(_columns.DateRect(row), entry);
            HandleRowInput(row, index, entry);
        }

        private void DrawDate(Rect rect, TodoEntry entry)
        {
            if (entry.RawDate.Length == 0)
                return;

            ETodoDateState state = _dateRules.Resolve(entry);

            // The raw text moves to the tooltip rather than being dropped, so the notation the comment
            // was written in is still there to check when a date looks wrong.
            TodoChrome.DrawPill(rect, new GUIContent(TodoDateLabel.Of(entry),
                    TodoDateLabel.TooltipOf(entry, _dateRules)),
                TodoStyles.DateColor(state), TodoStyles.DateStyle(state));
        }

        private void DrawHeaderRow(Rect row, TodoGroup group)
        {
            TodoChrome.DrawBand(row, TodoStyles.PanelColor());
            TodoChrome.DrawBand(new Rect(row.x, row.y, TodoStyles.BandWidth, row.height), group.Accent);

            Rect badge = new(row.xMax - TodoStyles.RowInset - TodoStyles.HeaderBadgeWidth, row.y,
                TodoStyles.HeaderBadgeWidth, row.height);

            Rect foldout = new(row.x + TodoStyles.RowInset, row.y, badge.x - row.x - TodoStyles.RowInset,
                row.height);

            bool expanded = !_collapsed.Contains(group.Label);
            bool result = EditorGUI.Foldout(foldout, expanded, group.Label, true, TodoStyles.Foldout);

            GUI.Label(badge, group.Entries.Count.ToString(), TodoStyles.Count);

            TodoChrome.DrawSeparator(new Rect(row.x, row.yMax - TodoStyles.SeparatorThickness, row.width,
                TodoStyles.SeparatorThickness));

            if (result == expanded)
                return;

            SetCollapsed(group.Label, !result);
        }

        // The list is being drawn while this runs, so the rows are rebuilt at the top of the next pass
        // rather than underneath the loop that is walking them.
        private void SetCollapsed(string label, bool collapsed)
        {
            if (collapsed)
                _collapsed.Add(label);
            else
                _collapsed.Remove(label);

            _needsRows = true;

            Repaint();
        }

        private void HandleRowInput(Rect row, int index, TodoEntry entry)
        {
            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || !row.Contains(current.mousePosition))
                return;

            Select(index);
            GUI.FocusControl(null);

            if (current.clickCount > 1)
                TodoNavigator.Open(entry);

            current.Use();
            Repaint();
        }

        // A click anywhere but in the field itself gives the caret up, so the hint comes back
        // without having to reach for the keyboard. The event is left alone for whatever was
        // actually clicked.
        private void HandleFocusRelease()
        {
            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || _searchRect.Contains(current.mousePosition))
                return;

            GUI.FocusControl(null);

            Repaint();
        }

        private void HandleKeyboard()
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown)
                return;

            if (current.keyCode == KeyCode.Escape)
            {
                ClearSearch();
                current.Use();

                return;
            }

            // Every key below moves through the list, which is not what a typed key should do
            // while the field has the caret.
            if (GUI.GetNameOfFocusedControl() == TodoToolbar.SearchControl)
                return;

            switch (current.keyCode)
            {
                case KeyCode.DownArrow:
                    MoveSelection(1);
                    current.Use();
                    break;

                case KeyCode.UpArrow:
                    MoveSelection(-1);
                    current.Use();
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    TodoNavigator.Open(_selected);
                    current.Use();
                    break;
            }
        }

        private void ClearSearch()
        {
            GUI.FocusControl(null);

            Repaint();

            if (_filter.Search.Length == 0)
                return;

            _filter.Search = string.Empty;
            _needsQuery = true;
        }

        private void MoveSelection(int step)
        {
            int index = _selectedRow;

            for (int i = 0; i < _rows.Count; i++)
            {
                index += step;

                if (index < 0 || index >= _rows.Count)
                    return;

                if (_rows[index].IsHeader)
                    continue;

                Select(index);
                ScrollTo(index);
                Repaint();

                return;
            }
        }

        private void Select(int index)
        {
            TodoRow row = _rows[index];

            _selectedRow = index;
            _selected = _groups[row.Group].Entries[row.Entry];
        }

        private void ScrollTo(int index)
        {
            float top = index * TodoStyles.RowHeight;

            if (top < _scroll.y)
            {
                _scroll.y = top;
                return;
            }

            float bottom = top + TodoStyles.RowHeight;

            if (bottom > _scroll.y + _viewHeight)
                _scroll.y = bottom - _viewHeight;
        }

        private void DrawDetail()
        {
            if (_selected == null)
                return;

            Rect area = GUILayoutUtility.GetRect(0f, TodoStyles.DetailHeight, GUILayout.ExpandWidth(true));

            TodoDetailPane.Draw(area, _selected, _palette.Of(_selected.Keyword));
        }
    }
}