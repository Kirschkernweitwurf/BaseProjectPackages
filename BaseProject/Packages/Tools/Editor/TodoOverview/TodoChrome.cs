using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.TodoOverview
{
    /// <summary>
    /// The drawing primitives the window is built from: rounded fills, hairlines, pills, buttons and
    /// the row background. Unity rounds a texture's corners while drawing it, so nothing has to be
    /// generated or cleaned up.
    /// </summary>
    internal static class TodoChrome
    {
        private const int LeftMouseButton = 0;
        private const float NoAspect = 0f;
        private const float NoBorder = 0f;
        private const float NoRadius = 0f;

        /// <summary>Fills a rounded rectangle.</summary>
        /// <param name="rect">The area to fill.</param>
        /// <param name="color">The fill color, alpha included.</param>
        /// <param name="radius">The corner radius.</param>
        internal static void DrawFill(Rect rect, Color color, float radius)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, NoAspect, color, NoBorder,
                radius);
        }

        /// <summary>Fills a rectangle with square corners, used for the colored bands.</summary>
        /// <param name="rect">The area to fill.</param>
        /// <param name="color">The fill color, alpha included.</param>
        internal static void DrawBand(Rect rect, Color color) => DrawFill(rect, color, NoRadius);

        /// <summary>Fills the given rectangle with the hairline color.</summary>
        /// <param name="line">The area of the line.</param>
        internal static void DrawSeparator(Rect line)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.DrawTexture(line, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, NoAspect,
                EditorPalette.Separator, NoBorder, NoRadius);
        }

        /// <summary>
        /// Fills the given rectangle with the color of a line between two columns, which the theme
        /// keeps apart from the hairline between two blocks because this one can be grabbed.
        /// </summary>
        /// <param name="line">The area of the line.</param>
        internal static void DrawDivider(Rect line)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.DrawTexture(line, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, NoAspect,
                EditorPalette.Divider, NoBorder, NoRadius);
        }

        /// <summary>
        /// Draws a small triangle pointing down, used on the dropdowns and on the column the list is
        /// sorted by. Built from stacked lines so it needs no font glyph and no texture.
        /// </summary>
        /// <param name="area">The area the triangle fills.</param>
        /// <param name="color">The color to draw it in.</param>
        /// <param name="pointUp">Whether the tip sits at the top instead of at the bottom.</param>
        internal static void DrawCaret(Rect area, Color color, bool pointUp)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            int rows = Mathf.Max(1, Mathf.RoundToInt(area.height));
            float step = area.width * 0.5f / rows;

            for (int i = 0; i < rows; i++)
            {
                float inset = step * i;

                float y = pointUp
                    ? area.yMax - 1f - i
                    : area.y + i;

                DrawFill(new Rect(area.x + inset, y, area.width - inset * 2f, 1f), color, NoRadius);
            }
        }

        /// <summary>Draws the background of a list row.</summary>
        /// <param name="rect">The full row rectangle.</param>
        /// <param name="selected">Whether the row is the selected one.</param>
        /// <param name="hovered">Whether the mouse sits on the row.</param>
        /// <param name="even">Whether this is an even row, which is the one that gets striped.</param>
        internal static void DrawRowBackground(Rect rect, bool selected, bool hovered, bool even)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (selected)
            {
                DrawFill(rect, EditorPalette.SelectionFill, NoRadius);
                return;
            }

            if (hovered)
            {
                DrawFill(rect, EditorPalette.Hover, NoRadius);
                return;
            }

            if (even)
                DrawFill(rect, EditorPalette.Stripe, NoRadius);
        }

        /// <summary>Draws a filled pill with a label centered in it.</summary>
        /// <param name="rect">The area of the pill.</param>
        /// <param name="content">The label and its tooltip.</param>
        /// <param name="fill">The fill color.</param>
        /// <param name="style">The style the label is drawn with.</param>
        internal static void DrawPill(Rect rect, GUIContent content, Color fill, GUIStyle style)
        {
            DrawFill(rect, fill, TodoStyles.ChipRadius);
            GUI.Label(rect, content, style);
        }

        /// <summary>
        /// Draws a rounded button that lights up under the mouse. Built by hand instead of with
        /// <see cref="GUI.Button"/> so the fill is a plain color that can carry the accent or the
        /// color of a keyword.
        /// </summary>
        /// <param name="rect">The area of the button.</param>
        /// <param name="content">The label and its tooltip.</param>
        /// <param name="fill">The fill color while the mouse is away from it.</param>
        /// <param name="style">The style the label is drawn with.</param>
        /// <param name="radius">The corner radius.</param>
        /// <returns><c>true</c> when the button was clicked.</returns>
        internal static bool DrawButton(Rect rect, GUIContent content, Color fill, GUIStyle style, float radius)
        {
            int control = GUIUtility.GetControlID(FocusType.Passive, rect);
            Event current = Event.current;
            bool hover = rect.Contains(current.mousePosition);

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            switch (current.GetTypeForControl(control))
            {
                case EventType.MouseDown when hover && current.button == LeftMouseButton:
                    GUIUtility.hotControl = control;
                    current.Use();
                    break;

                case EventType.MouseUp when GUIUtility.hotControl == control:
                    GUIUtility.hotControl = 0;
                    current.Use();

                    return hover;

                case EventType.Repaint:
                    DrawFill(rect, EditorStyleUtility.Shade(fill, hover, GUIUtility.hotControl == control), radius);
                    GUI.Label(rect, content, style);
                    break;
            }

            return false;
        }

        /// <summary>
        /// Draws a button that opens a menu, marked with a caret on its right. Unlike a plain button
        /// this one reports on the press rather than on the release and never takes the hot control,
        /// which is how the editor's own dropdowns behave: the menu has to take over the input while
        /// the mouse is still down, or it opens into a half finished click.
        /// </summary>
        /// <param name="rect">The area of the button.</param>
        /// <param name="content">The label and its tooltip.</param>
        /// <returns><c>true</c> when the menu should be opened.</returns>
        internal static bool DrawDropdown(Rect rect, GUIContent content)
        {
            int control = GUIUtility.GetControlID(FocusType.Passive, rect);
            Event current = Event.current;
            bool hover = rect.Contains(current.mousePosition);

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            switch (current.GetTypeForControl(control))
            {
                case EventType.MouseDown when hover && current.button == LeftMouseButton:
                    current.Use();

                    return true;

                case EventType.Repaint:
                    DrawFill(rect, EditorStyleUtility.Shade(TodoStyles.ControlColor(), hover, false),
                        TodoStyles.ButtonRadius);

                    GUI.Label(rect, content, TodoStyles.Dropdown);

                    DrawCaret(new Rect(rect.xMax - TodoStyles.Gap - TodoStyles.CaretWidth,
                        rect.center.y - TodoStyles.CaretHeight * 0.5f, TodoStyles.CaretWidth,
                        TodoStyles.CaretHeight), EditorPalette.DimText, false);

                    break;
            }

            return false;
        }

        /// <summary>
        /// Draws a pill that can be clicked, used for the keyword filters above the list. A pill whose
        /// keyword is hidden loses its color, so what is filtered out can be seen at a glance.
        /// </summary>
        /// <param name="rect">The area of the pill.</param>
        /// <param name="content">The label and its tooltip.</param>
        /// <param name="color">The color of the keyword this pill stands for.</param>
        /// <param name="active">Whether the keyword is currently shown.</param>
        /// <returns><c>true</c> when the pill was clicked.</returns>
        internal static bool DrawFilterPill(Rect rect, GUIContent content, Color color, bool active)
        {
            Color fill = active
                ? color
                : TodoStyles.MutedChipColor();

            GUIStyle style = active
                ? TodoStyles.ChipStyle(color)
                : TodoStyles.MutedChip;

            return DrawButton(rect, content, fill, style, TodoStyles.ChipRadius);
        }
    }
}