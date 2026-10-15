using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEngine;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// The title strip of the table: one title per column, the struct name over the columns that come
    /// from the same struct, the sort arrow and the draggable lines between columns.
    /// </summary>
    internal static class DataTableHeader
    {
        private const float GroupLineInset = 6f;
        private const string GroupSeparator = " > ";
        private const string GroupTooltipFormat = "The fields of {0}, one column each.";
        private const string SortHint = "Click to sort by this column, again to reverse, a third time to go back to "
            + "the order by name.";
        private const string TooltipSeparator = "\n\n";

        /// <summary>The height of the strip, taller when a struct name sits over some of the columns.</summary>
        /// <param name="columns">The columns of the table.</param>
        /// <returns>The height.</returns>
        internal static float Height(DataTableColumns columns) => columns.HasGroups
            ? EditorTableStyles.HeaderHeight * 2f
            : EditorTableStyles.HeaderHeight;

        /// <summary>Draws the strip and handles sort clicks and resizing.</summary>
        /// <param name="area">The strip area.</param>
        /// <param name="columns">The columns of the table.</param>
        /// <param name="sorting">The sort state, cycled by a click on a title.</param>
        /// <param name="scrollX">How far the table is scrolled sideways.</param>
        /// <param name="styles">The built window styles.</param>
        /// <returns>True when the sort changed.</returns>
        internal static bool Draw(Rect area, DataTableColumns columns, DataTableSorting sorting, float scrollX,
            DataHubStyles styles)
        {
            DataHubGui.Fill(area, EditorTableStyles.HeaderColor);

            GUI.BeginClip(area);

            Rect local = new(0f, 0f, area.width, area.height);
            float frozen = columns.FrozenWidth;

            // The scrolled titles first, clipped at the frozen ones so they slide under them.
            GUI.BeginClip(new Rect(frozen, 0f, Mathf.Max(0f, local.width - frozen), local.height));
            bool isSorted = DrawTitles(columns, sorting, scrollX, -frozen, false, styles);
            GUI.EndClip();

            if (DrawTitles(columns, sorting, scrollX, 0f, true, styles))
                isSorted = true;

            // Last, so the lines sit on top of the titles.
            columns.HandleDividers(local, scrollX);

            EditorRows.DrawSeparator(new Rect(0f, local.yMax - EditorMetrics.SeparatorThickness, local.width,
                EditorMetrics.SeparatorThickness));

            GUI.EndClip();

            return isSorted;
        }

        private static bool DrawTitles(DataTableColumns columns, DataTableSorting sorting, float scrollX, float shift,
            bool isFrozenPart, DataHubStyles styles)
        {
            IReadOnlyList<DataColumn> all = columns.All;
            bool isSorted = false;

            if (!isFrozenPart
                && columns.HasGroups)
                DrawGroups(columns, scrollX, shift, styles);

            for (int i = 0; i < all.Count; i++)
            {
                if (DataTableColumns.IsFrozen(i) != isFrozenPart)
                    continue;

                if (DrawTitle(columns, i, sorting, scrollX, shift, styles))
                    isSorted = true;
            }

            return isSorted;
        }

        // One label over each run of columns that come from the same struct, like the four mood
        // scores, so the titles below can stay short.
        private static void DrawGroups(DataTableColumns columns, float scrollX, float shift, DataHubStyles styles)
        {
            IReadOnlyList<DataColumn> all = columns.All;
            int start = 0;

            while (start < all.Count)
            {
                string group = all[start].Group;
                int end = start;
                float width = all[start].Width;

                while (end + 1 < all.Count
                    && all[end + 1].Group == group)
                {
                    end++;
                    width += all[end].Width;
                }

                if (group != null)
                {
                    Rect area = new(columns.ScreenLeft(start, scrollX) + shift, 0f, width,
                        EditorTableStyles.HeaderHeight);

                    GUI.Label(area, new GUIContent(group, string.Format(GroupTooltipFormat, group)),
                        styles.GroupHeader);
                    EditorRows.DrawSeparator(new Rect(area.x + GroupLineInset,
                        area.yMax - EditorMetrics.SeparatorThickness, area.width - GroupLineInset * 2f,
                        EditorMetrics.SeparatorThickness));
                }

                start = end + 1;
            }
        }

        // A title outside a struct spans the whole strip, so there is no empty band above it.
        private static bool DrawTitle(DataTableColumns columns, int index, DataTableSorting sorting, float scrollX,
            float shift, DataHubStyles styles)
        {
            DataColumn column = columns.All[index];

            float top = column.Group == null
                ? 0f
                : EditorTableStyles.HeaderHeight;

            float height = column.Group == null
                ? Height(columns)
                : EditorTableStyles.HeaderHeight;

            Rect cell = new(columns.ScreenLeft(index, scrollX) + shift + DataHubStyles.CellPadding, top,
                Mathf.Max(0f, column.Width - DataHubStyles.CellPadding * 2f), height);

            string title = column.Group == null
                ? column.Header
                : column.Group + GroupSeparator + column.Header;

            string tooltip = title + TooltipSeparator + column.Tooltip + TooltipSeparator + SortHint;

            GUIContent content = new(column.Header, tooltip);
            GUIStyle style = StyleFor(column.Alignment, styles);

            GUI.Label(cell, content, style);

            if (sorting.ColumnIndex == index)
                DrawArrow(cell, content, column.Alignment, sorting.Order, style);

            return HandleClick(cell, index, columns, sorting, scrollX);
        }

        private static GUIStyle StyleFor(TextAnchor alignment, DataHubStyles styles) => alignment switch
        {
            TextAnchor.MiddleCenter => styles.HeaderCenter,
            TextAnchor.MiddleRight => styles.HeaderRight,
            _ => styles.Header
        };

        // Next to the title on the side away from the column edge it is aligned to, so a right-aligned
        // number column keeps its title flush with the numbers.
        private static void DrawArrow(Rect cell, GUIContent title, TextAnchor alignment, ESortOrder order,
            GUIStyle style)
        {
            float titleWidth = Mathf.Min(cell.width, style.CalcSize(title).x);
            float gap = EditorTableStyles.HeaderArrowGap;
            float arrowWidth = EditorMetrics.SortArrowWidth;

            float x = alignment switch
            {
                TextAnchor.MiddleRight => cell.xMax - titleWidth - gap - arrowWidth,
                TextAnchor.MiddleCenter => cell.center.x + titleWidth * 0.5f + gap,
                _ => cell.x + titleWidth + gap
            };

            EditorRows.DrawSortArrow(new Rect(x, cell.y, arrowWidth, cell.height), order, EditorPalette.Text);
        }

        private static bool HandleClick(Rect cell, int index, DataTableColumns columns, DataTableSorting sorting,
            float scrollX)
        {
            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || current.button != 0
                || !cell.Contains(current.mousePosition))
                return false;

            bool isFrozen = DataTableColumns.IsFrozen(index);

            // A scrolled title that slid under the frozen ones is hidden there and cannot be clicked.
            if (!isFrozen
                && current.mousePosition.x < 0f)
                return false;

            // The few pixels around a divider belong to the drag, not to the title behind them. The
            // dividers are measured from the strip's left edge, which the frozen part shares.
            float stripX = isFrozen
                ? current.mousePosition.x
                : current.mousePosition.x + columns.FrozenWidth;

            if (columns.IsOverDivider(stripX, scrollX))
                return false;

            sorting.Cycle(index);
            current.Use();

            return true;
        }
    }
}