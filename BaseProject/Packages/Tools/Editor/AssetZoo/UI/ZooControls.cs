using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// The small controls the window draws itself.
    /// </summary>
    internal static class ZooControls
    {
        private const float SegmentHeight = 22f;
        private const float SegmentInset = 2f;
        private const float SegmentOuterRadius = 6f;
        private const float SegmentRadius = 4f;

        /// <summary>
        /// Draws a row of choices with one picked, for a setting with few options, where seeing all of them at once
        /// says more than a dropdown would.
        /// </summary>
        /// <param name="styles">The built styles.</param>
        /// <param name="label">The name of the setting.</param>
        /// <param name="choices">The choices. Each one's tooltip says what it does.</param>
        /// <param name="selected">The picked choice.</param>
        /// <returns>The picked choice after this pass.</returns>
        public static int Segmented(ZooWindowStyles styles, GUIContent label, IReadOnlyList<GUIContent> choices,
            int selected)
        {
            Rect area = EditorGUILayout.GetControlRect(true, SegmentHeight);

            // Drawn by hand, because a prefix label takes its look from whichever control has the keyboard focus,
            // and these have none. It left one label of the window dimmed at random.
            float labelWidth = EditorGUIUtility.labelWidth;
            GUI.Label(new Rect(area.x, area.y, labelWidth, area.height), label, EditorStyles.label);

            Rect field = new(area.x + labelWidth, area.y, area.width - labelWidth, area.height);

            DrawRounded(field, EditorPalette.Field, SegmentOuterRadius);

            float width = (field.width - SegmentInset * 2f) / choices.Count;

            for (int i = 0; i < choices.Count; i++)
            {
                Rect part = new(field.x + SegmentInset + width * i, field.y + SegmentInset, width,
                    field.height - SegmentInset * 2f);

                bool isPicked = i == selected;
                bool isHovered = part.Contains(Event.current.mousePosition);

                if (isPicked)
                    DrawRounded(part, EditorPalette.Secondary, SegmentRadius);
                else if (isHovered)
                    DrawRounded(part, EditorPalette.Hover, SegmentRadius);

                Color previous = GUI.contentColor;
                GUI.contentColor = isPicked
                    ? EditorPalette.Text
                    : EditorPalette.DimText;

                if (GUI.Button(part, choices[i], styles.Segment))
                    selected = i;

                GUI.contentColor = previous;
            }

            return selected;
        }

        /// <summary>Fills a rectangle with rounded corners.</summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="color">The fill.</param>
        /// <param name="radius">The corner radius.</param>
        public static void DrawRounded(Rect rect, Color color, float radius)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, radius);
        }

        /// <summary>Outlines a rectangle with rounded corners.</summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="color">The line color.</param>
        /// <param name="width">The line width.</param>
        /// <param name="radius">The corner radius.</param>
        public static void DrawRoundedOutline(Rect rect, Color color, float width, float radius)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, width, radius);
        }

        /// <summary>Draws text in any color with a style whose own color is white.</summary>
        /// <param name="rect">Where the text goes.</param>
        /// <param name="text">The text.</param>
        /// <param name="style">The style.</param>
        /// <param name="color">The color.</param>
        public static void DrawText(Rect rect, string text, GUIStyle style, Color color)
        {
            Color previous = GUI.contentColor;
            GUI.contentColor = color;
            GUI.Label(rect, text, style);
            GUI.contentColor = previous;
        }

        /// <summary>
        /// Draws a button that is only an icon and a hover plate, for the small actions on a row.
        /// </summary>
        /// <param name="rect">Where it goes.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="tooltip">What it does. An icon has no words, so this is required.</param>
        /// <returns>True in the pass it was clicked.</returns>
        public static bool IconButton(Rect rect, Texture icon, string tooltip)
        {
            if (rect.Contains(Event.current.mousePosition))
                DrawRounded(rect, EditorPalette.Hover, SegmentRadius);

            return GUI.Button(rect, new GUIContent(icon, tooltip), GUIStyle.none);
        }
    }
}