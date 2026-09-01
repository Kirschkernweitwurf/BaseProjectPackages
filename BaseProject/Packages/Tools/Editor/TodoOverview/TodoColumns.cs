using Base.ToolPackage.Editor.TodoOverview.Model;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.TodoOverview
{
    /// <summary>
    /// Owns the width of every column, the dividers between them and the titles above them. The
    /// message column takes whatever the other columns leave over, the rest are dragged to size and
    /// remembered between sessions.
    /// <para>
    /// Every cell a row draws into is handed out from here, so the titles and all rows share the
    /// exact same x positions no matter how wide the window is.
    /// </para>
    /// </summary>
    internal sealed class TodoColumns
    {
        private const string DateKey = "Base.ToolPackage.TodoOverview.Column.Date";
        private const string DateLabel = "Due";
        private const string KeywordKey = "Base.ToolPackage.TodoOverview.Column.Keyword";
        private const string KeywordLabel = "Type";
        private const string LocationKey = "Base.ToolPackage.TodoOverview.Column.Location";
        private const string LocationLabel = "File";
        private const string MessageLabel = "Item";
        private const string OwnerKey = "Base.ToolPackage.TodoOverview.Column.Owner";
        private const string OwnerLabel = "Owner";
        private const string SortTooltip = "Sort by {0}. Click again to turn it around, once more "
            + "for the default order";

        private float _savedDate;
        private float _savedKeyword;
        private float _savedLocation;
        private float _savedOwner;

        private float _date;
        private float _keyword;
        private float _location;
        private float _owner;

        private ETodoDivider _dragging = ETodoDivider.None;

        /// <summary>Restores the widths the user last dragged the columns to.</summary>
        internal TodoColumns()
        {
            _savedDate = EditorPrefs.GetFloat(DateKey, TodoStyles.DateWidth);
            _savedKeyword = EditorPrefs.GetFloat(KeywordKey, TodoStyles.ChipWidth);
            _savedLocation = EditorPrefs.GetFloat(LocationKey, TodoStyles.LocationWidth);
            _savedOwner = EditorPrefs.GetFloat(OwnerKey, TodoStyles.OwnerWidth);
        }

        /// <summary>Fits the columns into the given width. Call once per pass before anything draws.</summary>
        /// <param name="width">The width the whole table has to share.</param>
        internal void Recalculate(float width)
        {
            _date = Mathf.Max(_savedDate, TodoStyles.MinDateWidth);
            _keyword = Mathf.Max(_savedKeyword, TodoStyles.MinKeywordWidth);
            _location = Mathf.Max(_savedLocation, TodoStyles.MinLocationWidth);
            _owner = Mathf.Max(_savedOwner, TodoStyles.MinOwnerWidth);

            float taken = TodoStyles.RowInset * 2f + TodoStyles.Gap * 4f;
            float message = width - taken - _keyword - _owner - _date - _location;

            if (message >= TodoStyles.MinMessageWidth)
                return;

            // The message is what the eye actually reads, so the other columns give way to it.
            float deficit = TodoStyles.MinMessageWidth - message;

            deficit -= Reclaim(ref _location, TodoStyles.MinLocationWidth, deficit);
            deficit -= Reclaim(ref _owner, TodoStyles.MinOwnerWidth, deficit);
            deficit -= Reclaim(ref _date, TodoStyles.MinDateWidth, deficit);

            Reclaim(ref _keyword, TodoStyles.MinKeywordWidth, deficit);
        }

        /// <summary>The colored band in front of a row.</summary>
        /// <param name="row">The row the cell belongs to.</param>
        /// <returns>The band rectangle.</returns>
        internal Rect BandRect(Rect row) => new(row.x, row.y, TodoStyles.BandWidth, row.height);

        /// <summary>The keyword pill at the start of a row.</summary>
        /// <param name="row">The row the cell belongs to.</param>
        /// <returns>The keyword rectangle.</returns>
        internal Rect KeywordRect(Rect row) => Pill(row, row.x + TodoStyles.RowInset, _keyword);

        /// <summary>The text of the item, which takes what the other columns leave over.</summary>
        /// <param name="row">The row the cell belongs to.</param>
        /// <returns>The message rectangle.</returns>
        internal Rect MessageRect(Rect row)
        {
            float left = KeywordRect(row).xMax + TodoStyles.Gap;
            float width = Mathf.Max(0f, OwnerRect(row).x - TodoStyles.Gap - left);

            return Line(row, left, width);
        }

        /// <summary>The name of the responsible person.</summary>
        /// <param name="row">The row the cell belongs to.</param>
        /// <returns>The owner rectangle.</returns>
        internal Rect OwnerRect(Rect row) => Line(row, DateRect(row).x - TodoStyles.Gap - _owner, _owner);

        /// <summary>The date pill.</summary>
        /// <param name="row">The row the cell belongs to.</param>
        /// <returns>The date rectangle.</returns>
        internal Rect DateRect(Rect row) => Pill(row, LocationRect(row).x - TodoStyles.Gap - _date, _date);

        /// <summary>The file name and line number at the end of a row.</summary>
        /// <param name="row">The row the cell belongs to.</param>
        /// <returns>The location rectangle.</returns>
        internal Rect LocationRect(Rect row) => Line(row, row.xMax - TodoStyles.RowInset - _location, _location);

        /// <summary>
        /// Draws the column titles. A title that stands for a sort order can be clicked to switch the
        /// list over to it, and the one currently in use is marked.
        /// </summary>
        /// <param name="row">The title row.</param>
        /// <param name="filter">What the list is currently sorted by, and in which direction.</param>
        /// <param name="clicked">The order the clicked title stands for.</param>
        /// <returns><c>true</c> when a title was clicked.</returns>
        internal bool TryDrawTitles(Rect row, TodoFilter filter, out ETodoSort clicked)
        {
            clicked = filter.Sort;

            // Not short circuiting, so every title is drawn whatever the one before it reported.
            bool hit = TryDrawTitle(Cell(row, KeywordRect(row)), KeywordLabel, ETodoSort.Keyword, filter,
                ref clicked);

            hit |= TryDrawTitle(Cell(row, MessageRect(row)), MessageLabel, ETodoSort.Message, filter, ref clicked);
            hit |= TryDrawTitle(Cell(row, OwnerRect(row)), OwnerLabel, ETodoSort.Owner, filter, ref clicked);
            hit |= TryDrawTitle(Cell(row, DateRect(row)), DateLabel, ETodoSort.Date, filter, ref clicked);
            hit |= TryDrawTitle(Cell(row, LocationRect(row)), LocationLabel, ETodoSort.Location, filter,
                ref clicked);

            return hit;
        }

        /// <summary>Draws the dividers in the title row and processes a drag on them.</summary>
        /// <param name="row">The title row the dividers are grabbed in.</param>
        internal void ProcessDividers(Rect row)
        {
            HandleDivider(ETodoDivider.KeywordMessage, DividerX(KeywordRect(row).xMax), row);
            HandleDivider(ETodoDivider.MessageOwner, DividerX(OwnerRect(row).x - TodoStyles.Gap), row);
            HandleDivider(ETodoDivider.OwnerDate, DividerX(DateRect(row).x - TodoStyles.Gap), row);
            HandleDivider(ETodoDivider.DateLocation, DividerX(LocationRect(row).x - TodoStyles.Gap), row);
        }

        /// <summary>Draws the column lines down the list, so the rows read as a table.</summary>
        /// <param name="area">The area the whole list covers.</param>
        internal void DrawGuides(Rect area)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            DrawGuide(DividerX(KeywordRect(area).xMax), area);
            DrawGuide(DividerX(OwnerRect(area).x - TodoStyles.Gap), area);
            DrawGuide(DividerX(DateRect(area).x - TodoStyles.Gap), area);
            DrawGuide(DividerX(LocationRect(area).x - TodoStyles.Gap), area);
        }

        private static Rect Cell(Rect row, Rect column) => new(column.x, row.y, column.width, row.height);

        private static float DividerX(float edge) => edge + TodoStyles.Gap * 0.5f;

        private static void DrawGuide(float x, Rect area)
            => TodoChrome.DrawSeparator(new Rect(x, area.y, TodoStyles.SeparatorThickness, area.height));

        private static Rect Line(Rect row, float x, float width)
        {
            float height = EditorGUIUtility.singleLineHeight;

            return new Rect(x, row.y + (row.height - height) * 0.5f, width, height);
        }

        private static Rect Pill(Rect row, float x, float width) => new(x,
            row.y + (row.height - TodoStyles.ChipHeight) * 0.5f, width, TodoStyles.ChipHeight);

        private static float Reclaim(ref float width, float minimum, float deficit)
        {
            float taken = Mathf.Min(Mathf.Max(0f, deficit), width - minimum);

            width -= taken;

            return taken;
        }

        private static bool TryDrawTitle(Rect cell, string label, ETodoSort sort, TodoFilter filter,
            ref ETodoSort clicked)
        {
            GUIContent content = new(label, string.Format(SortTooltip, label));

            // No cursor rect here: it would sit on top of the divider's resize cursor and win.
            bool pressed = GUI.Button(cell, content, TodoStyles.Header);

            if (filter.Sort == sort)
                DrawSortMark(cell, TodoStyles.Header.CalcSize(content).x, filter.Descending);

            if (!pressed)
                return false;

            clicked = sort;

            return true;
        }

        private static void DrawSortMark(Rect cell, float textWidth, bool descending)
        {
            Rect caret = new(cell.x + textWidth + TodoStyles.TightGap,
                cell.center.y - TodoStyles.CaretHeight * 0.5f, TodoStyles.CaretWidth, TodoStyles.CaretHeight);

            TodoChrome.DrawCaret(caret, TodoStyles.AccentColor(), descending);
        }

        private void HandleDivider(ETodoDivider divider, float x, Rect row)
        {
            Rect hit = new(x - TodoStyles.DividerHitWidth * 0.5f, row.y, TodoStyles.DividerHitWidth, row.height);

            EditorGUIUtility.AddCursorRect(hit, MouseCursor.ResizeHorizontal);

            Event current = Event.current;

            switch (current.type)
            {
                case EventType.Repaint:
                    DrawGuide(x, row);
                    break;

                case EventType.MouseDown when hit.Contains(current.mousePosition):
                    _dragging = divider;
                    current.Use();
                    break;

                case EventType.MouseDrag when _dragging == divider:
                    Resize(divider, current.mousePosition.x, row);
                    current.Use();
                    break;

                case EventType.MouseUp when _dragging == divider:
                    _dragging = ETodoDivider.None;
                    Save();
                    current.Use();
                    break;
            }
        }

        // Every column but the keyword one hangs off the right edge, so their width grows as the
        // divider is dragged to the left.
        private void Resize(ETodoDivider divider, float mouseX, Rect row)
        {
            float half = TodoStyles.Gap * 0.5f;

            switch (divider)
            {
                case ETodoDivider.KeywordMessage:
                    _savedKeyword = Mathf.Max(TodoStyles.MinKeywordWidth,
                        mouseX - half - row.x - TodoStyles.RowInset);

                    break;

                case ETodoDivider.MessageOwner:
                    _savedOwner = Mathf.Max(TodoStyles.MinOwnerWidth, OwnerRect(row).xMax - mouseX - half);
                    break;

                case ETodoDivider.OwnerDate:
                    _savedDate = Mathf.Max(TodoStyles.MinDateWidth, DateRect(row).xMax - mouseX - half);
                    break;

                case ETodoDivider.DateLocation:
                    _savedLocation = Mathf.Max(TodoStyles.MinLocationWidth,
                        LocationRect(row).xMax - mouseX - half);

                    break;
            }
        }

        private void Save()
        {
            EditorPrefs.SetFloat(DateKey, _savedDate);
            EditorPrefs.SetFloat(KeywordKey, _savedKeyword);
            EditorPrefs.SetFloat(LocationKey, _savedLocation);
            EditorPrefs.SetFloat(OwnerKey, _savedOwner);
        }
    }
}