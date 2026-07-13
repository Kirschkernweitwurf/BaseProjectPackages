using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the small action buttons used by the copy, clear and open widgets, both at a reserved
    /// inline rect and, for the non-inline overload, on their own thin row below the field.
    /// </summary>
    public static class FieldButtonRenderer
    {
        private static GUIStyle _style;

        /// <summary>Draws a button at a fixed rect. Returns true on click.</summary>
        public static bool DrawAt(Rect rect, GUIContent content) => GUI.Button(rect, content, Style);

        /// <summary>Draws a mini button right-aligned on its own thin row. Returns true on click.</summary>
        public static bool DrawRight(GUIContent content, float width)
        {
            Rect row = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect button = new(row.xMax - width, row.y, width, row.height);
            return GUI.Button(button, content, Style);
        }

        private static GUIStyle Style => _style ??= new GUIStyle(EditorStyles.miniButton)
        {
            padding = new RectOffset(2, 2, 0, 0),
            fontSize = 10
        };
    }
}
