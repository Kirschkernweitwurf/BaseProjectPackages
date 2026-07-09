using System;
using System.Reflection;
using Base.AttributePackage.Editor.Inspectors;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Everything a handler needs about a single inspected member, passed by reference.
    /// </summary>
    public readonly struct MemberContext
    {
        /// <summary>The serialized property being drawn.</summary>
        public readonly SerializedProperty Property;

        /// <summary>The reflected field behind the property, or null if it could not be resolved.</summary>
        public readonly FieldInfo Field;

        /// <summary>The primary inspected object.</summary>
        public readonly Object Target;

        /// <summary>The active editor, for access to serializedObject, targets and Repaint.</summary>
        public readonly AttributePackageEditor Editor;

        /// <summary>The object reference value captured before the field was drawn this frame.</summary>
        public readonly Object ObjectReferenceBefore;

        /// <summary>Creates a context for a single member.</summary>
        public MemberContext(SerializedProperty property,
            FieldInfo field,
            Object target,
            AttributePackageEditor editor,
            Object objectReferenceBefore)
        {
            Property = property;
            Field = field;
            Target = target;
            Editor = editor;
            ObjectReferenceBefore = objectReferenceBefore;
        }

        /// <summary>Returns the field attribute of the given type, or null.</summary>
        public T GetAttribute<T>() where T : Attribute => Field?.GetCustomAttribute<T>();

        /// <summary>Human-readable label derived from the property name.</summary>
        public string DisplayName => ObjectNames.NicifyVariableName(Property.name);
    }
}