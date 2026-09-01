using System;
using System.Collections.Generic;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Fills a <see cref="RequiredGetAttribute"/> field from the hierarchy and reports it when the
    /// search comes back empty.
    /// </summary>
    /// <remarks>
    /// The search only runs while the field is empty, so a reference that was deliberately pointed
    /// somewhere else is never overwritten.
    /// </remarks>
    internal sealed class RequiredGetHandler : IAfterFieldHandler
    {
        private const int HandlerOrder = 6;

        /// <inheritdoc/>
        public int Order => HandlerOrder;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            RequiredGetAttribute attribute = context.GetAttribute<RequiredGetAttribute>();
            if (attribute == null)
                return;

            if (context.Target is not Component component)
                return;

            if (context.Property.isArray && context.Property.propertyType != SerializedPropertyType.String)
                Fill(context, attribute, component);
            else
                FillSingle(context, attribute, component);

            Report(context, attribute);
        }

        private static void FillSingle(in MemberContext context, RequiredGetAttribute attribute,
            Component component)
        {
            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            if (context.Property.objectReferenceValue != null)
                return;

            Type type = context.Field?.FieldType;
            if (type == null || !IsSearchable(type))
                return;

            Component found = Search(component, type, attribute);

            if (found != null)
                context.Property.objectReferenceValue = found;
        }

        private static void Fill(in MemberContext context, RequiredGetAttribute attribute, Component component)
        {
            if (context.Property.arraySize > 0)
                return;

            Type type = ElementType(context.Field?.FieldType);
            if (type == null || !IsSearchable(type))
                return;

            List<Component> found = SearchAll(component, type, attribute);
            if (found.Count == 0)
                return;

            context.Property.arraySize = found.Count;

            for (int i = 0; i < found.Count; i++)
                context.Property.GetArrayElementAtIndex(i).objectReferenceValue = found[i];
        }

        private static void Report(in MemberContext context, RequiredGetAttribute attribute)
        {
            if (!IsEmpty(context.Property))
                return;

            CompactHelpBox.Error(ValueResolver.Text(context, attribute.Message)
                ?? context.DisplayName + " " + RequiredGetAttribute.DefaultReason);
        }

        private static bool IsEmpty(SerializedProperty property)
        {
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
                return property.arraySize == 0;

            return property.propertyType == SerializedPropertyType.ObjectReference
                && property.objectReferenceValue == null;
        }

        private static bool IsSearchable(Type type) => typeof(Component).IsAssignableFrom(type)
            || type.IsInterface;

        private static Type ElementType(Type type)
        {
            if (type == null)
                return null;

            if (type.IsArray)
                return type.GetElementType();

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                ? type.GetGenericArguments()[0]
                : null;
        }

        // Self is checked before the wider searches, so a component on this object always wins over an
        // equally valid one further away.
        private static Component Search(Component component, Type type, RequiredGetAttribute attribute)
        {
            if (attribute.IncludeSelf)
            {
                Component own = component.GetComponent(type);
                if (own != null)
                    return own;
            }

            if (attribute.InChildren)
            {
                Component child = component.GetComponentInChildren(type, attribute.IncludeInactive);
                if (child != null && (attribute.IncludeSelf || child.gameObject != component.gameObject))
                    return child;
            }

            if (!attribute.InParents)
                return null;

            Component parent = component.GetComponentInParent(type, attribute.IncludeInactive);

            return parent != null && (attribute.IncludeSelf || parent.gameObject != component.gameObject)
                ? parent
                : null;
        }

        private static List<Component> SearchAll(Component component, Type type, RequiredGetAttribute attribute)
        {
            List<Component> found = new();

            if (attribute.InChildren)
                found.AddRange(component.GetComponentsInChildren(type, attribute.IncludeInactive));
            else if (attribute.InParents)
                found.AddRange(component.GetComponentsInParent(type, attribute.IncludeInactive));
            else
                found.AddRange(component.GetComponents(type));

            if (attribute.IncludeSelf)
                return found;

            found.RemoveAll(candidate => candidate.gameObject == component.gameObject);
            return found;
        }
    }
}