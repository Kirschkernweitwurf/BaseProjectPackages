using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>Trims <see cref="MaxLengthAttribute"/> string fields to the allowed length.</summary>
    public sealed class MaxLengthHandler : IAfterFieldHandler
    {
        public int Order => 10;

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
