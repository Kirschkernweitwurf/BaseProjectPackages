using System;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Resolves the current value of a member referenced by name from attributes, checking fields
    /// first and readable properties second. Shared by drawers and condition evaluation, so member
    /// lookup behaves the same everywhere.
    /// </summary>
    public static class MemberValueResolver
    {
        /// <summary>
        /// Tries to read the value of a field or readable property with the given name. Returns false
        /// when no such member exists on the type.
        /// </summary>
        public static bool TryResolve(Type type, object owner, string member, out object value)
        {
            value = null;
            if (type == null || owner == null)
                return false;

            FieldInfo field = ReflectionCache.GetField(type, member);
            if (field != null)
            {
                value = field.GetValue(owner);
                return true;
            }

            PropertyInfo property = ReflectionCache.GetProperty(type, member);
            if (property == null || !property.CanRead)
                return false;

            value = property.GetValue(owner, null);
            return true;
        }

        /// <summary>
        /// Tries to read a field of the given type from the object that owns the property. Returns false
        /// when no field of that name and type exists, which is a setup mistake rather than a missing
        /// value. A found but unassigned field returns true with a null value.
        /// </summary>
        public static bool TryResolveSibling<T>(SerializedProperty property, string fieldName, out T value)
            where T : class
        {
            value = null;

            Object target = property.serializedObject.targetObject;
            if (target == null || string.IsNullOrEmpty(fieldName))
                return false;

            FieldInfo field = ReflectionCache.GetField(target.GetType(), fieldName);
            if (field == null || field.FieldType != typeof(T))
                return false;

            value = field.GetValue(target) as T;
            return true;
        }
    }
}