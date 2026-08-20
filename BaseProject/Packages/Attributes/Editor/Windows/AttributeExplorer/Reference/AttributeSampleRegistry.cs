using System;
using System.Collections.Generic;
using System.Reflection;
using Base.AttributePackage.Samples;
using UnityEditor;

namespace Base.AttributePackage.Editor.Drawers.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// Collects the sample types and turns each one into the reference page for its attribute.
    /// </summary>
    /// <remarks>
    /// Nothing is registered by hand: a class carrying <see cref="AttributeSampleAttribute"/> becomes a
    /// page, so adding one is adding a file. The types are sorted before they are walked, because
    /// <see cref="TypeCache"/> promises no order and a duplicate would otherwise resolve differently
    /// from one domain reload to the next.
    /// </remarks>
    internal static class AttributeSampleRegistry
    {
        private const string AttributeSuffix = nameof(Attribute);

        private static AttributeSampleEntry[] _entries;

        /// <summary>Returns every sample, sorted by category and then by attribute name.</summary>
        /// <returns>The entries in list order.</returns>
        internal static AttributeSampleEntry[] All()
        {
            if (_entries != null)
                return _entries;

            List<Type> samples = new();

            foreach (Type type in TypeCache.GetTypesWithAttribute<AttributeSampleAttribute>())
                samples.Add(type);

            samples.Sort(comparison: static (left, right)
                => string.CompareOrdinal(left.FullName, right.FullName));

            List<AttributeSampleEntry> entries = new();
            HashSet<Type> seen = new();

            foreach (Type type in samples)
            {
                AttributeSampleAttribute sample = type.GetCustomAttribute<AttributeSampleAttribute>();

                if (sample?.AttributeType == null || !seen.Add(sample.AttributeType))
                    continue;

                entries.Add(new AttributeSampleEntry(Display(sample.AttributeType), sample.Category,
                    ObjectNames.NicifyVariableName(sample.Category.ToString()), type, sample.Description,
                    sample.Requirements, sample.Info, sample.Variations));
            }

            entries.Sort(Compare);

            _entries = entries.ToArray();

            return _entries;
        }

        /// <summary>Finds the entry with the given title.</summary>
        /// <param name="title">The title to look for.</param>
        /// <param name="entry">The matching entry, or the default when there is none.</param>
        /// <returns>True when an entry was found.</returns>
        internal static bool TryFind(string title, out AttributeSampleEntry entry)
        {
            foreach (AttributeSampleEntry candidate in All())
            {
                if (candidate.Title != title)
                    continue;

                entry = candidate;

                return true;
            }

            entry = default(AttributeSampleEntry);

            return false;
        }

        private static string Display(Type attributeType) => attributeType.Name.EndsWith(AttributeSuffix)
            ? attributeType.Name[..^AttributeSuffix.Length]
            : attributeType.Name;

        private static int Compare(AttributeSampleEntry left, AttributeSampleEntry right)
        {
            int category = left.Category.CompareTo(right.Category);

            return category != 0
                ? category
                : string.CompareOrdinal(left.Title, right.Title);
        }
    }
}