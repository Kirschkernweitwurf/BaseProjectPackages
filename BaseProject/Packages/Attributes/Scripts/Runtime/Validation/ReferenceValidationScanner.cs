using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Base.AttributePackage.Validation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage
{
    /// <summary>
    /// Runs every <see cref="IValidationRule"/> over the serialized fields of an object, at any depth.
    /// Descends into nested serializable types and into arrays and lists of them. Shared by the runtime
    /// validator and the editor overview window.
    /// </summary>
    public static class ReferenceValidationScanner
    {
        private const BindingFlags Flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private const int MaxDepth = 10;
        private const string PathSeparator = ".";

        private static readonly Dictionary<Type, FieldInfo[]> SerializedFields = new();

        /// <summary>Appends every validation issue found on the given object to the results list.</summary>
        public static void Collect(Object root, List<ReferenceIssue> results)
        {
            if (root == null)
                return;

            Scan(root.GetType(), root, root, null, 0, results);
        }

        private static void Scan(Type type, object instance, Object owner, string prefix, int depth,
            List<ReferenceIssue> results)
        {
            if (instance == null || depth > MaxDepth)
                return;

            foreach (FieldInfo field in GetSerializedFields(type))
            {
                string path = Combine(prefix, field.Name);

                foreach (IValidationRule rule in ValidationRules.All)
                {
                    if (rule.IsViolated(field, instance, out string reason))
                        results.Add(new ReferenceIssue(owner, path, field.FieldType, reason));
                }

                Type fieldType = field.FieldType;

                if (IsNestedSerializable(fieldType))
                {
                    object value = field.GetValue(instance);
                    Scan(value?.GetType() ?? fieldType, value, owner, path, depth + 1, results);
                    continue;
                }

                if (IsSerializableCollection(fieldType, out Type element) && field.GetValue(instance) is IList list)
                    ScanList(list, element, owner, path, depth, results);
            }
        }

        private static void ScanList(IList list, Type element, Object owner, string basePath, int depth,
            List<ReferenceIssue> results)
        {
            for (int i = 0; i < list.Count; i++)
            {
                object item = list[i];
                Scan(item?.GetType() ?? element, item, owner, $"{basePath}[{i}]", depth + 1, results);
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

        private static bool IsNestedSerializable(Type type) => !type.IsPrimitive
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

        private static string Combine(string prefix, string name) => prefix == null
            ? name
            : prefix + PathSeparator + name;
    }
}