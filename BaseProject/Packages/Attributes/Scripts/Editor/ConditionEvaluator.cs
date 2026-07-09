using System;
using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Resolves condition members (bool or enum) referenced by name from conditional attributes.
    /// Serialized bools are read from the SerializedProperty for immediate response.
    /// </summary>
    public static class ConditionEvaluator
    {
        /// <summary>
        /// Resolves a bool member: a serialized bool field, a bool field, a bool property or a
        /// parameterless bool method. Returns true when the member cannot be resolved.
        /// </summary>
        public static bool ResolveBool(in MemberContext context, string member)
        {
            SerializedProperty property = context.Editor.serializedObject.FindProperty(member);
            if (property != null && property.propertyType == SerializedPropertyType.Boolean)
                return property.boolValue;

            Type type = context.Target.GetType();

            FieldInfo field = ReflectionCache.GetField(type, member);
            if (field != null && field.FieldType == typeof(bool))
                return (bool)field.GetValue(context.Target);

            PropertyInfo info = ReflectionCache.GetProperty(type, member);
            if (info != null && info.CanRead && info.PropertyType == typeof(bool))
                return (bool)info.GetValue(context.Target, null);

            MethodInfo method = ReflectionCache.GetMethod(type, member);
            if (method != null && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
                return (bool)method.Invoke(context.Target, null);

            return true;
        }

        /// <summary>Resolves the current value of an enum field or property, or null.</summary>
        public static object ResolveEnum(in MemberContext context, string member)
        {
            Type type = context.Target.GetType();

            FieldInfo field = ReflectionCache.GetField(type, member);
            if (field != null)
                return field.GetValue(context.Target);

            PropertyInfo info = ReflectionCache.GetProperty(type, member);
            if (info != null && info.CanRead)
                return info.GetValue(context.Target, null);

            return null;
        }
    }
}