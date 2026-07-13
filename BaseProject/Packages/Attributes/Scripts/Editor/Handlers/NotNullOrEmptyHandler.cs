using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>Shows a compact error when a <see cref="NotNullOrEmptyAttribute"/> value is null or empty.</summary>
    public sealed class NotNullOrEmptyHandler : IAfterFieldHandler
    {
        public int Order => 20;

        public void AfterField(in MemberContext context)
        {
            NotNullOrEmptyAttribute attribute = context.GetAttribute<NotNullOrEmptyAttribute>();
            if (attribute == null)
                return;

            if (IsNullOrEmpty(context.Property))
                CompactHelpBox.Error(attribute.Message ?? context.DisplayName + " must not be empty");
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