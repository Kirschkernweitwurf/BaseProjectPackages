using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Drawing helpers for the row-and-badge layout every Base list window uses: striped rows, hover
    /// and selection tints, hairline separators and measured badges.
    /// </summary>
    public static class EditorRows
    {
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
        {
            Rect badge = new(cell.x, cell.y + (cell.height - EditorMetrics.BadgeHeight) * 0.5f,
                cell.width, EditorMetrics.BadgeHeight);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(badge, fill);

            GUI.Label(badge, text, style);
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