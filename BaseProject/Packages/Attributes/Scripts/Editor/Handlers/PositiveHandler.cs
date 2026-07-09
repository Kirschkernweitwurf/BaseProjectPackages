using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Validation;
using UnityEditor;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Clamps <see cref="PositiveAttribute"/> fields to zero or above.</summary>
    public sealed class PositiveHandler : IAfterFieldHandler
    {
        public int Order => 10;

        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<PositiveAttribute>() == null)
                return;

            SerializedProperty property = context.Property;

            if (property.propertyType == SerializedPropertyType.Integer && property.intValue < 0)
                property.intValue = 0;
            else if (property.propertyType == SerializedPropertyType.Float && property.floatValue < 0f)
                property.floatValue = 0f;
        }
    }
}