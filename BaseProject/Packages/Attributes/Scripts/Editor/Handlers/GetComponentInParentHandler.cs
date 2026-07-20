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

            Object found = ParentComponentSearch.FindInParents(component.transform, type, attribute.Name,
                attribute.IncludeInactive);

            if (found != null)
                context.Property.objectReferenceValue = found;
        }
    }
}