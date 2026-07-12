using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// A single required object reference found to be null, including the nested path to it.
    /// </summary>
    public readonly struct MissingRequiredReference
    {
        /// <summary>The component that owns the reference, used as log context and ping target.</summary>
        public readonly MonoBehaviour Component;

        /// <summary>Dotted path from the component to the field, for example "level1.level2.required".</summary>
        public readonly string Path;

        /// <summary>The declared type of the missing reference.</summary>
        public readonly Type FieldType;

        /// <summary>Optional custom message from the attribute, or null.</summary>
        public readonly string Message;

        /// <summary>Creates a missing-reference record.</summary>
        public MissingRequiredReference(MonoBehaviour component, string path, Type fieldType, string message)
        {
            Component = component;
            Path = path;
            FieldType = fieldType;
            Message = message;
        }
    }
}