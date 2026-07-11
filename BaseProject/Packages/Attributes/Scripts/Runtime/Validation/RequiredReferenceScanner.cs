using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Validation
{
    /// <summary>
    /// Finds every <see cref="RequiredAttribute"/> object reference on a component that is null, at any
    /// depth. Descends into nested serializable types and into arrays and lists of them, so required
    /// fields inside nested classes and structs are checked too. Shared by the runtime validator and
    /// the editor overview window.
    /// </summary>
    public static class RequiredReferenceScanner
    {
        private const int MaxDepth = 10;
        private const string PathSeparator = ".";

        private const BindingFlags Flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static readonly Dictionary<Type, FieldInfo[]> SerializedFields = new();

        /// <summary>Appends every missing required reference on the given component to the results list.</summary>
        public static void Collect(MonoBehaviour behaviour, List<MissingRequiredReference> results)
        {
            if (behaviour == null)
                return;

            Scan(behaviour.GetType(), behaviour, behaviour, null, 0, results);
        }

        private static void Scan(Type type, object instance, MonoBehaviour component, string prefix, int depth,
            List<MissingRequiredReference> results)
        {
            if (instance == null || depth > MaxDepth)
                return;

            foreach (FieldInfo field in GetSerializedFields(type))
            {
                Type fieldType = field.FieldType;

                if (typeof(Object).IsAssignableFrom(fieldType))
                {
                    if (field.IsDefined(typeof(RequiredAttribute), true) && field.GetValue(instance) as Object == null)
                        results.Add(new MissingRequiredReference(component, Combine(prefix, field.Name), fieldType,
                            MessageOf(field)));

                    continue;
                }

                if (IsNestedSerializable(fieldType))
                {
                    object value = field.GetValue(instance);
                    Scan(value?.GetType() ?? fieldType, value, component, Combine(prefix, field.Name), depth + 1,
                        results);
                    continue;
                }

                if (IsSerializableCollection(fieldType, out Type element) && field.GetValue(instance) is IList list)
                    ScanList(list, element, component, Combine(prefix, field.Name), depth, results);
            }
        }

        private static void ScanList(IList list, Type element, MonoBehaviour component, string basePath, int depth,
            List<MissingRequiredReference> results)
        {
            for (int i = 0; i < list.Count; i++)
            {
                object item = list[i];
                Scan(item?.GetType() ?? element, item, component, $"{basePath}[{i}]", depth + 1, results);
            }
        }

        private static FieldInfo[] GetSerializedFields(Type type)
        {
            if (SerializedFields.TryGetValue(type, out FieldInfo[] cached))
                return cached;

            List<FieldInfo> fields = new();
            Type current = type;

            while (current != null && current != typeof(object) && !IsFrameworkType(current))
            {
                foreach (FieldInfo field in current.GetFields(Flags))
                {
                    if (field.IsInitOnly || Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
                        continue;

                    if (field.IsPublic || Attribute.IsDefined(field, typeof(SerializeField)))
                        fields.Add(field);
                }

                current = current.BaseType;
            }

            FieldInfo[] result = fields.ToArray();
            SerializedFields[type] = result;
            return result;
        }

        private static bool IsNestedSerializable(Type type)
            => !type.IsPrimitive
                && type != typeof(string)
                && !type.IsEnum
                && !typeof(Object).IsAssignableFrom(type)
                && !IsFrameworkType(type)
                && Attribute.IsDefined(type, typeof(SerializableAttribute));

        private static bool IsSerializableCollection(Type type, out Type element)
        {
            element = null;

            if (type.IsArray)
                element = type.GetElementType();
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                element = type.GetGenericArguments()[0];
            else
                return false;

            return element != null && IsNestedSerializable(element);
        }

        private static bool IsFrameworkType(Type type)
        {
            string assembly = type.Assembly.GetName().Name;
            return assembly.StartsWith("Unity") || assembly.StartsWith("System") || assembly == "mscorlib";
        }

        private static string MessageOf(FieldInfo field) => field.GetCustomAttribute<RequiredAttribute>(true)?.Message;

        private static string Combine(string prefix, string name)
            => prefix == null
                ? name
                : prefix + PathSeparator + name;
    }
}
