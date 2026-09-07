using UnityEditor;
using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// Drawing helpers for the row-and-badge layout every Base list window uses: striped rows, hover
    /// and selection tints, hairline separators and measured badges.
    /// </summary>
    public static class EditorRows
    {
        private const float ArrowBaselineNudge = 0.18f;
        private const float ArrowFontScale = 1.15f;
        private const int MinSortArrowFontSize = 6;
        private const int MinSortArrowWidth = 3;

        private static readonly GUIContent AscendingArrow = new("\u25B2");
        private static readonly GUIContent DescendingArrow = new("\u25BC");

        private static GUIStyle _sortArrow;

        /// <summary>
        /// Fills a row background with the tint its current state calls for. Selection wins over
        /// hover, hover over striping, and an even row with no state is left untouched.
        /// </summary>
        /// <param name="row">The full row rectangle.</param>
        /// <param name="index">The row index, used for the stripe of every second row.</param>
        /// <param name="isHovered">Whether the mouse sits on the row.</param>
        /// <param name="isSelected">Whether the row is selected.</param>
        public static void DrawRowBackground(Rect row, int index, bool isHovered = false, bool isSelected = false)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (isSelected)
            {
                EditorGUI.DrawRect(row, EditorPalette.SelectionFill);
                return;
            }

            if (isHovered)
            {
                EditorGUI.DrawRect(row, EditorPalette.Hover);
                return;
            }

            if (index % 2 != 0)
                EditorGUI.DrawRect(row, EditorPalette.Stripe);
        }

        /// <summary>
        /// Draws a hairline across the given width.
        /// </summary>
        /// <param name="area">The area the line spans; only its width and top edge are used.</param>
        public static void DrawSeparator(Rect area)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(new Rect(area.x, area.y, area.width, EditorMetrics.SeparatorThickness),
                EditorPalette.Separator);
        }

        /// <summary>
        /// The width a badge needs for the given text.
        /// </summary>
        /// <param name="text">The badge text.</param>
        /// <param name="style">The style the text is measured with.</param>
        /// <param name="minimumWidth">A floor applied so a column of badges keeps one width.</param>
        /// <returns>The width to lay the badge out with.</returns>
        public static float MeasureBadge(string text, GUIStyle style, float minimumWidth = 0f)
        {
            if (style == null)
                return minimumWidth;

            return Mathf.Max(minimumWidth, style.CalcSize(new GUIContent(text)).x + EditorMetrics.BadgePadding);
        }

        /// <summary>
        /// Draws a filled badge, vertically centered in its cell.
        /// </summary>
        /// <param name="cell">The cell the badge is centered in.</param>
        /// <param name="text">The badge text.</param>
        /// <param name="fill">The badge background color.</param>
        /// <param name="style">The style the text is drawn with.</param>
        public static void DrawBadge(Rect cell, string text, Color fill, GUIStyle style)
            => DrawBadge(cell, EditorGUIUtility.TrTempContent(text), fill, style);

        /// <summary>
        /// Draws a filled badge, vertically centered in its cell.
        /// </summary>
        /// <param name="cell">The cell the badge is centered in.</param>
        /// <param name="content">The badge text, and the tooltip shown on hover.</param>
        /// <param name="fill">The badge background color.</param>
        /// <param name="style">The style the text is drawn with.</param>
        public static void DrawBadge(Rect cell, GUIContent content, Color fill, GUIStyle style)
        {
            Rect badge = new(cell.x, cell.y + (cell.height - EditorMetrics.BadgeHeight) * 0.5f,
                cell.width, EditorMetrics.BadgeHeight);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(badge, fill);

            GUI.Label(badge, content, style);
        }

        /// <summary>
        /// Draws the triangle that marks the column a list is sorted by, pointing up for ascending
        /// and down for descending. Nothing is drawn for <see cref="ESortOrder.Default"/>.
        /// </summary>
        /// <remarks>
        /// Drawn as a glyph rather than as stacked hairlines. A one pixel rectangle only stays a
        /// pixel while the editor draws at whole pixels, and at 125 or 150 percent display scaling
        /// each row lands between two of them and comes out at half brightness. Text is the one thing
        /// in this API that is already resolved against the real pixel grid.
        /// </remarks>
        /// <param name="area">The area to center the triangle in.</param>
        /// <param name="order">The order the column is sorted in.</param>
        /// <param name="color">The triangle color.</param>
        public static void DrawSortArrow(Rect area, ESortOrder order, Color color)
        {
            if (Event.current.type != EventType.Repaint || order == ESortOrder.Default)
                return;

            GUIStyle style = SortArrowStyle();
            Color previous = GUI.color;

            GUI.color = color;
            style.Draw(SortArrowArea(area), SortArrowContent(order), false, false, false, false);
            GUI.color = previous;
        }

        /// <summary>
        /// The glyph for one sort order, pointing the way that order reads.
        /// </summary>
        /// <param name="order">The order the column is sorted in.</param>
        /// <returns>The glyph, or an empty one for the unsorted state.</returns>
        internal static GUIContent SortArrowContent(ESortOrder order) => order switch
        {
            ESortOrder.Ascending => AscendingArrow,
            ESortOrder.Descending => DescendingArrow,
            _ => GUIContent.none
        };

        /// <summary>
        /// The area the glyph is drawn in, as wide as the theme asks for and centered on the row.
        /// </summary>
        /// <param name="area">The area handed to the caller.</param>
        /// <returns>The glyph rectangle.</returns>
        internal static Rect SortArrowArea(Rect area)
        {
            float width = Mathf.Max(MinSortArrowWidth, EditorMetrics.SortArrowWidth);

            // Nudged down by a fraction of its own size. A glyph is placed by its font box, and the
            // box reserves room under the baseline for descenders that a triangle does not have, so
            // centering the box leaves the ink sitting high.
            float nudge = Mathf.Round(width * ArrowBaselineNudge);

            return new Rect(area.x, area.y + nudge, width, area.height);
        }

        // Sized from the themed width rather than from the font, so the arrow still grows and shrinks
        // with the theme the way every other measurement here does.
        private static GUIStyle SortArrowStyle()
        {
            int size = Mathf.Max(MinSortArrowFontSize,
                Mathf.RoundToInt(EditorMetrics.SortArrowWidth * ArrowFontScale));

            if (_sortArrow == null)
                _sortArrow = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(),
                    margin = new RectOffset()
                };

            _sortArrow.fontSize = size;

            return _sortArrow;
        }

        /// <summary>
        /// Draws one vertical guide line per nesting level, so a deep tree stays readable.
        /// </summary>
        /// <param name="row">The full row rectangle.</param>
        /// <param name="depth">The nesting level of the row.</param>
        /// <param name="color">The color of the guide lines.</param>
        public static void DrawIndentGuides(Rect row, int depth, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            for (int level = 1; level <= depth; level++)
            {
                float x = row.x + level * EditorMetrics.Indent - EditorMetrics.Indent * 0.5f;

                EditorGUI.DrawRect(new Rect(x, row.y, EditorMetrics.SeparatorThickness, row.height), color);
            }
        }
    }
}