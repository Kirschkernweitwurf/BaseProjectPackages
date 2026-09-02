using System;
using Base.AttributesPackage.Editor.Core.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>Auto-assigns a <see cref="GetComponentAttribute"/> field from the same GameObject.</summary>
    internal sealed class GetComponentHandler : IAfterFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 5;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<GetComponentAttribute>() == null)
                return;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            if (context.Property.objectReferenceValue != null)
                return;

            if (context.Editor.serializedObject.isEditingMultipleObjects)
                return;

            if (context.Target is not Component component)
                return;

            Type type = context.Field?.FieldType;
            if (type == null || !typeof(Component).IsAssignableFrom(type))
                return;

            Component found = component.GetComponent(type);
            if (found != null)
                context.Property.objectReferenceValue = found;
        }
    }
}