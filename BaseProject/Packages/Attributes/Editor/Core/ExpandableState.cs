using UnityEditor;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Open state of the inline inspectors drawn by <see cref="ExpandableAttribute"/>. Stored in
    /// EditorPrefs rather than on the property, so it survives selection changes and domain reloads and
    /// so the attribute's default can be honored the first time a field is seen.
    /// </summary>
    internal static class ExpandableState
    {
        /// <summary>Returns whether the inline inspector of the given member is open.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="attribute">The attribute driving the foldout.</param>
        /// <returns>True while the inline inspector should be drawn.</returns>
        internal static bool IsExpanded(in MemberContext context, ExpandableAttribute attribute)
            => EditorPrefs.GetBool(KeyFor(context), attribute.DefaultExpanded);

        /// <summary>Stores the open state of the given member.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="expanded">The new state.</param>
        internal static void SetExpanded(in MemberContext context, bool expanded)
            => EditorPrefs.SetBool(KeyFor(context), expanded);

        /// <summary>
        /// Returns whether the member needs room for a foldout arrow in front of its label, which is
        /// true once an expandable reference actually has something to expand.
        /// </summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>True when the arrow is drawn for this member.</returns>
        internal static bool NeedsArrow(in MemberContext context)
        {
            if (context.GetAttribute<ExpandableAttribute>() == null)
                return false;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return false;

            return context.Property.objectReferenceValue != null;
        }

        private static string KeyFor(in MemberContext context)
            => StateKey.For(context.Target.GetType(), context.Property.propertyPath);
    }
}