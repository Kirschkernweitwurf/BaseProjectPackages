using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Draws one number cell of a date or duration row: a number field with its unit letter printed
    /// inside the right end of the box, plus the small separator labels between cells. Both rows are
    /// laid out from explicit rectangles so the columns line up, which is why the caller slices and
    /// this only fills.
    /// </summary>
    /// <remarks>
    /// The letter sits inside the field rather than beside it, because a letter floating between the
    /// box and the separator binds to whichever it is nearer and reads as labelling the wrong number.
    /// Inside the box the border does the grouping, and the separator outside can only be read as
    /// standing between two of them.
    /// </remarks>
    public static class TimeUnitField
    {
        /// <summary>Horizontal gap between a row of cells and the button after it.</summary>
        public const float Gap = 4f;

        /// <summary>Width of a separator label such as the dash between year and month.</summary>
        public const float SeparatorWidth = 11f;

        /// <summary>Width reserved for the unit letter inside the right end of the field.</summary>
        public const float SuffixWidth = 22f;

        // A cell stretched across the whole inspector leaves the letter marooned at the far end of an
        // otherwise empty box. Past this width the row stops growing and the slack is left at the end.
        private const float MaxCellWidth = 62f;

        // What is left for the digits once the letter has taken its share, so a cell can never shrink
        // to where the two overlap.
        private const float MinNumberWidth = 26f;

        private const float SuffixRightInset = 3f;

        // Dimmed, so the letter reads as a unit printed on the field rather than as part of the value
        // typed into it.
        private static readonly Color SuffixText = new(0.55f, 0.55f, 0.58f);

        private static GUIStyle SeparatorStyle => _separatorStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 0, 0)
        };

        private static GUIStyle SuffixStyle => _suffixStyle ??= BuildSuffixStyle();

        private static GUIStyle _separatorStyle;
        private static GUIStyle _suffixStyle;

        /// <summary>
        /// Splits a row into equally wide cells, after taking off the separators and the trailing
        /// button. The width is capped, so a wide inspector leaves slack at the end of the row rather
        /// than stretching every box around the few digits it holds.
        /// </summary>
        /// <param name="row">The row the cells share.</param>
        /// <param name="cells">How many number cells the row holds.</param>
        /// <param name="buttonWidth">Width of the button after the last cell, or zero for none.</param>
        /// <returns>The width of a single cell.</returns>
        public static float CellWidth(Rect row, int cells, float buttonWidth)
        {
            float separators = (cells - 1) * SeparatorWidth;
            float trailing = buttonWidth > 0f
                ? buttonWidth + Gap
                : 0f;

            float available = (row.width - separators - trailing) / cells;

            return Mathf.Clamp(available, MinNumberWidth + SuffixWidth, MaxCellWidth);
        }

        /// <summary>Carves the next slice off a row and advances the cursor past it.</summary>
        /// <param name="row">The row being sliced.</param>
        /// <param name="x">The running left edge, advanced by the slice width.</param>
        /// <param name="width">The width of the slice.</param>
        /// <returns>The rectangle of the slice.</returns>
        public static Rect Slice(Rect row, ref float x, float width)
        {
            Rect slice = new(x, row.y, width, row.height);
            x += width;

            return slice;
        }

        /// <summary>Draws a number field with its unit letter and clamps what comes back.</summary>
        /// <param name="cell">The box the field and its unit letter share.</param>
        /// <param name="value">The value to show.</param>
        /// <param name="suffix">The unit letter printed inside the right end of the box.</param>
        /// <param name="min">Lowest accepted value.</param>
        /// <param name="max">Highest accepted value.</param>
        /// <returns>The clamped value the user left in the field.</returns>
        public static int Draw(Rect cell, int value, string suffix, int min, int max)
        {
            int edited = EditorGUI.IntField(cell, value);

            // Painted after the field and therefore over it. The digits are left aligned and the cell
            // is wide enough for the longest value each unit accepts, so the two never meet.
            Rect unit = new(cell.xMax - SuffixWidth, cell.y, SuffixWidth - SuffixRightInset, cell.height);

            GUI.Label(unit, suffix, SuffixStyle);

            return Mathf.Clamp(edited, min, max);
        }

        /// <summary>Draws the small label that sits between two cells.</summary>
        /// <param name="rect">The slice the label occupies.</param>
        /// <param name="text">The separator text.</param>
        public static void DrawSeparator(Rect rect, string text) => GUI.Label(rect, text, SeparatorStyle);

        private static GUIStyle BuildSuffixStyle()
        {
            GUIStyle style = new(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(0, 0, 0, 0)
            };

            // Every state, or the letter lights up white while the field it is printed on has focus.
            style.normal.textColor = SuffixText;
            style.hover.textColor = SuffixText;
            style.active.textColor = SuffixText;
            style.focused.textColor = SuffixText;

            return style;
        }
    }
}