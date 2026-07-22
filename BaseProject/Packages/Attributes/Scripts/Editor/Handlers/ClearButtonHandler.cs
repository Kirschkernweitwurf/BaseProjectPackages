using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Clears a <see cref="ClearButtonAttribute"/> field. Object references reset to none and strings to
    /// empty. Disabled while already empty. Inline by default, or on its own row when inline is false.
    /// </summary>
    public sealed class ClearButtonHandler : IInlineFieldWidget, IAfterFieldHandler
    {
        private const float Width = 22f;

        int IInlineFieldWidget.Order => 30;

        int IAfterFieldHandler.Order => 91;

        private static readonly GUIContent Content = new("\u2715", "Clear the value.");

        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<ClearButtonAttribute>() is not
                {
                    Inline: false
                })
                return;

            SerializedProperty property = context.Property;
            if (!IsClearable(property))
                return;

            using (new EditorGUI.DisabledScope(!HasValue(property)))
            {
                if (FieldButtonRenderer.DrawRight(Content, Width))
                    Clear(property);
            }
        }

        public float GetWidth(in MemberContext context)
        {
            ClearButtonAttribute attribute = context.GetAttribute<ClearButtonAttribute>();
            return attribute is
                {
                    Inline: true
                }
                && IsClearable(context.Property)
                    ? Width
                    : 0f;
        }

        public void Draw(Rect rect, in MemberContext context)
        {
            SerializedProperty property = context.Property;
            using (new EditorGUI.DisabledScope(!HasValue(property)))
            {
                if (FieldButtonRenderer.DrawAt(rect, Content))
                    Clear(property);
            }
        }

        private static void Clear(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                property.objectReferenceValue = null;
            else if (property.propertyType == SerializedPropertyType.String)
                property.stringValue = string.Empty;
        }

        private static bool IsClearable(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.ObjectReference
                || property.propertyType == SerializedPropertyType.String;

        private static bool HasValue(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                return property.objectReferenceValue != null;

            return !string.IsNullOrEmpty(property.stringValue);
        }
    }
}