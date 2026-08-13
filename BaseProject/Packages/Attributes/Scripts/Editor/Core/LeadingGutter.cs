using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Decides which members need room in front of their label for a small control, and where that
    /// control goes. The pipeline only offers trailing widgets, so anything that belongs in front of the
    /// label is drawn over the field's own row after the fact, and needs a gutter to land in.
    /// </summary>
    internal static class LeadingGutter
    {
        /// <summary>Width of the gutter, matching one indent step.</summary>
        /// <summary>
        /// Width of the control drawn in the gutter.
        /// </summary>
        /// <summary>Width of the control drawn in the gutter.</summary>
        public const float Width = 8f;

        /// <summary>Indent steps reserved for a checkbox, which needs room for a gap after it.</summary>
        public const int ToggleSteps = 1;

        /// <summary>Indent step reserved for a foldout arrow, which sits directly against its label.</summary>
        public const int ArrowSteps = 1;

        /// <summary>
        /// One indent step, which is the whole width of the gutter and therefore where the label starts.
        /// </summary>
        public const float IndentStep = 15f;

        // None. The control starts exactly where Unity's left toggle starts its own box, so the two
        // share a left edge when they sit next to each other, and moving this to buy a wider gap was
        // buying it in the wrong place.
        private const float Overhang = 0f;

        /// <summary>Returns whether the member needs a gutter in front of its label.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>True when one indent step should be reserved.</returns>
        /// <summary>
        /// How many indent steps the field gives up so its gutter control has somewhere to sit.
        /// </summary>
        /// <remarks>
        /// One step either way. The step is fifteen pixels and the label starts at the end of it, so a
        /// control narrower than that leaves the difference as the gap before the text. An arrow uses
        /// the whole step, the way Unity draws its own; a checkbox is drawn smaller so there is air
        /// after it.
        /// </remarks>
        /// <param name="context">The member being drawn.</param>
        /// <returns>The number of steps, or zero when the field needs no gutter.</returns>
        public static int StepsFor(in MemberContext context)
        {
            if (context.GetAttribute<PrefixToggleAttribute>() != null)
                return ToggleSteps;

            return ExpandableState.NeedsArrow(context)
                ? ArrowSteps
                : 0;
        }

        /// <summary>
        /// Returns the rect of the gutter for a field row. Called from an after-field handler, where the
        /// ambient indent is the outer one and the field was drawn one step further in.
        /// </summary>
        /// <param name="row">The row the field occupied.</param>
        /// <param name="indentLevel">The ambient indent level.</param>
        /// <param name="height">Height of the control.</param>
        /// <returns>The rect the leading control should fill.</returns>
        public static Rect RectFor(Rect row, int indentLevel, float height)
            => new(row.x + Left(indentLevel), row.y, Width, height);

        /// <summary>
        /// A square rect for a control that has to look like one, centred on the row.
        /// </summary>
        /// <remarks>
        /// A checkbox drawn into a rect the height of a line is a tall thin box rather than a checkbox.
        /// Squaring it is also the only thing that buys a gap before the label: the gutter is one indent
        /// step, the label starts at the end of it and neither can move, so the gap is exactly the part
        /// of the step the box leaves unused.
        /// </remarks>
        /// <param name="row">The row the field occupies.</param>
        /// <param name="indentLevel">The indent the field is drawn at.</param>
        /// <param name="height">The height of the row, used to centre the square in it.</param>
        /// <returns>The square rect to draw the control in.</returns>
        public static Rect SquareFor(Rect row, int indentLevel, float height)
        {
            float size = Mathf.Min(Width, height);

            return new Rect(row.x + Left(indentLevel), row.y + (height - size) * 0.5f, size, size);
        }

        private static float Left(int indentLevel) => Mathf.Max(indentLevel * IndentStep - Overhang, 0f);
    }
}