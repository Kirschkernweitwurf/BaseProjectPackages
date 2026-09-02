using Base.AttributesPackage.Editor.Core.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>Snaps <see cref="PowerOfTwoAttribute"/> int fields to the nearest power of two.</summary>
    internal sealed class PowerOfTwoHandler : IAfterFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 10;

        /// <inheritdoc/>
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