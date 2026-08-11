using System;
using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Resolves condition members (bool or enum) referenced by name from conditional attributes.
    /// Serialized siblings are read from the SerializedProperty for immediate response; other members
    /// are read from the declaring object so conditions work inside nested serializable types.
    /// </summary>
    public static class ConditionEvaluator
    {
        /// <summary>
        /// Combines several bool members into one result. An empty member list resolves to true, so an
        /// attribute without arguments never hides or disables anything.
        /// </summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="mode">How the members are combined.</param>
        /// <param name="members">Names of the bool members to evaluate.</param>
        /// <returns>True when the combined condition holds.</returns>
        public static bool ResolveAll(in MemberContext context, EConditionMode mode, string[] members)
        {
            if (members == null || members.Length == 0)
                return true;

            foreach (string member in members)
            {
                bool value = ResolveBool(context, member);

                if (mode == EConditionMode.All && !value)
                    return false;

                if (mode == EConditionMode.Any && value)
                    return true;
            }

            // All-mode reaching this point means nothing failed; Any-mode means nothing succeeded.
            return mode == EConditionMode.All;
        }

        /// <summary>
        /// Resolves a bool member: a serialized bool sibling, a bool field, a bool property or a
        /// parameterless bool method. Returns true when the member cannot be resolved.
        /// </summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="member">Name of the bool member to evaluate.</param>
        /// <returns>The value of the member, or true when it cannot be resolved.</returns>
        public static bool ResolveBool(in MemberContext context, string member)
        {
            SerializedProperty property = context.FindSiblingProperty(member);
            if (property != null && property.propertyType == SerializedPropertyType.Boolean)
                return property.boolValue;

            Type type = context.DeclaringType;
            object owner = context.DeclaringObject;
            if (type == null || owner == null)
                return true;

            FieldInfo field = ReflectionCache.GetField(type, member);
            if (field != null && field.FieldType == typeof(bool))
                return (bool)field.GetValue(owner);

            PropertyInfo info = ReflectionCache.GetProperty(type, member);
            if (info != null && info.CanRead && info.PropertyType == typeof(bool))
                return (bool)info.GetValue(owner, null);

            MethodInfo method = ReflectionCache.GetMethod(type, member);
            if (method != null && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
                return (bool)method.Invoke(owner, null);

            return true;
        }

        /// <summary>Resolves the current value of an enum field or property, or null.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="member">Name of the enum member to evaluate.</param>
        /// <returns>The boxed enum value, or null when it cannot be resolved.</returns>
        public static object ResolveEnum(in MemberContext context, string member) => MemberValueResolver.TryResolve(
            context.DeclaringType, context.DeclaringObject, member,
            out object value)
            ? value
            : null;
    }
}