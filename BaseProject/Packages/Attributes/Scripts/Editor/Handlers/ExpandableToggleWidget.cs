using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the toggle that opens the inline inspector of an <see cref="ExpandableAttribute"/>
    /// reference. The state lives in EditorPrefs so it survives selection changes and domain reloads,
    /// and so the attribute's default expanded setting can be honored on first sight.
    /// </summary>
    public sealed class ExpandableToggleWidget : IInlineFieldWidget
    {
        private const float ButtonWidth = 22f;
        private const string CollapsedLabel = "\u25B6";
        private const string ExpandedLabel = "\u25BC";
        private const string Tooltip = "Edit this asset inline.";
        private const int WidgetOrder = 5;

        public int Order => WidgetOrder;

        public float GetWidth(in MemberContext context) => IsSupported(context)
            ? ButtonWidth
            : 0f;

        public void Draw(Rect rect, in MemberContext context)
        {
            ExpandableAttribute attribute = context.GetAttribute<ExpandableAttribute>();
            if (attribute == null)
                return;

            string key = KeyFor(context);
            bool expanded = EditorPrefs.GetBool(key, attribute.DefaultExpanded);

            GUIContent content = new(expanded
                ? ExpandedLabel
                : CollapsedLabel, Tooltip);

            if (GUI.Button(rect, content, EditorStyles.miniButton))
                EditorPrefs.SetBool(key, !expanded);
        }

        /// <summary>Returns whether the inline inspector of the given member is open.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="attribute">The attribute driving the toggle.</param>
        /// <returns>True while the inline inspector should be drawn.</returns>
        public static bool IsExpanded(in MemberContext context, ExpandableAttribute attribute)
            => EditorPrefs.GetBool(KeyFor(context), attribute.DefaultExpanded);

        private static string KeyFor(in MemberContext context)
            => StateKey.For(context.Target.GetType(), context.Property.propertyPath);

        // The toggle only makes sense once something is assigned, and only for asset references.
        private static bool IsSupported(in MemberContext context)
        {
            if (context.GetAttribute<ExpandableAttribute>() == null)
                return false;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return false;

            return context.Property.objectReferenceValue != null;
        }
    }
}