using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Decides which members need room in front of their label for a small control, and where that
    /// control goes. The pipeline only offers trailing widgets, so anything that belongs in front of the
    /// label is drawn over the field's own row after the fact, and needs a gutter to land in.
    /// </summary>
    public static class LeadingGutter
    {
        /// <summary>Width of the gutter, matching one indent step.</summary>
        public const float Width = 14f;

        private const float IndentStep = 15f;

        /// <summary>Returns whether the member needs a gutter in front of its label.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>True when one indent step should be reserved.</returns>
        public static bool IsNeeded(in MemberContext context)
            => ExpandableState.NeedsArrow(context) || context.GetAttribute<PrefixToggleAttribute>() != null;

        /// <summary>
        /// Returns the rect of the gutter for a field row. Called from an after-field handler, where the
        /// ambient indent is the outer one and the field was drawn one step further in.
        /// </summary>
        /// <param name="row">The row the field occupied.</param>
        /// <param name="indentLevel">The ambient indent level.</param>
        /// <param name="height">Height of the control.</param>
        /// <returns>The rect the leading control should fill.</returns>
        public static Rect RectFor(Rect row, int indentLevel, float height)
            => new(row.x + indentLevel * IndentStep, row.y, Width, height);
    }
}
