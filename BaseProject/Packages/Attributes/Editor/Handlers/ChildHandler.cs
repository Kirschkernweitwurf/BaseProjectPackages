using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Auto-assigns a <see cref="ChildAttribute"/> field from the children. Hierarchy scans for
    /// still-empty fields are throttled per field, so an unassignable field does not walk the whole
    /// child hierarchy on every repaint.
    /// </summary>
    internal sealed class ChildHandler : IAfterFieldHandler
    {
        private const double RetryInterval = 0.5;

        public int Order => 5;

        private static readonly Dictionary<string, double> NextAttempt = new();

        // Keyed per property, so the table grows with every field ever touched. Play mode is the point
        // at which none of it matters any more.
        static ChildHandler() => EditorApplication.playModeStateChanged += _ => NextAttempt.Clear();

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

            if (context.Target is not Component component)
                return;

            Type type = context.Field?.FieldType;
            if (type == null)
                return;

            if (!ShouldAttempt(context))
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

        private static bool ShouldAttempt(in MemberContext context)
        {
            string key = StateKey.For(context.Target.GetInstanceID(), context.Property.propertyPath);
            double now = EditorApplication.timeSinceStartup;

            if (NextAttempt.TryGetValue(key, out double next) && now < next)
                return false;

            NextAttempt[key] = now + RetryInterval;
            return true;
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