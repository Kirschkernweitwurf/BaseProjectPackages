using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Caches field, method and property lookups per type. Cleared automatically on domain reload.
    /// </summary>
    public static class ReflectionCache
    {
        private const BindingFlags Flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> Fields = new();

        private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> Methods = new();

        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> Properties = new();

        private static readonly Dictionary<FieldInfo, Dictionary<Type, Attribute>> AttributesByField = new();

        private static readonly Func<Type, Dictionary<string, FieldInfo>> BuildFields =
            type => Build(type, select: declaring => declaring.GetFields(Flags));

        private static readonly Func<Type, Dictionary<string, MethodInfo>> BuildMethods =
            type => Build(type, select: declaring => declaring.GetMethods(Flags));

        private static readonly Func<Type, Dictionary<string, PropertyInfo>> BuildProperties =
            type => Build(type, select: declaring => declaring.GetProperties(Flags));

        /// <summary>
        /// Returns the field attribute of the given type, cached per field. Attributes are compile-time
        /// metadata, so this avoids repeated reflection during inspector repaints.
        /// </summary>
        public static T GetAttribute<T>(FieldInfo field) where T : Attribute
        {
            if (field == null)
                return null;

            if (!AttributesByField.TryGetValue(field, out Dictionary<Type, Attribute> map))
            {
                map = new Dictionary<Type, Attribute>();
                AttributesByField[field] = map;
            }

            if (!map.TryGetValue(typeof(T), out Attribute cached))
            {
                cached = field.GetCustomAttribute<T>();
                map[typeof(T)] = cached;
            }

            return (T)cached;
        }

        /// <summary>Returns the field with the given name anywhere in the type hierarchy, or null.</summary>
        public static FieldInfo GetField(Type type, string name)
            => GetMap(Fields, type, BuildFields).TryGetValue(name, out FieldInfo field)
                ? field
                : null;

        /// <summary>Returns the first method with the given name anywhere in the type hierarchy, or null.</summary>
        public static MethodInfo GetMethod(Type type, string name)
            => GetMap(Methods, type, BuildMethods).TryGetValue(name, out MethodInfo method)
                ? method
                : null;

        /// <summary>Returns the property with the given name anywhere in the type hierarchy, or null.</summary>
        public static PropertyInfo GetProperty(Type type, string name) => GetMap(Properties, type, BuildProperties)
            .TryGetValue(name, out PropertyInfo property)
            ? property
            : null;

        /// <summary>All fields declared across the type hierarchy.</summary>
        public static IEnumerable<FieldInfo> AllFields(Type type) => GetMap(Fields, type, BuildFields).Values;

        /// <summary>All properties declared across the type hierarchy.</summary>
        public static IEnumerable<PropertyInfo> AllProperties(Type type)
            => GetMap(Properties, type, BuildProperties).Values;

        private static Dictionary<string, T> GetMap<T>(Dictionary<Type, Dictionary<string, T>> cache, Type type,
            Func<Type, Dictionary<string, T>> build)
        {
            if (!cache.TryGetValue(type, out Dictionary<string, T> map))
            {
                map = build(type);
                cache[type] = map;
            }

            return map;
        }

        // Walks up the hierarchy so members declared on a base class are found too. The first
        // declaration wins, which is the most derived one.
        private static Dictionary<string, T> Build<T>(Type type, Func<Type, T[]> select) where T : MemberInfo
        {
            Dictionary<string, T> map = new();

            while (type != null)
            {
                foreach (T member in select(type))
                {
                    if (!map.ContainsKey(member.Name))
                        map[member.Name] = member;
                }

                type = type.BaseType;
            }

            return map;
        }
    }
}