using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Core
{
    /// <summary>
    /// Draws the small action buttons used by the copy, clear and open widgets into their reserved
    /// rect at the right edge of the field.
    /// </summary>
    internal static class FieldButtonRenderer
    {
        private const int FontSize = 10;
        private const int HorizontalPadding = 2;

        private static GUIStyle Style
        {
            get
            {
                EnsureFresh();

                return _style ??= new GUIStyle(EditorStyles.miniButton)
                {
                    padding = EditorStyleUtility.HorizontalPadding(HorizontalPadding),
                    fontSize = FontSize
                };
            }
        }

        private static readonly EditorStyleWatch Watch = new();

        private static GUIStyle _style;

        /// <summary>Draws a button at a fixed rect.</summary>
        /// <param name="rect">Where to draw it.</param>
        /// <param name="content">Label and tooltip of the button.</param>
        /// <returns>True on click.</returns>
        internal static bool DrawAt(Rect rect, GUIContent content) => GUI.Button(rect, content, Style);

        // A GUIStyle copies its colors out of EditorStyles when it is built and does not stay
        // linked to them, so a cached one keeps the previous theme's colors after a switch.
        // Dropping it here has the next access rebuild it against the theme actually in use.
        private static void EnsureFresh()
        {
            if (!Watch.IsStale)
                return;

            _style = null;

            Watch.MarkFresh();
        }
    }
}