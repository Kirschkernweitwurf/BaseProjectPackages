using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Every color, style and indentation the menu manager windows draw with. Kept in one place so
    /// the two windows cannot drift apart, and so a color is looked up rather than written twice.
    /// The styles are built on first use because <see cref="EditorStyles"/> is only valid inside a
    /// GUI call.
    /// </summary>
    internal static class MenuManagerTheme
    {
        /// <summary>Horizontal offset one nesting level adds to a row.</summary>
        public const float Indent = 14f;

        private const float GuideWidth = 1f;

        /// <summary>Bold text field used for a group name.</summary>
        public static GUIStyle Title => _title ??= new GUIStyle(EditorStyles.textField)
        {
            fontStyle = FontStyle.Bold
        };

        /// <summary>Centered label that draws the drag handle glyph.</summary>
        public static GUIStyle Grip => _grip ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14
        };

        /// <summary>Small bold label used by the column header.</summary>
        public static GUIStyle Column => _column ??= new GUIStyle(EditorStyles.miniBoldLabel);

        /// <summary>Box that follows the mouse while something is being dragged.</summary>
        public static GUIStyle Ghost => _ghost ??= new GUIStyle(EditorStyles.helpBox)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold
        };

        private static GUIStyle _column;
        private static GUIStyle _ghost;
        private static GUIStyle _grip;
        private static GUIStyle _title;

        /// <summary>Background of a group row.</summary>
        /// <param name="active">Whether the group is the one being dragged.</param>
        /// <returns>The fill color of the row.</returns>
        public static Color HeaderColor(bool active) => active
            ? new Color(0.23f, 0.36f, 0.55f, 0.6f)
            : EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.07f)
                : new Color(0f, 0f, 0f, 0.07f);

        /// <summary>Background of a section header row.</summary>
        public static Color SectionColor() => EditorGUIUtility.isProSkin
            ? new Color(0.35f, 0.45f, 0.6f, 0.25f)
            : new Color(0.35f, 0.45f, 0.6f, 0.18f);

        /// <summary>Tint that marks a row as read only.</summary>
        public static Color LockedColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.02f)
            : new Color(0f, 0f, 0f, 0.03f);

        /// <summary>Tint of every second entry row.</summary>
        public static Color RowStripeColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.03f)
            : new Color(0f, 0f, 0f, 0.03f);

        /// <summary>Line that stands for a separator in the built menu.</summary>
        public static Color SeparatorColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.45f)
            : new Color(0f, 0f, 0f, 0.45f);

        /// <summary>Faint line shown while hovering a spot where a separator can be added.</summary>
        public static Color SeparatorHintColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.14f)
            : new Color(0f, 0f, 0f, 0.14f);

        /// <summary>Tint of the row that is currently being dragged.</summary>
        public static Color SelectionColor() => new(0.23f, 0.55f, 0.95f, 0.15f);

        /// <summary>Tint of the entry an overview window linked to.</summary>
        public static Color FocusColor() => new(0.95f, 0.75f, 0.2f, 0.22f);

        /// <summary>Color of the line that marks the drop target.</summary>
        public static Color AccentColor() => new(0.23f, 0.55f, 0.95f, 0.9f);

        /// <summary>Tint behind a priority that was overridden by hand.</summary>
        public static Color OverrideColor() => new(0.95f, 0.75f, 0.2f, 0.18f);

        /// <summary>Color of the vertical lines that mark the nesting depth.</summary>
        public static Color GuideColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.1f)
            : new Color(0f, 0f, 0f, 0.12f);

        /// <summary>Draws one vertical guide per nesting level of a row.</summary>
        /// <param name="full">The whole row rect.</param>
        /// <param name="depth">Nesting level of the row.</param>
        public static void DrawGuides(Rect full, int depth)
        {
            for (int level = 1; level <= depth; level++)
            {
                float x = full.x + level * Indent - Indent * 0.5f;
                EditorGUI.DrawRect(new Rect(x, full.y, GuideWidth, full.height), GuideColor());
            }
        }

        /// <summary>Rect of a drop line at the given height, inset to match the nesting level.</summary>
        /// <param name="y">Height the line sits at.</param>
        /// <param name="row">Row the line belongs to.</param>
        /// <param name="depth">Nesting level the line is drawn for.</param>
        /// <returns>The rect to fill with <see cref="AccentColor"/>.</returns>
        public static Rect LineAt(float y, Rect row, int depth) => new(row.x + depth * Indent + 6f, y - 1f,
            row.width - depth * Indent - 12f, 2f);
    }
}