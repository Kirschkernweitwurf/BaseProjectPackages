using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Clamps <see cref="MaxAttribute"/> fields to a maximum.</summary>
    public sealed class MaxHandler : IAfterFieldHandler
    {
        public int Order => 10;

        public void AfterField(in MemberContext context)
        {
            MaxAttribute attribute = context.GetAttribute<MaxAttribute>();
            if (attribute == null)
                return;

            SerializedProperty property = context.Property;
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int max = Mathf.RoundToInt(attribute.Max);
                if (property.intValue > max)
                    property.intValue = max;
            }
            else if (property.propertyType == SerializedPropertyType.Float && property.floatValue > attribute.Max)
            {
                property.floatValue = attribute.Max;
            }
        }
    }
}