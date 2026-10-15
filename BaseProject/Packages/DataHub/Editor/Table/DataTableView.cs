using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Extensions;
using Base.DataHubPackage.Editor.Validation;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// The table in the middle of the Data Hub: one row per asset of a type, one column per serialized
    /// field. Cells edit in place, an edit on a selected row is copied to every other selected row, and
    /// only the rows in view are drawn, so a few hundred assets stay responsive.
    /// </summary>
    internal sealed class DataTableView
    {
        private const string EditUndoFormat = "Edit {0}";
        private const string EmptyHint = "Press New to create the first one.";
        private const string EmptyMessage = "No assets of this type yet";

        // The name and the check badge always come first and are the only cells that are not controls.
        private const int FixedColumnCount = 2;
        private const string NoMatchHint = "Clear the search box or switch the problem filter off.";
        private const string NoMatchMessage = "Nothing matches the filter";
        private const int NoRow = -1;
        private const string OkBadgeText = "OK";
        private const string OkTooltip = "No problems found.";

        private readonly DataTableColumns _columns;
        private readonly List<DataRow> _filtered = new();
        private readonly List<DataRow> _rows = new();
        private readonly HashSet<DataRow> _selection = new();
        private readonly DataTableSorting _sorting = new();

        private int _anchorIndex = NoRow;
        private bool _needsFilter;
        private bool _problemsOnly;
        private Vector2 _scroll;
        private string _search = string.Empty;
        private Object[] _selectedAssets = Array.Empty<Object>();
        private float _viewHeight;

        /// <summary>Creates the view for one table entry.</summary>
        /// <param name="table">The table entry.</param>
        /// <param name="registry">The extensions, for the computed columns.</param>
        internal DataTableView(IDataTable table, DataHubRegistry registry)
        {
            Table = table;

            List<IDataColumn> computed = new();
            registry.CollectColumns(table.DataType, computed);

            _columns = new DataTableColumns(table.DataType.FullName, DataColumnBuilder.Build(table.DataType, computed));
        }

        /// <summary>The table entry this view shows.</summary>
        internal IDataTable Table { get; }

        /// <summary>Number of assets of the type.</summary>
        internal int RowCount => _rows.Count;

        /// <summary>Number of assets left after search and filter.</summary>
        internal int FilteredCount => _filtered.Count;

        /// <summary>Number of assets with at least one problem.</summary>
        internal int ProblemCount { get; private set; }

        /// <summary>The selected assets in table order. The same array until the selection changes.</summary>
        internal Object[] SelectedAssets => _selectedAssets;

        /// <summary>The asset of the first row, for a fallback folder, or null for an empty table.</summary>
        internal Object FirstAsset => _rows.Count > 0
            ? _rows[0].Asset
            : null;

        /// <summary>Loads every asset of the type again and checks it. The selection survives.</summary>
        /// <param name="index">The assets of the project.</param>
        /// <param name="validation">The checks to run on each row.</param>
        internal void Rebuild(DataAssetIndex index, DataValidation validation)
        {
            HashSet<Object> selected = new(_selectedAssets);

            Dispose();

            List<string> paths = new();
            index.CollectPaths(Table.DataType, paths);

            foreach (string path in paths)
            {
                Object asset = AssetDatabase.LoadMainAssetAtPath(path);

                if (asset == null)
                    continue;

                DataRow row = new(asset);
                validation.Validate(row);
                _rows.Add(row);

                if (selected.Contains(asset))
                    _selection.Add(row);
            }

            CountProblems();
            ApplyFilter();
        }

        /// <summary>Checks every row again, after an undo or an edit made outside the table.</summary>
        /// <param name="validation">The checks to run.</param>
        internal void Revalidate(DataValidation validation)
        {
            foreach (DataRow row in _rows)
                validation.Validate(row);

            CountProblems();

            // The problem filter depends on the result.
            if (_problemsOnly)
                _needsFilter = true;
        }

        /// <summary>Sets the search text and the problem filter. Applied on the next layout pass.</summary>
        /// <param name="search">Text a row has to contain in its name or any cell.</param>
        /// <param name="problemsOnly">True to show only rows with problems.</param>
        internal void SetFilter(string search, bool problemsOnly)
        {
            _search = search ?? string.Empty;
            _problemsOnly = problemsOnly;
            _needsFilter = true;
        }

        /// <summary>
        /// Applies a pending sort or filter. Call on the layout event only, because it changes which
        /// rows the next pass draws.
        /// </summary>
        internal void ApplyPending()
        {
            if (!_needsFilter)
                return;

            ApplyFilter();
        }

        /// <summary>Selects the row of <paramref name="asset"/> alone and scrolls it into view.</summary>
        /// <param name="asset">The asset to select.</param>
        internal void Select(Object asset)
        {
            int index = _filtered.FindIndex(row => row.Asset == asset);

            if (index < 0)
                return;

            SelectSingle(index);
            ScrollTo(index);
        }

        /// <summary>Drops a column resize in progress.</summary>
        internal void CancelDrag() => _columns.Cancel();

        /// <summary>Releases every row.</summary>
        internal void Dispose()
        {
            foreach (DataRow row in _rows)
                row.Dispose();

            _rows.Clear();
            _filtered.Clear();
            _selection.Clear();
        }

        /// <summary>Draws the header and the visible rows, and handles editing and selection.</summary>
        /// <param name="area">The area to fill.</param>
        /// <param name="styles">The built window styles.</param>
        /// <param name="validation">The checks to run after an edit.</param>
        /// <param name="showContextMenu">Called when a row is right-clicked, after it was selected.</param>
        /// <returns>True when the selection changed.</returns>
        internal bool Draw(Rect area, DataHubStyles styles, DataValidation validation, Action showContextMenu)
        {
            float headerHeight = _columns.HasGroups
                ? EditorTableStyles.HeaderHeight * 2f
                : EditorTableStyles.HeaderHeight;

            Rect header = new(area.x, area.y, area.width, headerHeight);
            Rect body = new(area.x, header.yMax, area.width, Mathf.Max(0f, area.height - headerHeight));

            DrawHeader(header, styles);

            if (_filtered.Count == 0)
            {
                DrawEmptyState(body, styles);
                return false;
            }

            bool isChanged = DrawBody(body, styles, validation, showContextMenu);

            return HandleKeys() || isChanged;
        }

        private static Rect LineRect(Rect cell) => new(cell.x,
            cell.y + (cell.height - EditorGUIUtility.singleLineHeight) * 0.5f, cell.width,
            EditorGUIUtility.singleLineHeight);

        private static Rect Inset(Rect rect) => new(rect.x + DataHubStyles.CellPadding, rect.y,
            Mathf.Max(0f, rect.width - DataHubStyles.CellPadding * 2f), rect.height);

        private static void DrawCheck(Rect cell, DataRow row, DataHubStyles styles)
        {
            GUIContent content = row.HasProblems
                ? new GUIContent(row.Problems.Count.ToString(), row.ProblemText)
                : new GUIContent(OkBadgeText, OkTooltip);

            Color fill = row.HasProblems
                ? EditorTableStyles.DangerBadgeColor
                : EditorTableStyles.OkBadgeColor;

            float width = Mathf.Min(cell.width,
                EditorRows.MeasureBadge(content.text, styles.Badge, EditorTableStyles.MinBadgeWidth));

            Rect badge = new(cell.x, cell.y + (cell.height - EditorMetrics.BadgeHeight) * 0.5f, width,
                EditorMetrics.BadgeHeight);

            EditorRows.DrawBadge(badge, content, fill, styles.Badge);

            // A drawn badge carries no tooltip of its own, so an empty label over it does.
            GUI.Label(badge, new GUIContent(string.Empty, content.tooltip));
        }

        private void CountProblems()
        {
            ProblemCount = 0;

            foreach (DataRow row in _rows)
            {
                if (row.HasProblems)
                    ProblemCount++;
            }
        }

        private void ApplyFilter()
        {
            _needsFilter = false;
            _filtered.Clear();

            foreach (DataRow row in _rows)
            {
                if (_problemsOnly && !row.HasProblems)
                    continue;

                if (Matches(row))
                    _filtered.Add(row);
            }

            _filtered.Sort((first, second) => _sorting.Compare(first, second, _columns.All));

            // A row the filter hides cannot stay selected, or an edit would reach an asset nobody sees.
            _selection.IntersectWith(_filtered);
            _anchorIndex = Mathf.Min(_anchorIndex, _filtered.Count - 1);

            UpdateSelectedAssets();
        }

        private bool Matches(DataRow row)
        {
            if (_search.Length == 0)
                return true;

            foreach (DataColumn column in _columns.All)
            {
                if (column.Kind == EDataColumnKind.Check)
                    continue;

                if (row.CellText(column).IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private void DrawHeader(Rect header, DataHubStyles styles)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(header, EditorTableStyles.HeaderColor);

            GUI.BeginClip(header);

            Rect local = new(0f, 0f, header.width, header.height);
            float offset = -_scroll.x;

            float titleY = _columns.HasGroups
                ? EditorTableStyles.HeaderHeight
                : 0f;

            if (_columns.HasGroups)
                DrawGroups(offset, styles);

            for (int i = 0; i < _columns.All.Count; i++)
            {
                Rect cell = new(offset + _columns.Left(i), titleY, _columns.All[i].Width,
                    EditorTableStyles.HeaderHeight);

                DrawTitle(Inset(cell), i, offset, styles);
            }

            // Last, so the lines sit on top of the titles.
            _columns.HandleDividers(local, offset);

            EditorRows.DrawSeparator(new Rect(0f, local.yMax - EditorMetrics.SeparatorThickness, local.width,
                EditorMetrics.SeparatorThickness));

            GUI.EndClip();
        }

        // One label over each run of columns that come from the same struct, like the four mood
        // scores, so the titles below can stay short.
        private void DrawGroups(float offset, DataHubStyles styles)
        {
            IReadOnlyList<DataColumn> columns = _columns.All;
            int start = 0;

            while (start < columns.Count)
            {
                string group = columns[start].Group;
                int end = start;
                float width = columns[start].Width;

                while (end + 1 < columns.Count
                    && columns[end + 1].Group == group)
                {
                    end++;
                    width += columns[end].Width;
                }

                if (group != null)
                {
                    Rect area = new(offset + _columns.Left(start), 0f, width, EditorTableStyles.HeaderHeight);

                    GUI.Label(Inset(area), group, styles.Header);
                    EditorRows.DrawSeparator(new Rect(area.x, area.yMax - EditorMetrics.SeparatorThickness,
                        area.width, EditorMetrics.SeparatorThickness));
                }

                start = end + 1;
            }
        }

        // The arrow sits right after the title, so it reads as belonging to that word.
        private void DrawTitle(Rect cell, int index, float offset, DataHubStyles styles)
        {
            DataColumn column = _columns.All[index];

            string tooltip = column.Group == null
                ? column.Header
                : column.Group + " " + column.Header;

            GUIContent title = new(column.Header, tooltip);
            GUI.Label(cell, title, styles.Header);

            if (_sorting.ColumnIndex == index
                && _sorting.Order != ESortOrder.Default)
            {
                float titleWidth = Mathf.Min(cell.width, styles.Header.CalcSize(title).x);
                Rect arrow = new(cell.x + titleWidth + EditorTableStyles.HeaderArrowGap, cell.y,
                    EditorMetrics.SortArrowWidth, cell.height);

                EditorRows.DrawSortArrow(arrow, _sorting.Order, EditorPalette.Text);
            }

            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || current.button != 0
                || !cell.Contains(current.mousePosition))
                return;

            // The few pixels around a divider belong to the drag, not to the title behind them.
            if (_columns.IsOverDivider(current.mousePosition.x, offset))
                return;

            _sorting.Cycle(index);
            _needsFilter = true;

            current.Use();
        }

        private void DrawEmptyState(Rect body, DataHubStyles styles)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            bool isEmpty = _rows.Count == 0;

            Rect icon = EditorTableStyles.EmptyIconRect(body);
            GUI.DrawTexture(icon, EditorIcons.Script, ScaleMode.ScaleToFit, true, 0f, EditorPalette.DimText, 0f, 0f);

            Rect title = new(body.x, icon.yMax + EditorTableStyles.EmptyLineGap, body.width, EditorMetrics.RowHeight);

            GUI.Label(title, isEmpty
                ? EmptyMessage
                : NoMatchMessage, styles.EmptyTitle);

            Rect hint = new(body.x, title.yMax, body.width, EditorMetrics.RowHeight);

            GUI.Label(hint, isEmpty
                ? EmptyHint
                : NoMatchHint, styles.EmptyHint);
        }

        private bool DrawBody(Rect body, DataHubStyles styles, DataValidation validation, Action showContextMenu)
        {
            float rowHeight = EditorTableStyles.RowHeight;
            float contentWidth = Mathf.Max(_columns.TotalWidth, body.width - GUI.skin.verticalScrollbar.fixedWidth);
            Rect content = new(0f, 0f, _columns.TotalWidth, _filtered.Count * rowHeight);

            _scroll = GUI.BeginScrollView(body, _scroll, content);
            _viewHeight = body.height;

            int first = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / rowHeight));
            int last = Mathf.Min(_filtered.Count, Mathf.CeilToInt((_scroll.y + body.height) / rowHeight) + 1);

            bool isChanged = false;

            for (int i = first; i < last; i++)
            {
                Rect row = new(0f, i * rowHeight, contentWidth, rowHeight);

                if (DrawRow(row, i, body.width, styles, validation, showContextMenu))
                    isChanged = true;
            }

            GUI.EndScrollView();

            return isChanged;
        }

        private bool DrawRow(Rect rect, int index, float viewWidth, DataHubStyles styles, DataValidation validation,
            Action showContextMenu)
        {
            DataRow row = _filtered[index];

            // Deleted from outside since the last rebuild, which the next project change takes care of.
            if (row.Asset == null)
                return false;

            row.Serialized.UpdateIfRequiredOrScript();

            bool isSelected = _selection.Contains(row);
            bool isHovered = rect.Contains(Event.current.mousePosition);

            EditorRows.DrawRowBackground(rect, index, isHovered, isSelected);

            // Before the cells, so a click into a cell control also selects its row.
            bool isChanged = HandleRowInput(rect, index, row, showContextMenu);

            for (int i = 0; i < _columns.All.Count; i++)
            {
                DataColumn column = _columns.All[i];
                float left = _columns.Left(i);

                // Columns scrolled out of view sideways are skipped like rows below the fold.
                if (left + column.Width < _scroll.x
                    || left > _scroll.x + viewWidth)
                    continue;

                Rect cell = Inset(new Rect(left, rect.y, column.Width, rect.height));

                DrawCell(cell, column, row, isSelected, styles, validation);
            }

            return isChanged;
        }

        private void DrawCell(Rect cell, DataColumn column, DataRow row, bool isSelected, DataHubStyles styles,
            DataValidation validation)
        {
            switch (column.Kind)
            {
                case EDataColumnKind.Name:
                    GUI.Label(cell, new GUIContent(row.Asset.name, AssetDatabase.GetAssetPath(row.Asset)), isSelected
                        ? styles.NameBold
                        : styles.Name);

                    break;

                case EDataColumnKind.Check:
                    DrawCheck(cell, row, styles);
                    break;

                case EDataColumnKind.Field:
                    DrawField(cell, column, row, styles, validation);
                    break;

                case EDataColumnKind.Computed:
                    string text = row.CellText(column);
                    GUI.Label(cell, new GUIContent(text, text), styles.Detail);

                    break;
            }
        }

        private void DrawField(Rect cell, DataColumn column, DataRow row, DataHubStyles styles,
            DataValidation validation)
        {
            SerializedProperty property = row.Serialized.FindProperty(column.PropertyPath);

            // A row of a derived type always has the base type's fields, so this only happens for an
            // asset whose script changed since the columns were built.
            if (property == null)
                return;

            if (!DataCell.Draw(LineRect(cell), property, column, styles.Detail))
                return;

            ApplyEdit(row, column, property, validation);
        }

        // One undo step for the whole edit, however many selected rows it reached.
        private void ApplyEdit(DataRow row, DataColumn column, SerializedProperty property, DataValidation validation)
        {
            int group = Undo.GetCurrentGroup();

            row.Serialized.ApplyModifiedProperties();
            validation.Validate(row);

            if (_selection.Contains(row))
            {
                foreach (DataRow other in _selection)
                {
                    if (other == row)
                        continue;

                    other.Serialized.Update();
                    DataCell.Copy(property, other.Serialized.FindProperty(column.PropertyPath));
                    other.Serialized.ApplyModifiedProperties();
                    validation.Validate(other);
                }
            }

            Undo.SetCurrentGroupName(string.Format(EditUndoFormat, column.Header));
            Undo.CollapseUndoOperations(group);

            CountProblems();
        }

        private bool HandleRowInput(Rect rect, int index, DataRow row, Action showContextMenu)
        {
            Event current = Event.current;

            if (!rect.Contains(current.mousePosition))
                return false;

            if (current.type == EventType.ContextClick)
            {
                if (!_selection.Contains(row))
                    SelectSingle(index);

                current.Use();
                showContextMenu?.Invoke();

                return true;
            }

            if (current.type != EventType.MouseDown
                || current.button != 0)
                return false;

            bool isOnName = current.mousePosition.x < _columns.Left(FixedColumnCount);

            // A double click on the name finds the asset in the Project window.
            if (isOnName
                && current.clickCount == 2)
                EditorGUIUtility.PingObject(row.Asset);

            if (current.shift)
                SelectRange(index);
            else if (EditorGUI.actionKey)
                Toggle(index, row);
            else if (isOnName
                || !_selection.Contains(row)
                || _selection.Count == 1)
                SelectSingle(index);
            else
                return false;

            // Only the name and the badge are not controls. A click anywhere else has to reach the cell.
            if (isOnName)
                current.Use();

            return true;
        }

        private bool HandleKeys()
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown
                || GUIUtility.keyboardControl != 0
                || _filtered.Count == 0)
                return false;

            int step = current.keyCode switch
            {
                KeyCode.DownArrow => 1,
                KeyCode.UpArrow => -1,
                _ => 0
            };

            if (step == 0)
                return false;

            int index = Mathf.Clamp(_anchorIndex + step, 0, _filtered.Count - 1);

            SelectSingle(index);
            ScrollTo(index);
            current.Use();

            return true;
        }

        private void SelectSingle(int index)
        {
            _selection.Clear();
            _selection.Add(_filtered[index]);
            _anchorIndex = index;

            UpdateSelectedAssets();
        }

        private void Toggle(int index, DataRow row)
        {
            if (!_selection.Remove(row))
                _selection.Add(row);

            _anchorIndex = index;

            UpdateSelectedAssets();
        }

        private void SelectRange(int index)
        {
            if (_anchorIndex < 0)
            {
                SelectSingle(index);
                return;
            }

            _selection.Clear();

            int from = Mathf.Min(_anchorIndex, index);
            int to = Mathf.Max(_anchorIndex, index);

            for (int i = from; i <= to; i++)
                _selection.Add(_filtered[i]);

            UpdateSelectedAssets();
        }

        // Rebuilt only when the selection changes, so the inspector panel sees the same array every
        // frame and keeps its cached editor.
        private void UpdateSelectedAssets()
        {
            List<Object> assets = new();

            foreach (DataRow row in _filtered)
            {
                if (_selection.Contains(row))
                    assets.Add(row.Asset);
            }

            _selectedAssets = assets.ToArray();
        }

        private void ScrollTo(int index)
        {
            float rowHeight = EditorTableStyles.RowHeight;
            float top = index * rowHeight;

            if (top < _scroll.y)
            {
                _scroll.y = top;
                return;
            }

            float bottom = top + rowHeight;

            if (bottom > _scroll.y + _viewHeight)
                _scroll.y = bottom - _viewHeight;
        }
    }
}