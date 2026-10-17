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
    /// only the rows in view are drawn, so a few hundred assets stay responsive. The name and the check
    /// stay put while the other columns scroll sideways.
    /// </summary>
    internal sealed class DataTableView
    {
        private const string AlsoWarningsFormat = "\n\nAlso worth a second look:\n{0}";
        private const string ClearFilterLabel = "Clear filters";
        private const string CreateFirstFormat = "New {0}";
        private const string DragManyFormat = "{0} assets";
        private const string EditUndoFormat = "Edit {0}";
        private const float EmptyButtonGap = 12f;
        private const string EmptyHint = "Press New to create the first one and fill the table.";
        private const string EmptyMessage = "No assets of this type yet";
        private const float FrozenShadowAlpha = 0.18f;
        private const float FrozenShadowWidth = 4f;
        private const string NoMatchHint = "No row matches the search or filter.";
        private const string NoMatchMessage = "Nothing matches";
        private const int NoRow = -1;
        private const float OkIconAlpha = 0.55f;
        private const string OkTooltip = "All good, nothing to fix here.";
        private const string PingTooltip = "Click to find this asset in the Project window.";
        private const string WarningsTooltipFormat = "Worth a second look:\n{0}";
        private const float WheelStep = 20f;

        private readonly DataTableColumns _columns;
        private readonly List<DataRow> _filtered = new();
        private readonly List<DataRow> _rows = new();
        private readonly HashSet<DataRow> _selection = new();
        private readonly DataTableSorting _sorting = new();

        private int _anchorIndex = NoRow;
        private Rect _body;
        private int _cursorIndex = NoRow;
        private DataRow _dragRow;
        private int _hoveredIndex = NoRow;
        private bool _isFitted;
        private bool _needsFilter;
        private EDataRowFilter _filter;
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
            computed.RemoveAll(column => table.IsColumnHidden(column.GetType()));

            _columns = new DataTableColumns(table.DataType.FullName,
                DataColumnBuilder.Build(table.DataType, computed, registry.FieldTextFor, table.IsFieldHidden));
        }

        /// <summary>The table entry this view shows.</summary>
        internal IDataTable Table { get; }

        /// <summary>Number of assets of the type.</summary>
        internal int RowCount => _rows.Count;

        /// <summary>Number of assets left after search and filter.</summary>
        internal int FilteredCount => _filtered.Count;

        /// <summary>Number of assets with at least one problem.</summary>
        internal int ProblemCount { get; private set; }

        /// <summary>Number of assets with at least one warning.</summary>
        internal int WarningCount { get; private set; }

        /// <summary>The selected assets in table order. The same array until the selection changes.</summary>
        internal Object[] SelectedAssets => _selectedAssets;

        /// <summary>The columns, in display order.</summary>
        internal IReadOnlyList<DataColumn> Columns => _columns.All;

        /// <summary>Every row of the table, shown or filtered out.</summary>
        internal IReadOnlyList<DataRow> Rows => _rows;

        /// <summary>The rows the search and the filter leave, in table order.</summary>
        internal IReadOnlyList<DataRow> ShownRows => _filtered;

        /// <summary>The selected rows, in table order.</summary>
        internal IReadOnlyList<DataRow> SelectedRows => _filtered.FindAll(row => _selection.Contains(row));

        /// <summary>The column of the cell the last right click hit, or null for none.</summary>
        internal DataColumn ContextColumn { get; private set; }

        /// <summary>The cell the last right click hit, in screen coordinates, for a box to open at.</summary>
        internal Rect ContextRect { get; private set; }

        /// <summary>The assets of every row the search and the filter leave, in table order.</summary>
        internal Object[] FilteredAssets => _filtered.ConvertAll(row => row.Asset).ToArray();

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

            // The problem and warning filters depend on the result.
            if (_filter != EDataRowFilter.All)
                _needsFilter = true;
        }

        /// <summary>Sets the search text and the check filter. Applied on the next layout pass.</summary>
        /// <param name="search">Text a row has to contain in its name or any cell.</param>
        /// <param name="filter">Which rows to show by what their checks found.</param>
        internal void SetFilter(string search, EDataRowFilter filter)
        {
            _search = search ?? string.Empty;
            _filter = filter;
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
        /// <returns>False when no shown row has the asset, because it is filtered out or not in the table.</returns>
        internal bool Select(Object asset)
        {
            int index = _filtered.FindIndex(row => row.Asset == asset);

            if (index < 0)
                return false;

            SelectSingle(index);
            ScrollTo(index);

            return true;
        }

        /// <summary>True when the table lists <paramref name="asset"/>, shown or filtered out.</summary>
        /// <param name="asset">The asset to look for.</param>
        /// <returns>True when a row has the asset.</returns>
        internal bool Contains(Object asset) => _rows.Exists(row => row.Asset == asset);

        /// <summary>Selects every row the filter shows.</summary>
        internal void SelectAll()
        {
            if (_filtered.Count == 0)
                return;

            _selection.Clear();
            _selection.UnionWith(_filtered);
            _anchorIndex = 0;
            _cursorIndex = _filtered.Count - 1;

            UpdateSelectedAssets();
        }

        /// <summary>
        /// Where the name of the one selected row is on screen, scrolled into view first, so a rename box
        /// can open right on it.
        /// </summary>
        /// <param name="rect">The name cell, in window coordinates.</param>
        /// <returns>False when not exactly one row is selected.</returns>
        internal bool TryGetNameRect(out Rect rect)
        {
            rect = default;

            if (_selectedAssets.Length != 1)
                return false;

            int index = _filtered.FindIndex(row => row.Asset == _selectedAssets[0]);

            if (index < 0)
                return false;

            ScrollTo(index);

            float rowHeight = EditorTableStyles.RowHeight;
            rect = new Rect(_body.x, _body.y + index * rowHeight - _scroll.y, _columns.All[0].Width, rowHeight);

            return true;
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

        /// <summary>
        /// Draws the header and the visible rows, and handles editing and selection. Call outside any
        /// clip or area, so the rename box opens at the right place.
        /// </summary>
        /// <param name="area">The area to fill, in window coordinates.</param>
        /// <param name="styles">The built window styles.</param>
        /// <param name="validation">The checks to run after an edit.</param>
        /// <param name="hooks">What the table asks the window to do.</param>
        /// <returns>True when the selection changed.</returns>
        internal bool Draw(Rect area, DataHubStyles styles, DataValidation validation, DataTableHooks hooks)
        {
            float headerHeight = DataTableHeader.Height(_columns);

            Rect header = new(area.x, area.y, area.width, headerHeight);
            _body = new Rect(area.x, header.yMax, area.width, Mathf.Max(0f, area.height - headerHeight));

            if (!_isFitted)
            {
                _isFitted = true;
                _columns.FitTitles(styles.Header, DataHubStyles.CellPadding);
            }

            HandleDragStart();

            if (DataTableHeader.Draw(header, _columns, _sorting, _scroll.x, styles, hooks.ShowColumnMenu))
                _needsFilter = true;

            if (_filtered.Count == 0)
            {
                DrawEmptyState(_body, styles, hooks);
                return false;
            }

            bool isChanged = DrawBody(_body, styles, validation, hooks);

            return HandleKeys(hooks) || isChanged;
        }

        private static Rect LineRect(Rect cell) => new(cell.x,
            cell.y + (cell.height - EditorGUIUtility.singleLineHeight) * 0.5f, cell.width,
            EditorGUIUtility.singleLineHeight);

        private static Rect Inset(Rect rect) => new(rect.x + DataHubStyles.CellPadding, rect.y,
            Mathf.Max(0f, rect.width - DataHubStyles.CellPadding * 2f), rect.height);

        // A quiet check when nothing is wrong, so the eye only stops at the red and amber ones.
        private static void DrawCheck(Rect cell, DataRow row, DataHubStyles styles)
        {
            if (!row.HasProblems
                && !row.HasWarnings)
            {
                float size = EditorTableStyles.IconSize;
                Rect icon = new(cell.center.x - size * 0.5f, cell.center.y - size * 0.5f, size, size);

                Color previous = GUI.color;
                GUI.color = EditorPalette.WithAlpha(previous, previous.a * OkIconAlpha);
                EditorIcons.Draw(icon, EditorIcons.Success);
                GUI.color = previous;

                GUI.Label(icon, new GUIContent(string.Empty, OkTooltip));

                return;
            }

            GUIContent content = CheckContent(row);

            Color fill = row.HasProblems
                ? EditorTableStyles.DangerBadgeColor
                : EditorTableStyles.WarningBadgeColor;

            float width = Mathf.Min(cell.width, DataHubGui.PillWidth(content.text, styles));

            Rect pill = new(cell.center.x - width * 0.5f, cell.center.y - EditorMetrics.BadgeHeight * 0.5f, width,
                EditorMetrics.BadgeHeight);

            DataHubGui.Pill(pill, content, fill, styles);
        }

        // The size the rows can use inside the scroll view, after the scrollbars that the content needs.
        // One bar can make the other necessary, so the check runs twice.
        private static Vector2 VisibleSize(Vector2 area, Vector2 content)
        {
            float barWidth = GUI.skin.verticalScrollbar.fixedWidth;
            float barHeight = GUI.skin.horizontalScrollbar.fixedHeight;
            Vector2 view = area;

            for (int pass = 0; pass < 2; pass++)
            {
                view.x = content.y > view.y
                    ? area.x - barWidth
                    : area.x;

                view.y = content.x > view.x
                    ? area.y - barHeight
                    : area.y;
            }

            return view;
        }

        // The count of the badge is what is broken; warnings are only counted when nothing is.
        private static GUIContent CheckContent(DataRow row)
        {
            if (!row.HasProblems)
                return new GUIContent(row.Warnings.Count.ToString(),
                    string.Format(WarningsTooltipFormat, row.WarningText));

            string tooltip = row.ProblemText;

            if (row.HasWarnings)
                tooltip += string.Format(AlsoWarningsFormat, row.WarningText);

            return new GUIContent(row.Problems.Count.ToString(), tooltip);
        }

        // A thin colored edge at the start of a row, so a broken or unused asset stands out while scrolling.
        private static void DrawCheckEdge(Rect rect, DataRow row)
        {
            if (!row.HasProblems
                && !row.HasWarnings)
                return;

            Color color = row.HasProblems
                ? EditorPalette.Danger
                : EditorPalette.Warning;

            DataHubGui.Fill(new Rect(rect.x, rect.y, DataHubStyles.ProblemEdgeWidth, rect.height), color);
        }

        private static bool PassesFilter(DataRow row, EDataRowFilter filter) => filter switch
        {
            EDataRowFilter.Problems => row.HasProblems,
            EDataRowFilter.Warnings => row.HasWarnings,
            _ => true
        };

        private void CountProblems()
        {
            ProblemCount = 0;
            WarningCount = 0;

            foreach (DataRow row in _rows)
            {
                if (row.HasProblems)
                    ProblemCount++;

                if (row.HasWarnings)
                    WarningCount++;
            }
        }

        private void ApplyFilter()
        {
            _needsFilter = false;
            _filtered.Clear();

            foreach (DataRow row in _rows)
            {
                if (!PassesFilter(row, _filter))
                    continue;

                if (Matches(row))
                    _filtered.Add(row);
            }

            _filtered.Sort((first, second) => _sorting.Compare(first, second, _columns.All));

            // A row the filter hides cannot stay selected, or an edit would reach an asset nobody sees.
            _selection.IntersectWith(_filtered);
            _anchorIndex = Mathf.Min(_anchorIndex, _filtered.Count - 1);
            _cursorIndex = Mathf.Min(_cursorIndex, _filtered.Count - 1);

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

        // An empty table offers to create the first asset, an empty result offers to clear the filter,
        // so the way out is right where the user is looking.
        private void DrawEmptyState(Rect body, DataHubStyles styles, DataTableHooks hooks)
        {
            bool isEmpty = _rows.Count == 0;

            Rect icon = EditorTableStyles.EmptyIconRect(body);
            DataHubGui.Icon(icon, DataHubIcons.Of(Table.DataType));

            Rect title = new(body.x, icon.yMax + EditorTableStyles.EmptyLineGap, body.width, EditorMetrics.RowHeight);

            GUI.Label(title, isEmpty
                ? EmptyMessage
                : NoMatchMessage, styles.EmptyTitle);

            Rect hint = new(body.x, title.yMax, body.width, EditorMetrics.RowHeight);

            GUI.Label(hint, isEmpty
                ? EmptyHint
                : NoMatchHint, styles.EmptyHint);

            GUIContent label = isEmpty
                ? new GUIContent(string.Format(CreateFirstFormat, ObjectNames.NicifyVariableName(Table.DataType.Name)),
                    DataHubIcons.New)
                : new GUIContent(ClearFilterLabel);

            GUIStyle style = isEmpty
                ? styles.HeaderPrimaryButton
                : styles.HeaderButton;

            float width = style.CalcSize(label).x;
            Rect button = new(body.center.x - width * 0.5f, hint.yMax + EmptyButtonGap, width,
                DataHubStyles.ButtonHeight);

            if (!GUI.Button(button, label, style))
                return;

            Action action = isEmpty
                ? hooks.Create
                : hooks.ClearFilter;

            action?.Invoke();
        }

        private bool DrawBody(Rect body, DataHubStyles styles, DataValidation validation, DataTableHooks hooks)
        {
            float rowHeight = EditorTableStyles.RowHeight;
            float frozen = _columns.FrozenWidth;
            float contentHeight = _filtered.Count * rowHeight;
            float scrolledWidth = _columns.TotalWidth - frozen;

            Rect scrollArea = new(body.x + frozen, body.y, Mathf.Max(0f, body.width - frozen), body.height);
            Vector2 view = VisibleSize(scrollArea.size, new Vector2(scrolledWidth, contentHeight));

            _viewHeight = view.y;
            _hoveredIndex = HoveredIndex(body, view.y);

            Rect frozenArea = new(body.x, body.y, frozen, view.y);
            HandleFrozenWheel(frozenArea, contentHeight);

            int first = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / rowHeight));
            int last = Mathf.Min(_filtered.Count, Mathf.CeilToInt((_scroll.y + view.y) / rowHeight) + 1);
            bool isChanged = false;

            _scroll = GUI.BeginScrollView(scrollArea, _scroll, new Rect(0f, 0f, scrolledWidth, contentHeight));

            Rect scrolledVisible = new(_scroll.x, _scroll.y, view.x, view.y);

            for (int i = first; i < last; i++)
            {
                Rect row = new(0f, i * rowHeight, Mathf.Max(scrolledWidth, view.x), rowHeight);

                if (DrawRow(row, i, false, scrolledVisible, styles, validation, hooks))
                    isChanged = true;
            }

            float gridHeight = Mathf.Min(view.y, contentHeight - _scroll.y);
            DrawGrid(false, _scroll.y, gridHeight);
            GUI.EndScrollView();

            GUI.BeginClip(frozenArea);

            Rect frozenVisible = new(0f, 0f, frozen, view.y);

            for (int i = first; i < last; i++)
            {
                Rect row = new(0f, i * rowHeight - _scroll.y, frozen, rowHeight);

                if (DrawRow(row, i, true, frozenVisible, styles, validation, hooks))
                    isChanged = true;
            }

            DrawGrid(true, 0f, gridHeight);
            GUI.EndClip();

            DrawFrozenEdge(frozenArea);

            return isChanged;
        }

        private int HoveredIndex(Rect body, float viewHeight)
        {
            Vector2 mouse = Event.current.mousePosition;

            if (!body.Contains(mouse)
                || mouse.y > body.y + viewHeight)
                return NoRow;

            int index = Mathf.FloorToInt((mouse.y - body.y + _scroll.y) / EditorTableStyles.RowHeight);

            return index < _filtered.Count
                ? index
                : NoRow;
        }

        // The frozen columns sit outside the scroll view, so a wheel turned over them is passed on by hand.
        private void HandleFrozenWheel(Rect frozenArea, float contentHeight)
        {
            Event current = Event.current;

            if (current.type != EventType.ScrollWheel
                || !frozenArea.Contains(current.mousePosition))
                return;

            float maxScroll = Mathf.Max(0f, contentHeight - _viewHeight);
            _scroll.y = Mathf.Clamp(_scroll.y + current.delta.y * WheelStep, 0f, maxScroll);

            current.Use();
        }

        private bool DrawRow(Rect rect, int index, bool isFrozenPart, Rect visible, DataHubStyles styles,
            DataValidation validation, DataTableHooks hooks)
        {
            DataRow row = _filtered[index];

            // Deleted from outside since the last rebuild, which the next project change takes care of.
            if (row.Asset == null)
                return false;

            // Once per row, and the scrolled part is drawn first.
            if (!isFrozenPart)
                row.Serialized.UpdateIfRequiredOrScript();

            bool isSelected = _selection.Contains(row);

            EditorRows.DrawRowBackground(rect, index, index == _hoveredIndex, isSelected);

            if (isFrozenPart)
                DrawCheckEdge(rect, row);

            // Before the cells, so a click into a cell control also selects its row.
            bool isChanged = HandleRowInput(rect, index, row, isFrozenPart, visible, hooks);

            float shift = isFrozenPart
                ? 0f
                : _columns.FrozenWidth;

            for (int i = 0; i < _columns.All.Count; i++)
            {
                if (DataTableColumns.IsFrozen(i) != isFrozenPart)
                    continue;

                DataColumn column = _columns.All[i];
                float left = _columns.Left(i) - shift;

                // Columns scrolled out of view sideways are skipped like rows below the fold.
                if (left + column.Width < visible.x
                    || left > visible.xMax)
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
                    GUIStyle style = isSelected
                        ? styles.NameBold
                        : styles.Name;

                    string tooltip = AssetDatabase.GetAssetPath(row.Asset);
                    Rect icon = DataCell.DrawAssetName(cell, row.Asset, style, tooltip);
                    GUI.Label(icon, new GUIContent(string.Empty, PingTooltip));

                    break;

                case EDataColumnKind.Check:
                    DrawCheck(cell, row, styles);
                    break;

                case EDataColumnKind.Field:
                    DrawField(cell, column, row, isSelected, styles, validation);
                    break;

                case EDataColumnKind.Computed:
                case EDataColumnKind.FieldText:
                    string text = row.CellText(column);

                    GUIStyle detail = column.Alignment == TextAnchor.MiddleRight
                        ? styles.DetailNumber
                        : styles.Detail;

                    GUI.Label(cell, new GUIContent(text, text), detail);

                    break;
            }
        }

        private void DrawField(Rect cell, DataColumn column, DataRow row, bool isSelected, DataHubStyles styles,
            DataValidation validation)
        {
            SerializedProperty property = row.Serialized.FindProperty(column.PropertyPath);

            // A row of a derived type always has the base type's fields, so this only happens for an
            // asset whose script changed since the columns were built.
            if (property == null)
                return;

            if (!DataCell.Draw(LineRect(cell), property, column, isSelected, styles))
                return;

            ApplyEdit(row, column, property, validation);
        }

        // One line per column edge over the rows, not per cell, and none below the last row.
        private void DrawGrid(bool isFrozenPart, float top, float height)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float shift = isFrozenPart
                ? 0f
                : _columns.FrozenWidth;

            for (int i = 0; i < _columns.All.Count; i++)
            {
                // The edge after the last frozen column is drawn stronger, by the caller.
                if (DataTableColumns.IsFrozen(i) != isFrozenPart
                    || i == DataTableColumns.FrozenCount - 1)
                    continue;

                float x = _columns.Left(i) + _columns.All[i].Width - shift - EditorMetrics.SeparatorThickness;

                EditorGUI.DrawRect(new Rect(x, top, EditorMetrics.SeparatorThickness, height),
                    DataHubStyles.GridColor);
            }
        }

        // A hairline at the frozen edge, and a soft shadow once columns slide under it.
        private void DrawFrozenEdge(Rect frozenArea)
        {
            DataHubGui.Fill(new Rect(frozenArea.xMax - EditorMetrics.SeparatorThickness, frozenArea.y,
                EditorMetrics.SeparatorThickness, frozenArea.height), EditorPalette.Separator);

            if (_scroll.x <= 0f)
                return;

            DataHubGui.Fill(new Rect(frozenArea.xMax, frozenArea.y, FrozenShadowWidth, frozenArea.height),
                new Color(0f, 0f, 0f, FrozenShadowAlpha));
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

        private bool HandleRowInput(Rect rect, int index, DataRow row, bool isFrozenPart, Rect visible,
            DataTableHooks hooks)
        {
            Event current = Event.current;

            if (!rect.Contains(current.mousePosition)
                || !visible.Contains(current.mousePosition))
                return false;

            if (current.type == EventType.ContextClick)
            {
                if (!_selection.Contains(row))
                    SelectSingle(index);

                ContextColumn = ColumnAt(current.mousePosition.x, isFrozenPart, rect, out Rect cell);
                ContextRect = GUIUtility.GUIToScreenRect(cell);

                current.Use();
                hooks.ShowContextMenu?.Invoke();

                return true;
            }

            if (current.type != EventType.MouseDown
                || current.button != 0)
                return false;

            // Only the frozen name and badge are not controls. A click anywhere else has to reach the cell.
            if (isFrozenPart)
                return HandleNameClick(index, row, hooks);

            if (current.shift)
                SelectRange(index);
            else if (EditorGUI.actionKey)
                Toggle(index, row);
            else if (!_selection.Contains(row)
                || _selection.Count == 1)
                SelectSingle(index);
            else
                return false;

            return true;
        }

        // The column under an x of one part of the table, which lays out its own columns from zero.
        private DataColumn ColumnAt(float x, bool isFrozenPart, Rect row, out Rect cell)
        {
            float shift = isFrozenPart
                ? 0f
                : _columns.FrozenWidth;

            for (int i = 0; i < _columns.All.Count; i++)
            {
                if (DataTableColumns.IsFrozen(i) != isFrozenPart)
                    continue;

                DataColumn column = _columns.All[i];
                cell = new Rect(_columns.Left(i) - shift, row.y, column.Width, row.height);

                if (x >= cell.x
                    && x < cell.xMax)
                    return column;
            }

            cell = row;
            return null;
        }

        private bool HandleNameClick(int index, DataRow row, DataTableHooks hooks)
        {
            Event current = Event.current;
            current.Use();

            // Remembered so a drag that starts here carries the rows out of the table.
            _dragRow = row;

            float iconRight = DataHubStyles.CellPadding + EditorTableStyles.IconSize;

            if (current.mousePosition.x <= iconRight)
                EditorGUIUtility.PingObject(row.Asset);

            if (current.shift)
            {
                SelectRange(index);
                return true;
            }

            if (EditorGUI.actionKey)
            {
                Toggle(index, row);
                return true;
            }

            SelectSingle(index);

            if (current.clickCount == 2)
                hooks.Rename?.Invoke();

            return true;
        }

        // Dragging a name drags the selected rows, so they can be dropped on a list in the Contents tab,
        // on a field in any inspector, or in the Project window.
        private void HandleDragStart()
        {
            Event current = Event.current;

            if (current.type == EventType.MouseUp)
                _dragRow = null;

            if (current.type != EventType.MouseDrag
                || _dragRow == null)
                return;

            Object[] dragged = _selection.Contains(_dragRow)
                ? _selectedAssets
                : new[] { _dragRow.Asset };

            _dragRow = null;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = dragged;
            DragAndDrop.StartDrag(dragged.Length == 1
                ? dragged[0].name
                : string.Format(DragManyFormat, dragged.Length));

            current.Use();
        }

        private bool HandleKeys(DataTableHooks hooks)
        {
            Event current = Event.current;

            if (current.type != EventType.KeyDown
                || GUIUtility.keyboardControl != 0
                || _filtered.Count == 0)
                return false;

            if (current.keyCode == KeyCode.F2)
            {
                current.Use();
                hooks.Rename?.Invoke();

                return false;
            }

            if (!TryGetKeyTarget(current.keyCode, out int target))
                return false;

            if (current.shift)
                SelectRange(target);
            else
                SelectSingle(target);

            ScrollTo(target);
            current.Use();

            return true;
        }

        private bool TryGetKeyTarget(KeyCode key, out int target)
        {
            switch (key)
            {
                case KeyCode.DownArrow:
                    target = _cursorIndex + 1;
                    break;

                case KeyCode.UpArrow:
                    target = _cursorIndex - 1;
                    break;

                case KeyCode.Home:
                    target = 0;
                    break;

                case KeyCode.End:
                    target = _filtered.Count - 1;
                    break;

                default:
                    target = NoRow;
                    return false;
            }

            target = Mathf.Clamp(target, 0, _filtered.Count - 1);

            return true;
        }

        private void SelectSingle(int index)
        {
            _selection.Clear();
            _selection.Add(_filtered[index]);
            _anchorIndex = index;
            _cursorIndex = index;

            UpdateSelectedAssets();
        }

        private void Toggle(int index, DataRow row)
        {
            if (!_selection.Remove(row))
                _selection.Add(row);

            _anchorIndex = index;
            _cursorIndex = index;

            UpdateSelectedAssets();
        }

        // From the anchor, which stays, to the given row, which becomes the cursor that the arrow keys move.
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

            _cursorIndex = index;

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