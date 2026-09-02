using Base.AttributesPackage.Editor.Core.Interfaces;
using UnityEditor;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>Trims <see cref="MaxLengthAttribute"/> string fields to the allowed length.</summary>
    internal sealed class MaxLengthHandler : IAfterFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 10;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            MaxLengthAttribute attribute = context.GetAttribute<MaxLengthAttribute>();
            if (attribute == null)
                return;

            SerializedProperty property = context.Property;
            if (property.propertyType != SerializedPropertyType.String)
                return;

            int max = attribute.Length < 0
                ? 0
                : attribute.Length;

            string value = property.stringValue;
            if (value != null && value.Length > max)
                property.stringValue = value.Substring(0, max);
        }
    }
}