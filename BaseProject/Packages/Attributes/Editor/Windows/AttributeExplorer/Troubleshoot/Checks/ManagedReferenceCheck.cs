using System;
using System.Collections.Generic;
using System.Reflection;
using Base.AttributesPackage.Editor.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Troubleshoot.Checks
{
    /// <summary>
    /// Verifies that polymorphic reference fields are set up so the picker can work. Without
    /// <c>[SerializeReference]</c> the field is not a managed reference and the picker draws a usage
    /// hint, and a declared type with no instantiable implementation gives an empty picker.
    /// </summary>
    internal sealed class ManagedReferenceCheck : IAttributeCheck
    {
        private const string MissingSerializeReference =
            "The field is not marked [SerializeReference], so there is no managed reference to pick a type for.";

        private const string UnityObjectMessage =
            "[SerializeReference] cannot store a UnityEngine.Object. Use a plain [SerializeField] instead.";

        /// <inheritdoc/>
        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                bool isManaged = field.GetCustomAttribute<SerializeReference>() != null;
                Type elementType = CheckedMembers.ElementType(field.FieldType);

                if (isManaged && elementType != null && typeof(Object).IsAssignableFrom(elementType))
                    AttributeIssues.Error(issues, field, typeof(SerializeReference), UnityObjectMessage);

                if (field.GetCustomAttribute<ReferencePickerAttribute>() == null)
                    continue;

                Verify(field, elementType, isManaged, issues);
            }
        }

        private static void Verify(FieldInfo field, Type elementType, bool isManaged, List<AttributeIssue> issues)
        {
            Type attributeType = typeof(ReferencePickerAttribute);

            if (!isManaged)
            {
                AttributeIssues.Error(issues, field, attributeType, MissingSerializeReference);
                return;
            }

            if (elementType == null)
                return;

            if (ManagedReferenceTypes.GetAssignable(elementType).Length == 0)
                AttributeIssues.Warning(issues, field, attributeType,
                    $"No instantiable type implements {elementType.Name}, so the picker stays empty. "
                    + "Candidates need a public parameterless constructor.");
        }
    }
}