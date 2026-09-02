using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Troubleshoot.Checks
{
    /// <summary>
    /// Verifies that the auto-assign attributes can actually reach a component. They resolve through
    /// GetComponent, so a field that is not a component type never fills, and the same attribute on a
    /// ScriptableObject has no GameObject to search at all.
    /// </summary>
    internal sealed class AutoAssignCheck : IAttributeCheck
    {
        private const string GameObjectMessage =
            "GameObject is not a component type, so the lookup never returns anything. Use Transform instead.";

        private const string ScriptableObjectMessage =
            "A ScriptableObject has no GameObject to search, so the field is never filled.";

        private static readonly Type[] AutoAssignAttributes =
        {
            typeof(GetComponentAttribute),
            typeof(GetComponentInParentAttribute),
            typeof(ChildAttribute)
        };

        /// <inheritdoc/>
        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            bool isScriptableObject = typeof(ScriptableObject).IsAssignableFrom(type);

            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                Type fieldType = CheckedMembers.ElementType(field.FieldType);
                if (fieldType == null)
                    continue;

                foreach (Type attributeType in AutoAssignAttributes)
                {
                    if (field.GetCustomAttribute(attributeType) == null)
                        continue;

                    Verify(field, fieldType, attributeType, isScriptableObject, issues);
                }
            }
        }

        private static void Verify(FieldInfo field, Type fieldType, Type attributeType, bool isScriptableObject,
            List<AttributeIssue> issues)
        {
            if (isScriptableObject)
            {
                AttributeIssues.Error(issues, field, attributeType, ScriptableObjectMessage);
                return;
            }

            if (fieldType == typeof(GameObject))
            {
                AttributeIssues.Error(issues, field, attributeType, GameObjectMessage);
                return;
            }

            if (typeof(Component).IsAssignableFrom(fieldType) || fieldType.IsInterface)
                return;

            AttributeIssues.Error(issues, field, attributeType,
                $"{fieldType.Name} is not a component type, so the field is never filled.");
        }
    }
}