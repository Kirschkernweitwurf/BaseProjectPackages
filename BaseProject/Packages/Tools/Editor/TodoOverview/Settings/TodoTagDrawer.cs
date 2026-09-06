using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.TodoOverview.Settings
{
    /// <summary>
    /// Draws one keyword in the settings page: the word, whether it is scanned for, its color, and a
    /// row of swatches underneath that sets the color in one click.
    /// </summary>
    /// <remarks>
    /// The color well stays, because a keyword may want a color no swatch offers. The swatches sit
    /// beside it for the far more common case of wanting one that simply reads well, which is hard to
    /// hit by eye in a color wheel and easy to miss by enough to make a chip hard to look at.
    /// </remarks>
    [CustomPropertyDrawer(typeof(TodoTag))]
    internal sealed class TodoTagDrawer : PropertyDrawer
    {
        private const float ColorWidth = 54f;
        private const float Gap = 4f;
        private const float SwatchHeight = 14f;
        private const float SwatchSpacing = 2f;
        private const float ToggleWidth = 16f;

        /// <summary>Draws the keyword row and the swatches under it.</summary>
        /// <param name="position">The area the row was given.</param>
        /// <param name="property">The tag being drawn.</param>
        /// <param name="label">The label the list gave this element.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty keyword = property.FindPropertyRelative(TodoTag.KeywordPropertyName);
            SerializedProperty color = property.FindPropertyRelative(TodoTag.ColorPropertyName);
            SerializedProperty enabled = property.FindPropertyRelative(TodoTag.EnabledPropertyName);

            if (keyword == null || color == null || enabled == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            float line = EditorGUIUtility.singleLineHeight;

            Rect toggle = new(position.xMax - ToggleWidth, position.y, ToggleWidth, line);
            Rect well = new(toggle.x - Gap - ColorWidth, position.y, ColorWidth, line);
            Rect word = new(position.x, position.y, Mathf.Max(0f, well.x - position.x - Gap), line);

            EditorGUI.PropertyField(word, keyword, GUIContent.none);
            EditorGUI.PropertyField(well, color, GUIContent.none);
            EditorGUI.PropertyField(toggle, enabled, GUIContent.none);

            DrawSwatches(new Rect(position.x, position.y + line + Gap, position.width, SwatchHeight), color);
        }

        /// <summary>Reports the height of the keyword row plus the swatches under it.</summary>
        /// <param name="property">The tag being measured.</param>
        /// <param name="label">The label the list gave this element.</param>
        /// <returns>The height the row needs.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight + Gap + SwatchHeight;

        private static void DrawSwatches(Rect area, SerializedProperty color)
        {
            IReadOnlyList<KeyValuePair<string, Color>> swatches = EditorSwatches.All();

            if (swatches.Count == 0 || area.width <= 0f)
                return;

            float width = (area.width - SwatchSpacing * (swatches.Count - 1)) / swatches.Count;

            if (width <= 0f)
                return;

            for (int index = 0; index < swatches.Count; index++)
            {
                Rect swatch = new(area.x + index * (width + SwatchSpacing), area.y, width, area.height);

                DrawSwatch(swatch, swatches[index], color);
            }
        }

        private static void DrawSwatch(Rect rect, KeyValuePair<string, Color> swatch,
            SerializedProperty color)
        {
            EditorGUI.DrawRect(rect, swatch.Value);

            // The one already in use is outlined rather than moved or marked with a glyph, so the row
            // stays a row of colors and the current pick can still be told apart at this size.
            if (Approximately(swatch.Value, color.colorValue))
                DrawOutline(rect, EditorPalette.Text);

            if (GUI.Button(rect, new GUIContent(string.Empty, swatch.Key), GUIStyle.none))
                color.colorValue = swatch.Value;
        }

        private static void DrawOutline(Rect rect, Color color)
        {
            float thickness = EditorMetrics.SeparatorThickness;

            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        // Compared channel by channel rather than with the equality operator, because a color that
        // made a round trip through the color well comes back a hair off the one it was set from.
        private static bool Approximately(Color left, Color right)
            => Mathf.Approximately(left.r, right.r)
                && Mathf.Approximately(left.g, right.g)
                && Mathf.Approximately(left.b, right.b);
    }
}