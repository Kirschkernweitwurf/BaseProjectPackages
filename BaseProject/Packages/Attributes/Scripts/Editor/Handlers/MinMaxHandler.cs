using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Validation;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Clamps int and float fields marked with <see cref="MinMaxAttribute"/>.</summary>
    public sealed class MinMaxHandler : IAfterFieldHandler
    {
        public int Order => 10;

        public void AfterField(in MemberContext context)
        {
            MinMaxAttribute attribute = context.GetAttribute<MinMaxAttribute>();
            if (attribute == null)
                return;

            SerializedProperty property = context.Property;

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int min = Mathf.RoundToInt(attribute.Min);
                int max = Mathf.RoundToInt(attribute.Max);
                int clamped = Mathf.Clamp(property.intValue, min, max);
                if (clamped != property.intValue)
                    property.intValue = clamped;
            }
            else if (property.propertyType == SerializedPropertyType.Float)
            {
                float clamped = Mathf.Clamp(property.floatValue, attribute.Min, attribute.Max);
                if (!Mathf.Approximately(clamped, property.floatValue))
                    property.floatValue = clamped;
            }
        }
    }
}