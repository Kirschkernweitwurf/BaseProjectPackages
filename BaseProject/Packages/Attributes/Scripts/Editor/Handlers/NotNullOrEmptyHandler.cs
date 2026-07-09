using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Validation;
using UnityEditor;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Shows an error when a <see cref="NotNullOrEmptyAttribute"/> value is null or empty.</summary>
    public sealed class NotNullOrEmptyHandler : IAfterFieldHandler
    {
        public int Order => 20;

        public void AfterField(in MemberContext context)
        {
            NotNullOrEmptyAttribute attribute = context.GetAttribute<NotNullOrEmptyAttribute>();
            if (attribute == null)
                return;

            if (IsNullOrEmpty(context.Property))
                EditorGUILayout.HelpBox(attribute.Message ?? context.DisplayName + " must not be empty.",
                    MessageType.Error);
        }

        private static bool IsNullOrEmpty(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
                return string.IsNullOrEmpty(property.stringValue);

            if (property.isArray)
                return property.arraySize == 0;

            return false;
        }
    }
}