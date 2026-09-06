using UnityEditor;
using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// The search box a Base window filters a list with: a rounded field in the theme's tone, a
    /// magnifier at the left, a dimmed prompt while it is empty and a clear button once it is not.
    /// </summary>
    /// <remarks>
    /// Unity's own <c>toolbarSearchField</c> carries a fixed height in the built-in skin, which it
    /// applies over whatever layout height it is given. That is why it never lined up with the button
    /// next to it, and it is why this is drawn from a rectangle rather than laid out: a caller that
    /// says how tall the row is gets a field that height.
    /// <para>
    /// The prompt is placed after the magnifier rather than at the left edge of the field, which is
    /// the other half of what was wrong with drawing a label over Unity's field.
    /// </para>
    /// </remarks>
    public static class EditorSearchField
    {
        private const string ClearLabel = "x";
        private const string ClearTooltip = "Clear the search";
        private const float ClearWidth = 16f;
        private const float IconGap = 4f;
        private const string IconName = "Search Icon";
        private const float IconSize = 14f;
        private const float SideInset = 5f;

        /// <summary>
        /// Draws the search box and returns what it now holds.
        /// </summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="rect">The rectangle to fill. The field takes its full height.</param>
        /// <param name="text">The current search text.</param>
        /// <param name="prompt">The line shown while the box is empty.</param>
        /// <returns>The search text after this pass.</returns>
        public static string Draw(EditorWindowStyles styles, Rect rect, string text, string prompt)
        {
            if (styles == null)
                return text;

            bool hasText = !string.IsNullOrEmpty(text);

            Rect icon = new(rect.x + SideInset, rect.y + (rect.height - IconSize) * 0.5f, IconSize,
                IconSize);

            float right = hasText
                ? rect.xMax - SideInset - ClearWidth
                : rect.xMax - SideInset;

            Rect field = new(icon.xMax + IconGap, rect.y, Mathf.Max(0f, right - icon.xMax - IconGap),
                rect.height);

            DrawChrome(styles, rect, icon);

            string edited = EditorGUI.TextField(field, text, styles.SearchText);

            if (!hasText && Event.current.type == EventType.Repaint)
                GUI.Label(field, prompt, styles.Detail);

            if (!hasText)
                return edited;

            Rect clear = new(right, rect.y, ClearWidth, rect.height);

            if (!GUI.Button(clear, new GUIContent(ClearLabel, ClearTooltip), styles.Ping))
                return edited;

            // The caret stays in a field that is being emptied under it, and keeps the old text alive
            // for one more pass, so the focus is dropped along with the text.
            GUIUtility.keyboardControl = 0;

            return string.Empty;
        }

        private static void DrawChrome(EditorWindowStyles styles, Rect rect, Rect icon)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            styles.SearchField.Draw(rect, false, false, false, false);

            Texture magnifier = EditorIcons.Named(IconName);

            if (magnifier != null)
                GUI.DrawTexture(icon, magnifier, ScaleMode.ScaleToFit);
        }
    }
}