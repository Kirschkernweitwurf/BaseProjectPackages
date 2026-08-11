using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>
    /// Everything a handle drawer needs about one field: the property to read and write, the object that
    /// owns it, and the transform the local space of the scene is measured against.
    /// </summary>
    public readonly struct HandleContext
    {
        /// <summary>The serialized property being visualized.</summary>
        public readonly SerializedProperty Property;

        /// <summary>The reflected field behind the property.</summary>
        public readonly FieldInfo Field;

        /// <summary>The inspected Unity object.</summary>
        public readonly Object Target;

        /// <summary>The transform local space is measured against, or null for an asset.</summary>
        public readonly Transform Transform;

        /// <summary>The type that declares <see cref="Field"/>.</summary>
        public readonly Type DeclaringType;

        /// <summary>The managed instance that owns <see cref="Field"/>.</summary>
        public readonly object DeclaringObject;

        /// <summary>Human-readable label derived from the property name.</summary>
        public string DisplayName => ObjectNames.NicifyVariableName(Property.name);

        /// <summary>Creates a context for one field.</summary>
        /// <param name="property">The serialized property being visualized.</param>
        /// <param name="field">The reflected field behind the property.</param>
        /// <param name="target">The inspected Unity object.</param>
        /// <param name="transform">The transform local space is measured against.</param>
        /// <param name="declaringType">The type that declares the field.</param>
        /// <param name="declaringObject">The managed instance that owns the field.</param>
        public HandleContext(SerializedProperty property, FieldInfo field, Object target, Transform transform,
            Type declaringType, object declaringObject)
        {
            Property = property;
            Field = field;
            Target = target;
            Transform = transform;
            DeclaringType = declaringType;
            DeclaringObject = declaringObject;
        }

        /// <summary>
        /// Resolves a sibling member holding a Vector3, used by the handles that let the caller move the
        /// gizmo off the transform. Returns false when the member is missing or of the wrong type.
        /// </summary>
        /// <param name="member">Name of the member to read.</param>
        /// <param name="value">The resolved vector.</param>
        /// <returns>True when the member resolved to a Vector3.</returns>
        public bool TryResolveVector(string member, out Vector3 value)
        {
            value = Vector3.zero;

            if (string.IsNullOrEmpty(member))
                return false;

            SerializedProperty sibling = FindSibling(member);
            if (sibling != null && sibling.propertyType == SerializedPropertyType.Vector3)
            {
                value = sibling.vector3Value;
                return true;
            }

            if (!MemberValueResolver.TryResolve(DeclaringType, DeclaringObject, member, out object resolved))
                return false;

            if (resolved is not Vector3 vector)
                return false;

            value = vector;
            return true;
        }

        /// <summary>Resolves a sibling member as display text, for the label handle.</summary>
        /// <param name="member">Name of the member to read.</param>
        /// <param name="text">The resolved text.</param>
        /// <returns>True when the member resolved to something printable.</returns>
        public bool TryResolveText(string member, out string text)
        {
            text = null;

            if (string.IsNullOrEmpty(member))
                return false;

            if (!MemberValueResolver.TryResolve(DeclaringType, DeclaringObject, member, out object resolved))
                return false;

            text = resolved?.ToString() ?? string.Empty;
            return true;
        }

        // Sibling lookup mirrors the inspector pipeline: relative to this property's path, so a field
        // inside a nested serializable type resolves against that type rather than the component.
        private SerializedProperty FindSibling(string member)
        {
            string path = Property.propertyPath;
            int separator = path.LastIndexOf('.');

            if (separator < 0)
                return Property.serializedObject.FindProperty(member);

            return Property.serializedObject.FindProperty($"{path[..separator]}.{member}");
        }
    }
}
