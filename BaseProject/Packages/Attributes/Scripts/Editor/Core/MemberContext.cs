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

        /// <summary>The primary inspected Unity object.</summary>
        public readonly Object Target;

        /// <summary>
        /// The type that declares <see cref="Field"/>. Equals the target type for top-level members and
        /// the nested serializable type when the pipeline descends into it.
        /// </summary>
        public readonly Type DeclaringType;

        /// <summary>
        /// The managed instance that owns <see cref="Field"/>. Equals <see cref="Target"/> for top-level
        /// members and the nested instance when the pipeline descends into it. Used for reflection-based
        /// conditions and validation.
        /// </summary>
        public readonly object DeclaringObject;

        /// <summary>The active editor, for access to serializedObject, targets and Repaint.</summary>
        public readonly AttributePackageEditor Editor;

        /// <summary>The object reference value captured before the field was drawn this frame.</summary>
        public readonly Object ObjectReferenceBefore;

        /// <summary>Creates a context for a single member.</summary>
        public MemberContext(SerializedProperty property,
            FieldInfo field,
            Object target,
            Type declaringType,
            object declaringObject,
            AttributePackageEditor editor,
            Object objectReferenceBefore)
        {
            Property = property;
            Field = field;
            Target = target;
            DeclaringType = declaringType;
            DeclaringObject = declaringObject;
            Editor = editor;
            ObjectReferenceBefore = objectReferenceBefore;
        }

        /// <summary>Returns the field attribute of the given type, or null. Cached per field.</summary>
        public T GetAttribute<T>() where T : Attribute => ReflectionCache.GetAttribute<T>(Field);

        /// <summary>Human-readable label derived from the property name.</summary>
        public string DisplayName => ObjectNames.NicifyVariableName(Property.name);

        /// <summary>
        /// Finds a sibling property by name, relative to this member's path. Resolves top-level members
        /// for top-level fields and members of the same nested object when descended.
        /// </summary>
        public SerializedProperty FindSiblingProperty(string member)
        {
            string path = Property.propertyPath;
            int separator = path.LastIndexOf('.');
            string siblingPath = separator < 0
                ? member
                : path.Substring(0, separator + 1) + member;

            return Editor.serializedObject.FindProperty(siblingPath);
        }
    }
}
