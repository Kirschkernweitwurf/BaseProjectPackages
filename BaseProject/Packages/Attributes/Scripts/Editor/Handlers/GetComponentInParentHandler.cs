using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Auto-assigns a <see cref="GetComponentInParentAttribute"/> field from the ancestors. Search
    /// starts at the parent and walks upward, so the own GameObject is never used.
    /// </summary>
    public sealed class GetComponentInParentHandler : IAfterFieldHandler
    {
        public int Order => 5;

        public void AfterField(in MemberContext context)
        {
            GetComponentInParentAttribute attribute = context.GetAttribute<GetComponentInParentAttribute>();
            if (attribute == null)
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
            if (type == null)
                return;

            Object found = Search(component.transform, type, attribute.Name, attribute.IncludeInactive);
            if (found != null)
                context.Property.objectReferenceValue = found;
        }

        private static Object Search(Transform start, Type type, string name, bool includeInactive)
        {
            for (Transform current = start.parent; current != null; current = current.parent)
            {
                if (!includeInactive && !current.gameObject.activeInHierarchy)
                    continue;

                if (!string.IsNullOrEmpty(name) && current.name != name)
                    continue;

                Object match = Match(current, type);
                if (match != null)
                    return match;
            }

            return null;
        }

        private static Object Match(Transform current, Type type)
        {
            if (type == typeof(Transform))
                return current;

            if (type == typeof(GameObject))
                return current.gameObject;

            return current.GetComponent(type);
        }
    }
}
