using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the small action buttons used by the copy, clear and open widgets into their reserved
    /// rect at the right edge of the field.
    /// </summary>
    internal static class FieldButtonRenderer
    {
        private const int FontSize = 10;
        private const int HorizontalPadding = 2;

        private static GUIStyle Style => _style ??= new GUIStyle(EditorStyles.miniButton)
        {
            padding = new RectOffset(HorizontalPadding, HorizontalPadding, 0, 0),
            fontSize = FontSize
        };

        private static GUIStyle _style;

        /// <summary>Draws a button at a fixed rect.</summary>
        /// <param name="rect">Where to draw it.</param>
        /// <param name="content">Label and tooltip of the button.</param>
        /// <returns>True on click.</returns>
        public static bool DrawAt(Rect rect, GUIContent content) => GUI.Button(rect, content, Style);
    }
}