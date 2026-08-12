using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples
{
    /// <summary>
    /// Walks the sample types and produces one entry per attribute they demonstrate.
    /// </summary>
    /// <remarks>
    /// Nothing is registered by hand. A field in a sample that carries a package attribute becomes an
    /// entry for it, so writing a sample is writing a field and the list follows. The first member to
    /// demonstrate an attribute wins, which keeps the list one row per attribute even when several
    /// samples happen to use the same one.
    /// </remarks>
    internal static class AttributeSampleRegistry
    {
        private const string AttributeSuffix = "Attribute";
        private const string PackageNamespace = "Base.AttributePackage";

        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static AttributeSampleEntry[] _entries;

        /// <summary>Returns every demonstrated attribute, grouped by category.</summary>
        /// <returns>The entries in list order.</returns>
        public static AttributeSampleEntry[] All()
        {
            if (_entries != null)
                return _entries;

            List<AttributeSampleEntry> entries = new();
            HashSet<string> seen = new();

            foreach (Type type in TypeCache.GetTypesWithAttribute<AttributeSampleAttribute>())
            {
                AttributeSampleAttribute sample =
                    (AttributeSampleAttribute)Attribute.GetCustomAttribute(type,
                        typeof(AttributeSampleAttribute));

                if (sample == null)
                    continue;

                Collect(type, sample, entries, seen);
            }

            entries.Sort(Compare);

            _entries = entries.ToArray();
            return _entries;
        }

        private static void Collect(Type type, AttributeSampleAttribute sample,
            List<AttributeSampleEntry> entries, HashSet<string> seen)
        {
            foreach (FieldInfo field in type.GetFields(MemberFlags))
                Add(field, field.Name, type, sample, entries, seen);

            foreach (MethodInfo method in type.GetMethods(MemberFlags))
                Add(method, method.Name, type, sample, entries, seen);
        }

        private static void Add(MemberInfo member, string name, Type type, AttributeSampleAttribute sample,
            List<AttributeSampleEntry> entries, HashSet<string> seen)
        {
            string description = DescriptionOf(member);

            foreach (Attribute attribute in member.GetCustomAttributes())
            {
                Type attributeType = attribute.GetType();

                if (attributeType.Namespace != PackageNamespace || !seen.Add(attributeType.Name))
                    continue;

                entries.Add(new AttributeSampleEntry(Display(attributeType), sample.Category, type, name,
                    description));
            }
        }

        // The tooltip doubles as the explanation, so a sample carries its documentation in the one place
        // that also shows it in the inspector.
        private static string DescriptionOf(MemberInfo member)
            => member.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? string.Empty;

        private static string Display(Type attributeType) => attributeType.Name.EndsWith(AttributeSuffix)
            ? attributeType.Name[..^AttributeSuffix.Length]
            : attributeType.Name;

        private static int Compare(AttributeSampleEntry left, AttributeSampleEntry right)
        {
            int category = string.CompareOrdinal(left.Category, right.Category);

            return category != 0
                ? category
                : string.CompareOrdinal(left.Title, right.Title);
        }
    }
}