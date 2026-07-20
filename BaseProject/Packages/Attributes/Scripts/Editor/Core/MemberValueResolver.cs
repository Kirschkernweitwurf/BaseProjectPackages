using System;
using System.Reflection;

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
    }
}