using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Answers which bool members are driven by a <see cref="PrefixToggleAttribute"/> somewhere on the
    /// same type, so those members can be hidden from their own row. Cached per type, since the answer
    /// is compile-time metadata and the inspector asks it on every repaint.
    /// </summary>
    internal static class PrefixToggleState
    {
        private const BindingFlags FieldFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<Type, HashSet<string>> Driven = new();

        /// <summary>Returns whether the given member is drawn as another field's prefix toggle.</summary>
        /// <param name="declaringType">The type that declares the member.</param>
        /// <param name="member">Name of the member.</param>
        /// <returns>True when the member has no row of its own.</returns>
        public static bool IsDrivenBySomeone(Type declaringType, string member)
            => declaringType != null && DrivenOn(declaringType).Contains(member);

        /// <summary>Resolves the serialized bool a prefix toggle writes to, or null.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>The bool property, or null when it cannot be used.</returns>
        public static SerializedProperty ResolveToggle(in MemberContext context)
        {
            PrefixToggleAttribute attribute = context.GetAttribute<PrefixToggleAttribute>();
            if (attribute == null)
                return null;

            SerializedProperty toggle = context.FindSiblingProperty(attribute.Member);

            // Only a serialized bool works, because the checkbox has to write back to it. A bool
            // property or method can be read but not assigned.
            return toggle != null && toggle.propertyType == SerializedPropertyType.Boolean
                ? toggle
                : null;
        }

        private static HashSet<string> DrivenOn(Type type)
        {
            if (Driven.TryGetValue(type, out HashSet<string> cached))
                return cached;

            HashSet<string> members = new();

            foreach (FieldInfo field in type.GetFields(FieldFlags))
            {
                PrefixToggleAttribute attribute = field.GetCustomAttribute<PrefixToggleAttribute>();

                if (attribute != null && !string.IsNullOrEmpty(attribute.Member))
                    members.Add(attribute.Member);
            }

            Driven[type] = members;
            return members;
        }
    }
}