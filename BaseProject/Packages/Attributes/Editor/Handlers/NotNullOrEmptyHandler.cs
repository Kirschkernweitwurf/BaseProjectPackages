using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using UnityEditor;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Shows a compact error when a <see cref="NotNullOrEmptyAttribute"/> value is null or empty.</summary>
    internal sealed class NotNullOrEmptyHandler : IAfterFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 20;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            NotNullOrEmptyAttribute attribute = context.GetAttribute<NotNullOrEmptyAttribute>();
            if (attribute == null)
                return;

            if (!IsNullOrEmpty(context.Property))
                return;

            CompactHelpBox.Error(ValueResolver.Text(context, attribute.Message)
                ?? context.DisplayName + " " + NotNullOrEmptyAttribute.DefaultReason);
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