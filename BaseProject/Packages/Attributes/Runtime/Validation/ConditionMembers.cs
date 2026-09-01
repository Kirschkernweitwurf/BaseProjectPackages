using System;
using System.Reflection;

namespace Base.AttributePackage
{
    /// <summary>
    /// Evaluates named bool members against a live instance through reflection. Used by validation
    /// rules, which run outside the inspector and therefore have no SerializedProperty to read from.
    /// The editor pipeline uses its own evaluator so serialized edits respond immediately.
    /// </summary>
    internal static class ConditionMembers
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        /// <summary>
        /// Returns whether the given members satisfy the mode. Members that cannot be resolved count as
        /// true, matching the editor pipeline, so a broken reference never silently suppresses a rule.
        /// </summary>
        /// <param name="instance">The object the members are read from.</param>
        /// <param name="mode">How the members are combined.</param>
        /// <param name="members">Names of the bool members to evaluate.</param>
        /// <returns>True when the combined condition holds.</returns>
        internal static bool Evaluate(object instance, EConditionMode mode, string[] members)
        {
            if (instance == null || members == null || members.Length == 0)
                return true;

            foreach (string member in members)
            {
                bool value = Resolve(instance, member);

                if (mode == EConditionMode.All && !value)
                    return false;

                if (mode == EConditionMode.Any && value)
                    return true;
            }

            return mode == EConditionMode.All;
        }

        private static bool Resolve(object instance, string member)
        {
            Type type = instance.GetType();

            FieldInfo field = type.GetField(member, MemberFlags);
            if (field != null && field.FieldType == typeof(bool))
                return (bool)field.GetValue(instance);

            PropertyInfo property = type.GetProperty(member, MemberFlags);
            if (property != null && property.CanRead && property.PropertyType == typeof(bool))
                return (bool)property.GetValue(instance, null);

            MethodInfo method = type.GetMethod(member, MemberFlags);
            if (method != null && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
                return (bool)method.Invoke(instance, null);

            return true;
        }
    }
}