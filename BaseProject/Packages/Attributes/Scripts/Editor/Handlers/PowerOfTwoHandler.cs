using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Validation;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Snaps <see cref="PowerOfTwoAttribute"/> int fields to the nearest power of two.</summary>
    public sealed class PowerOfTwoHandler : IAfterFieldHandler
    {
        public int Order => 10;

        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<PowerOfTwoAttribute>() == null)
                return;

            SerializedProperty property = context.Property;
            if (property.propertyType != SerializedPropertyType.Integer)
                return;

            int value = property.intValue;
            int snapped = value < 1
                ? 1
                : Mathf.ClosestPowerOfTwo(value);

            if (snapped != value)
                property.intValue = snapped;
        }
    }
}