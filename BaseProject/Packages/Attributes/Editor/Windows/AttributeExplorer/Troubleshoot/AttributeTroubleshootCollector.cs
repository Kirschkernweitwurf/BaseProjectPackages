using System;
using System.Collections.Generic;
using Base.AttributesPackage.Editor.Windows.AttributeExplorer.Troubleshoot.Samples;
using Base.UtilityPackage;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>
    /// Runs every discovered check over every type the attribute pipeline can reach: components,
    /// ScriptableObjects and the plain serializable types the pipeline descends into. Framework and
    /// Unity types are skipped, since their fields never carry these attributes, and so are the types
    /// marked <see cref="TroubleshootSampleAttribute"/>, which are broken on purpose.
    /// </summary>
    internal static class AttributeTroubleshootCollector
    {
        /// <summary>Scans the project and returns the findings grouped by declaring type.</summary>
        /// <param name="errors">Total number of findings that stop an attribute from working.</param>
        /// <param name="warnings">Total number of findings that only change behavior.</param>
        /// <returns>The groups, sorted by type name.</returns>
        internal static List<AttributeIssueGroup> CollectProject(out int errors, out int warnings)
            => Run(ProjectTypes(), out errors, out warnings);

        /// <summary>
        /// Scans only the deliberately broken sample types and returns their findings. Used by the
        /// samples tab so the window can be seen doing its job on a project that has nothing wrong.
        /// </summary>
        /// <param name="errors">Total number of findings that stop an attribute from working.</param>
        /// <param name="warnings">Total number of findings that only change behavior.</param>
        /// <returns>The groups, sorted by type name.</returns>
        internal static List<AttributeIssueGroup> CollectSamples(out int errors, out int warnings)
            => Run(SampleTypes(), out errors, out warnings);

        private static List<AttributeIssueGroup> Run(IEnumerable<Type> types, out int errors, out int warnings)
        {
            List<AttributeIssueGroup> groups = new();
            errors = 0;
            warnings = 0;

            foreach (Type type in types)
            {
                List<AttributeIssue> issues = new();

                foreach (IAttributeCheck check in AttributeCheckRegistry.Checks)
                    check.Inspect(type, issues);

                if (issues.Count == 0)
                    continue;

                AttributeIssueGroup group = new(type, issues);
                groups.Add(group);

                errors += group.ErrorCount;
                warnings += issues.Count - group.ErrorCount;
            }

            groups.Sort(comparison: (a, b) => string.CompareOrdinal(a.Type.FullName, b.Type.FullName));
            return groups;
        }

        private static HashSet<Type> ProjectTypes()
        {
            HashSet<Type> types = new();

            Add(types, TypeCache.GetTypesDerivedFrom<MonoBehaviour>());
            Add(types, TypeCache.GetTypesDerivedFrom<ScriptableObject>());
            Add(types, TypeCache.GetTypesWithAttribute<SerializableAttribute>());

            return types;
        }

        private static HashSet<Type> SampleTypes()
        {
            HashSet<Type> types = new();

            foreach (Type type in TypeCache.GetTypesWithAttribute<TroubleshootSampleAttribute>())
                types.Add(type);

            return types;
        }

        // The sample types are broken on purpose, so they are kept out of the project scan entirely
        // rather than reported and then ignored by the reader every single time.
        private static void Add(HashSet<Type> types, IEnumerable<Type> candidates)
        {
            foreach (Type candidate in candidates)
            {
                if (candidate.IsGenericTypeDefinition || FrameworkAssemblies.Contains(candidate))
                    continue;

                if (candidate.IsDefined(typeof(TroubleshootSampleAttribute), false))
                    continue;

                types.Add(candidate);
            }
        }
    }
}