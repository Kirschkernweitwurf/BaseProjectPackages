using System;
using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Resolves condition members (bool or enum) referenced by name from conditional attributes.
    /// Serialized siblings are read from the SerializedProperty for immediate response; other members
    /// are read from the declaring object so conditions work inside nested serializable types.
    /// </summary>
    public static class ConditionEvaluator
    {
        /// <summary>
        /// Resolves a bool member: a serialized bool sibling, a bool field, a bool property or a
        /// parameterless bool method. Returns true when the member cannot be resolved.
        /// </summary>
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
        public static object ResolveEnum(in MemberContext context, string member)
        {
            Type type = context.DeclaringType;
            object owner = context.DeclaringObject;
            if (type == null || owner == null)
                return null;

            FieldInfo field = ReflectionCache.GetField(type, member);
            if (field != null)
                return field.GetValue(owner);

            PropertyInfo info = ReflectionCache.GetProperty(type, member);
            if (info != null && info.CanRead)
                return info.GetValue(owner, null);

            return null;
        }
    }
}
