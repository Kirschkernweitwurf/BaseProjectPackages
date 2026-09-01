using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The layout blocks that give every Base editor window the same shape: a title with a sentence
    /// under it, sections behind their own headers, content inside a rounded card, a primary action,
    /// and a status line at the foot.
    /// </summary>
    /// <remarks>
    /// A window that only has a couple of toggles and a button gets the most out of this, because
    /// without it there is nothing to tell it apart from any other editor window in the project.
    /// <para>
    /// Everything here is laid out with <see cref="EditorGUILayout"/>, so it composes with whatever
    /// the window already draws rather than asking it to be rewritten around explicit rectangles.
    /// </para>
    /// </remarks>
    public static class EditorWindowChrome
    {
        private const float CardContentInset = 8f;

        /// <summary>
        /// Draws the name of the window and, when there is one, the sentence that says what it does.
        /// </summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="title">The name shown at the top.</param>
        /// <param name="description">The sentence under it, or null for none.</param>
        public static void DrawHeader(EditorWindowStyles styles, string title, string description = null)
        {
            if (styles == null)
                return;

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            GUILayout.Label(title, styles.Title);

            if (!string.IsNullOrEmpty(description))
            {
                EditorGUILayout.Space(EditorMetrics.TightGap);
                GUILayout.Label(description, styles.Description);
            }

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            DrawSeparator();

            EditorGUILayout.Space(EditorMetrics.ItemGap);
        }

        /// <summary>
        /// Draws the sentence that says what a page is for, followed by a hairline.
        /// </summary>
        /// <remarks>
        /// For a project settings page, which already carries its name in the tree on the left and
        /// would read as saying it twice if it drew a title of its own.
        /// </remarks>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="description">The sentence shown at the top of the page.</param>
        public static void DrawIntro(EditorWindowStyles styles, string description)
        {
            if (styles == null || string.IsNullOrEmpty(description))
                return;

            GUILayout.Label(description, styles.Description);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            DrawSeparator();

            EditorGUILayout.Space(EditorMetrics.ItemGap);
        }

        /// <summary>
        /// Draws the header of one section of the window.
        /// </summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="label">The name of the section.</param>
        public static void DrawSectionHeader(EditorWindowStyles styles, string label)
        {
            if (styles == null)
                return;

            GUILayout.Label(label, styles.SectionHeader);
            EditorGUILayout.Space(EditorMetrics.TightGap);
        }

        /// <summary>Draws a hairline across the full width of the window.</summary>
        public static void DrawSeparator()
        {
            Rect line = GUILayoutUtility.GetRect(0f, EditorMetrics.SeparatorThickness, GUILayout.ExpandWidth(true));

            EditorRows.DrawSeparator(line);
        }

        /// <summary>
        /// Opens a rounded card that the following controls are drawn inside.
        /// </summary>
        /// <remarks>
        /// Always pair with <see cref="EndCard"/>. The card is a layout group, so an early return
        /// between the two leaves the GUI unbalanced for the rest of the pass.
        /// <para>
        /// For a window that owns its whole frame. A project settings page does not: the tree, the
        /// search bar and the pane it draws into all belong to Unity, so a themed panel in the middle
        /// of one reads as a patch over the editor rather than as part of it. Those pages use the
        /// headers and the intro from here and leave their fields on the editor's own background.
        /// </para>
        /// </remarks>
        /// <param name="styles">The built chrome styles.</param>
        public static void BeginCard(EditorWindowStyles styles)
        {
            EditorGUILayout.BeginVertical(styles == null
                ? GUIStyle.none
                : styles.Card);

            EditorGUILayout.Space(CardContentInset);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(CardContentInset, false);
            EditorGUILayout.BeginVertical();
        }

        /// <summary>Closes the card opened by <see cref="BeginCard"/>.</summary>
        public static void EndCard()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(CardContentInset, false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(CardContentInset);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the one action the window is mostly opened for.
        /// </summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="label">The button label.</param>
        /// <param name="options">Extra layout options, usually a height.</param>
        /// <returns>True when the button was pressed.</returns>
        public static bool PrimaryButton(EditorWindowStyles styles, string label, params GUILayoutOption[] options)
            => Button(styles == null
                ? null
                : styles.PrimaryButton, label, options);

        /// <summary>
        /// Draws an action next to the primary one.
        /// </summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="label">The button label.</param>
        /// <param name="options">Extra layout options, usually a width.</param>
        /// <returns>True when the button was pressed.</returns>
        public static bool SecondaryButton(EditorWindowStyles styles, string label, params GUILayoutOption[] options)
            => Button(styles == null
                ? null
                : styles.SecondaryButton, label, options);

        /// <summary>
        /// Draws the status line at the foot of the window, and nothing at all when there is no
        /// message to show.
        /// </summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="message">The message, or null or empty to draw nothing.</param>
        public static void DrawFooter(EditorWindowStyles styles, string message)
        {
            if (styles == null || string.IsNullOrEmpty(message))
                return;

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            DrawSeparator();

            EditorGUILayout.Space(EditorMetrics.TightGap);
            GUILayout.Label(message, styles.Footer);
        }

        /// <summary>
        /// Draws the centered block a window shows when it has nothing to list, which is the one
        /// place a Base window says the good news rather than leaving an empty rectangle.
        /// </summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="icon">The icon above the message, or null for none.</param>
        /// <param name="title">The headline.</param>
        /// <param name="hint">The explanation under it, or null for none.</param>
        public static void DrawEmptyState(EditorWindowStyles styles, Texture icon, string title, string hint = null)
        {
            if (styles == null)
                return;

            GUILayout.FlexibleSpace();

            if (icon != null)
                DrawCenteredIcon(icon);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            GUILayout.Label(title, styles.EmptyTitle);

            if (!string.IsNullOrEmpty(hint))
                GUILayout.Label(hint, styles.EmptyHint);

            GUILayout.FlexibleSpace();
        }

        private static bool Button(GUIStyle style, string label, params GUILayoutOption[] options)
        {
            // A style set that has not been built yet would draw an invisible button, so the plain
            // editor button is used until the first GUI pass has run.
            if (style == null)
                return GUILayout.Button(label, options);

            return GUILayout.Button(label, style, options);
        }

        private static void DrawCenteredIcon(Texture icon)
        {
            float size = EditorTableStyles.EmptyIconSize;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(new GUIContent(icon), GUILayout.Width(size), GUILayout.Height(size));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}