using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Every color, style and indentation the menu manager windows draw with. Kept in one place so
    /// the two windows cannot drift apart, and so a color is looked up rather than written twice.
    /// The styles are built on first use because <see cref="EditorStyles"/> is only valid inside a
    /// GUI call.
    /// <para>
    /// The shared editor look lives in <see cref="EditorPalette"/>; what stays here are the colors
    /// that only mean something in a menu tree, such as a separator row or an overridden priority.
    /// </para>
    /// </summary>
    internal static class MenuManagerTheme
    {
        private const float DropLineHeight = 2f;
        private const float DropLineInset = 6f;
        private const int GripFontSize = 14;

        /// <summary>Horizontal offset one nesting level adds to a row.</summary>
        internal static float Indent => EditorMetrics.Indent;

        /// <summary>Bold text field used for a group name.</summary>
        internal static GUIStyle Title
        {
            get
            {
                EnsureFresh();

                return _title ??= new GUIStyle(EditorStyles.textField)
                {
                    fontStyle = FontStyle.Bold
                };
            }
        }

        /// <summary>Centered label that draws the drag handle glyph.</summary>
        internal static GUIStyle Grip
        {
            get
            {
                EnsureFresh();

                return _grip ??= new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = GripFontSize
                };
            }
        }

        /// <summary>Small bold label used by the column header.</summary>
        internal static GUIStyle Column
        {
            get
            {
                EnsureFresh();

                return _column ??= new GUIStyle(EditorStyles.miniBoldLabel);
            }
        }

        /// <summary>Box that follows the mouse while something is being dragged.</summary>
        internal static GUIStyle Ghost
        {
            get
            {
                EnsureFresh();

                return _ghost ??= new GUIStyle(EditorStyles.helpBox)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        private static readonly EditorStyleWatch Watch = new();

        private static GUIStyle _column;
        private static GUIStyle _ghost;
        private static GUIStyle _grip;
        private static GUIStyle _title;

        /// <summary>Background of a group row.</summary>
        /// <param name="active">Whether the group is the one being dragged.</param>
        /// <returns>The fill color of the row.</returns>
        internal static Color HeaderColor(bool active) => active
            ? new Color(0.23f, 0.36f, 0.55f, 0.6f)
            : EditorPalette.Tint(0.07f);

        /// <summary>Background of a section header row.</summary>
        internal static Color SectionColor() => EditorPalette.Pick(new Color(0.35f, 0.45f, 0.6f, 0.25f),
            new Color(0.35f, 0.45f, 0.6f, 0.18f));

        /// <summary>Tint that marks a row as read only.</summary>
        internal static Color LockedColor() => EditorPalette.Tint(0.02f, 0.03f);

        /// <summary>Tint of every second entry row.</summary>
        internal static Color RowStripeColor() => EditorPalette.Stripe;

        /// <summary>Line that stands for a separator in the built menu.</summary>
        internal static Color SeparatorColor() => EditorPalette.Tint(0.45f);

        /// <summary>Faint line shown while hovering a spot where a separator can be added.</summary>
        internal static Color SeparatorHintColor() => EditorPalette.Tint(0.14f);

        /// <summary>Tint of the row that is currently being dragged.</summary>
        internal static Color SelectionColor() => EditorPalette.SelectionFill;

        /// <summary>Tint of the entry an overview window linked to.</summary>
        internal static Color FocusColor() => Fade(EditorPalette.Focus, 0.22f);

        /// <summary>Color of the line that marks the drop target.</summary>
        internal static Color AccentColor() => Fade(EditorPalette.Accent, 0.9f);

        /// <summary>Tint behind a priority that was overridden by hand.</summary>
        internal static Color OverrideColor() => Fade(EditorPalette.Focus, 0.18f);

        /// <summary>Color of the vertical lines that mark the nesting depth.</summary>
        internal static Color GuideColor() => EditorPalette.Tint(0.1f, 0.12f);

        /// <summary>Draws one vertical guide per nesting level of a row.</summary>
        /// <param name="full">The whole row rect.</param>
        /// <param name="depth">Nesting level of the row.</param>
        internal static void DrawGuides(Rect full, int depth) => EditorRows.DrawIndentGuides(full, depth, GuideColor());

        /// <summary>Rect of a drop line at the given height, inset to match the nesting level.</summary>
        /// <param name="y">Height the line sits at.</param>
        /// <param name="row">Row the line belongs to.</param>
        /// <param name="depth">Nesting level the line is drawn for.</param>
        /// <returns>The rect to fill with <see cref="AccentColor"/>.</returns>
        internal static Rect LineAt(float y, Rect row, int depth)
            => new(row.x + depth * Indent + DropLineInset, y - DropLineHeight * 0.5f,
                row.width - depth * Indent - DropLineInset * 2f, DropLineHeight);

        private static Color Fade(Color color, float alpha) => new(color.r, color.g, color.b, alpha);

        // A GUIStyle copies its colors out of EditorStyles when it is built and does not stay
        // linked to them, so a cached one keeps the previous theme's colors after a switch.
        // Dropping it here has the next access rebuild it against the theme actually in use.
        private static void EnsureFresh()
        {
            if (!Watch.IsStale)
                return;

            _column = null;
            _ghost = null;
            _grip = null;
            _title = null;

            Watch.MarkFresh();
        }
    }
}