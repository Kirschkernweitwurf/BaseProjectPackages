using System;
using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.References;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Auto-assigns a <see cref="ChildAttribute"/> field from the children.</summary>
    public sealed class ChildHandler : IAfterFieldHandler
    {
        public int Order => 5;

        public void AfterField(in MemberContext context)
        {
            ChildAttribute attribute = context.GetAttribute<ChildAttribute>();
            if (attribute == null)
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
            if (type == null)
                return;

            Object found;
            if (!string.IsNullOrEmpty(attribute.Name))
                found = FindNamed(component.transform, attribute.Name, type, attribute.IncludeInactive);
            else if (typeof(Component).IsAssignableFrom(type))
                found = component.GetComponentInChildren(type, attribute.IncludeInactive);
            else
                found = null;

            if (found != null)
                context.Property.objectReferenceValue = found;
        }

        private static Object FindNamed(Transform root, string name, Type type, bool includeInactive)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(includeInactive))
            {
                if (child == root || child.name != name)
                    continue;

                if (type == typeof(Transform))
                    return child;

                if (type == typeof(GameObject))
                    return child.gameObject;

                Component component = child.GetComponent(type);
                if (component != null)
                    return component;
            }

            return null;
        }
    }
}