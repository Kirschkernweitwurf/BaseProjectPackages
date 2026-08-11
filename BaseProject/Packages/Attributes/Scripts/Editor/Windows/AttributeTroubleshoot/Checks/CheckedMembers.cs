using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Checks
{
    /// <summary>
    /// Resolves members by name the same way the drawers and handlers do at runtime, so the window
    /// reports exactly what would fail rather than a stricter or looser approximation.
    /// </summary>
    public static class CheckedMembers
    {
        /// <summary>Returns whether a field, property or method with the given name exists.</summary>
        /// <param name="owner">The type the member is looked up on.</param>
        /// <param name="name">Name of the member.</param>
        /// <returns>True when any member with that name exists.</returns>
        public static bool Exists(Type owner, string name) => ReflectionCache.GetField(owner, name) != null
            || ReflectionCache.GetProperty(owner, name) != null
            || ReflectionCache.GetMethod(owner, name) != null;

        /// <summary>
        /// Returns whether the member resolves to a bool, matching what the condition evaluator accepts:
        /// a bool field, a readable bool property or a parameterless method returning bool.
        /// </summary>
        /// <param name="owner">The type the member is looked up on.</param>
        /// <param name="name">Name of the member.</param>
        /// <returns>True when the member yields a bool.</returns>
        public static bool IsBool(Type owner, string name)
        {
            FieldInfo field = ReflectionCache.GetField(owner, name);
            if (field != null)
                return field.FieldType == typeof(bool);

            PropertyInfo property = ReflectionCache.GetProperty(owner, name);
            if (property != null)
                return property.CanRead && property.PropertyType == typeof(bool);

            MethodInfo method = ReflectionCache.GetMethod(owner, name);
            return method != null && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0;
        }

        /// <summary>
        /// Returns the type a member yields: a field or property type, or a method return type. Returns
        /// null when no member of that name exists or the method takes parameters.
        /// </summary>
        /// <param name="owner">The type the member is looked up on.</param>
        /// <param name="name">Name of the member.</param>
        /// <returns>The value type, or null.</returns>
        public static Type ValueTypeOf(Type owner, string name)
        {
            FieldInfo field = ReflectionCache.GetField(owner, name);
            if (field != null)
                return field.FieldType;

            PropertyInfo property = ReflectionCache.GetProperty(owner, name);
            if (property != null && property.CanRead)
                return property.PropertyType;

            MethodInfo method = ReflectionCache.GetMethod(owner, name);
            return method != null && method.GetParameters().Length == 0
                ? method.ReturnType
                : null;
        }

        /// <summary>
        /// Returns whether a field of exactly the given type exists. The sibling resolver used by the
        /// picker drawers matches on the exact field type, so a subclass does not qualify.
        /// </summary>
        /// <param name="owner">The type the field is looked up on.</param>
        /// <param name="name">Name of the field.</param>
        /// <param name="exactType">The type the field has to have.</param>
        /// <returns>True when the field exists with that exact type.</returns>
        public static bool HasFieldOfExactType(Type owner, string name, Type exactType)
        {
            FieldInfo field = ReflectionCache.GetField(owner, name);
            return field != null && field.FieldType == exactType;
        }

        /// <summary>Returns whether the type is an enumerable that is not a string.</summary>
        /// <param name="type">The type to test.</param>
        /// <returns>True when the type can be iterated for dropdown options.</returns>
        public static bool IsEnumerable(Type type) => type != null
            && type != typeof(string)
            && typeof(IEnumerable).IsAssignableFrom(type);

        /// <summary>Returns whether the type is one of the numeric types the drawers handle.</summary>
        /// <param name="type">The type to test.</param>
        /// <returns>True for the integer and floating point types.</returns>
        public static bool IsNumeric(Type type) => type == typeof(int)
            || type == typeof(float)
            || type == typeof(double)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(byte);

        /// <summary>Returns whether the type is an array or a generic list.</summary>
        /// <param name="type">The type to test.</param>
        /// <returns>True when the field would serialize as an array.</returns>
        public static bool IsCollection(Type type) => type.IsArray
            || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

        /// <summary>Returns the element type of an array or list, or the type itself.</summary>
        /// <param name="type">The type to unwrap.</param>
        /// <returns>The element type.</returns>
        public static Type ElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                ? type.GetGenericArguments()[0]
                : type;
        }
    }
}