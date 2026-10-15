using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Base.DataHubPackage.Editor
{
    /// <summary>
    /// Which fields of a type Unity serializes, in the order the inspector shows them. Shared by the
    /// table columns and the list editing, so both agree on what a field is.
    /// </summary>
    internal static class DataSerializedFields
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;
        private const string UnityAssemblyPrefix = "UnityEngine";

        private static readonly Dictionary<Type, List<FieldInfo>> Cache = new();

        /// <summary>The serialized fields of <paramref name="type"/>, base class fields first.</summary>
        /// <param name="type">The type to read.</param>
        /// <returns>The fields. Cached, do not change the list.</returns>
        internal static IReadOnlyList<FieldInfo> Of(Type type)
        {
            if (Cache.TryGetValue(type, out List<FieldInfo> cached))
                return cached;

            List<Type> chain = new();

            for (Type current = type; current != null && !IsUnityType(current); current = current.BaseType)
                chain.Add(current);

            List<FieldInfo> fields = new();

            for (int i = chain.Count - 1; i >= 0; i--)
            {
                foreach (FieldInfo field in chain[i].GetFields(FieldFlags))
                {
                    if (IsSerialized(field))
                        fields.Add(field);
                }
            }

            Cache.Add(type, fields);

            return fields;
        }

        /// <summary>True for a type that comes from Unity itself, which is where the field walk stops.</summary>
        /// <param name="type">The type to test.</param>
        /// <returns>True for object and Unity types.</returns>
        internal static bool IsUnityType(Type type) => type == typeof(object)
            || type.Assembly.GetName().Name.StartsWith(UnityAssemblyPrefix, StringComparison.Ordinal);

        /// <summary>The element type of an array or list, or null for anything else.</summary>
        /// <param name="type">The field type.</param>
        /// <returns>The element type, or null.</returns>
        internal static Type ElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                ? type.GetGenericArguments()[0]
                : null;
        }

        private static bool IsSerialized(FieldInfo field)
        {
            if (field.IsInitOnly
                || field.IsLiteral
                || Attribute.IsDefined(field, typeof(NonSerializedAttribute))
                || Attribute.IsDefined(field, typeof(HideInInspector)))
                return false;

            return field.IsPublic
                || Attribute.IsDefined(field, typeof(SerializeField))
                || Attribute.IsDefined(field, typeof(SerializeReference));
        }
    }
}