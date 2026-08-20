using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Reserves the step of indent a foldout arrow is drawn in, and works out where in it the arrow goes.
    /// </summary>
    /// <remarks>
    /// A member with an arrow in front of its label gives up one indent step so the arrow has somewhere
    /// to sit, which is the room Unity gives any other foldout. The label then starts at the end of that
    /// step, so the arrow fills it.
    /// </remarks>
    internal static class LeadingGutter
    {
        /// <summary>Width of the arrow, which is the whole step.</summary>
        public const float Width = 13f;

        private const float IndentStep = 15f;

        /// <summary>
        /// How many indent steps the field gives up so its arrow has somewhere to sit.
        /// </summary>
        /// <param name="context">The member being drawn.</param>
        /// <returns>One step when the member has an arrow, otherwise zero.</returns>
        public static int StepsFor(in MemberContext context) => ExpandableState.NeedsArrow(context)
            ? 1
            : 0;

        /// <summary>The rect the arrow is drawn in.</summary>
        /// <param name="row">The row the field occupies.</param>
        /// <param name="indentLevel">The indent the field is drawn at.</param>
        /// <param name="height">The height of the row.</param>
        /// <returns>The rect to draw the arrow in.</returns>
        public static Rect RectFor(Rect row, int indentLevel, float height)
            => new(row.x + indentLevel * IndentStep, row.y, Width, height);
    }
}