using System;
using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.References;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Auto-assigns a <see cref="FindComponentAttribute"/> field from the same GameObject.</summary>
    public sealed class FindComponentHandler : IAfterFieldHandler
    {
        public int Order => 5;

        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<FindComponentAttribute>() == null)
                return;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            if (context.Property.objectReferenceValue != null)
                return;

            if (context.Editor.serializedObject.isEditingMultipleObjects)
                return;

            if (!(context.Target is Component component))
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