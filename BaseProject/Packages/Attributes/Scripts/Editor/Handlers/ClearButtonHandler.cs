using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Clears a <see cref="ClearButtonAttribute"/> field. Object references reset to none and strings to
    /// empty. Disabled while already empty. Inline by default, or on its own row when inline is false.
    /// </summary>
    public sealed class ClearButtonHandler : FieldButtonHandler
    {
        private const int AfterFieldOrder = 91;
        private const float ButtonWidth = 22f;
        private const int WidgetOrder = 30;

        private static readonly GUIContent Content = new("\u2715", "Clear the value.");

        protected override int InlineOrder => WidgetOrder;

        protected override int RowOrder => AfterFieldOrder;

        protected override float InlineWidth => ButtonWidth;

        protected override float RowWidth => ButtonWidth;

        protected override bool TryGetPlacement(in MemberContext context, out bool inline)
        {
            ClearButtonAttribute attribute = context.GetAttribute<ClearButtonAttribute>();
            inline = attribute != null && attribute.Inline;
            return attribute != null;
        }

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.ObjectReference
                || property.propertyType == SerializedPropertyType.String;

        protected override bool IsEnabled(in MemberContext context) => HasValue(context.Property);

        protected override GUIContent GetContent(in MemberContext context) => Content;

        protected override void Execute(in MemberContext context) => Clear(context.Property);

        private static void Clear(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                property.objectReferenceValue = null;
            else if (property.propertyType == SerializedPropertyType.String)
                property.stringValue = string.Empty;
        }

        private static bool HasValue(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                return property.objectReferenceValue != null;

            return !string.IsNullOrEmpty(property.stringValue);
        }
    }
}