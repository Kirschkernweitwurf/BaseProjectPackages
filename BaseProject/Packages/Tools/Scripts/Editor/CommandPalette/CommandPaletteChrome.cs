using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// The drawing primitives the palette is built from: rounded fills and borders, hairlines,
    /// pill buttons and the keyboard caps in the footer. Unity can round a texture's corners while
    /// drawing it, so no textures have to be generated or cleaned up.
    /// </summary>
    internal static class CommandPaletteChrome
    {
        private const float HintGap = 14f;
        private const float KeyCapPadding = 6f;
        private const float LabelGap = 4f;
        private const int LeftMouseButton = 0;
        private const float NoAspect = 0f;
        private const float NoBorder = 0f;
        private const float NoRadius = 0f;

        /// <summary>Shrinks a rectangle horizontally by the same amount on both sides.</summary>
        /// <param name="rect">The rectangle to shrink.</param>
        /// <param name="padding">How much is taken off each side.</param>
        /// <returns>The shrunk rectangle.</returns>
        public static Rect Inset(Rect rect, float padding)
            => new(rect.x + padding, rect.y, rect.width - padding * 2f, rect.height);

        /// <summary>Fills a rounded rectangle.</summary>
        /// <param name="rect">The area to fill.</param>
        /// <param name="color">The fill color, alpha included.</param>
        /// <param name="radius">The corner radius.</param>
        public static void DrawFill(Rect rect, Color color, float radius)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, NoAspect, color, NoBorder,
                radius);
        }

        /// <summary>Draws the outline of a rounded rectangle.</summary>
        /// <param name="rect">The area to outline.</param>
        /// <param name="color">The line color, alpha included.</param>
        /// <param name="radius">The corner radius.</param>
        /// <param name="width">The line thickness.</param>
        public static void DrawBorder(Rect rect, Color color, float radius, float width)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, NoAspect, color, width,
                radius);
        }

        /// <summary>Fills the given rectangle with the hairline color.</summary>
        /// <param name="line">The area of the line.</param>
        public static void DrawSeparator(Rect line)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.DrawTexture(line, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, NoAspect,
                CommandPaletteStyles.SeparatorColor(), NoBorder, NoRadius);
        }

        /// <summary>
        /// Draws a pill shaped button. Built by hand instead of with <see cref="GUI.Button"/> so
        /// the fill can react to the mouse hovering it and holding it down.
        /// </summary>
        /// <param name="rect">The area of the button.</param>
        /// <param name="content">The label and tooltip.</param>
        /// <param name="active">Whether the button is switched on.</param>
        /// <returns><c>true</c> when the button was clicked.</returns>
        public static bool DrawPill(Rect rect, GUIContent content, bool active)
        {
            int control = GUIUtility.GetControlID(FocusType.Passive, rect);
            Event current = Event.current;
            bool hover = rect.Contains(current.mousePosition);

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
                    DrawPillFace(rect, content, active, hover, GUIUtility.hotControl == control);
                    break;
            }

            return false;
        }

        private static void DrawPillFace(Rect rect, GUIContent content, bool active, bool hover, bool pressed)
        {
            DrawFill(rect, CommandPaletteStyles.PillColor(active, hover, pressed), CommandPaletteStyles.PillRadius);

            GUI.Label(rect, content, active
                ? CommandPaletteStyles.ChipLabel
                : CommandPaletteStyles.PillLabel);
        }

        /// <summary>Draws one "key does something" hint and returns where the next one starts.</summary>
        /// <param name="area">The row the hint is drawn in.</param>
        /// <param name="x">Left edge of the hint.</param>
        /// <param name="key">The key combination shown on the cap.</param>
        /// <param name="label">What the key does.</param>
        /// <returns>The left edge of the next hint.</returns>
        public static float DrawHint(Rect area, float x, string key, string label)
        {
            GUIContent cap = new(key);
            GUIContent text = new(label);

            float capWidth = CommandPaletteStyles.KeyCapLabel.CalcSize(cap).x + KeyCapPadding * 2f;
            float capHeight = area.height - KeyCapPadding;
            Rect capRect = new(x, area.y + (area.height - capHeight) * 0.5f, capWidth, capHeight);

            DrawFill(capRect, CommandPaletteStyles.KeyCapColor(), CommandPaletteStyles.PillRadius);
            GUI.Label(capRect, cap, CommandPaletteStyles.KeyCapLabel);

            float textWidth = CommandPaletteStyles.HintLabel.CalcSize(text).x;
            Rect textRect = new(capRect.xMax + LabelGap, area.y, textWidth, area.height);

            GUI.Label(textRect, text, CommandPaletteStyles.HintLabel);

            return textRect.xMax + HintGap;
        }
    }
}