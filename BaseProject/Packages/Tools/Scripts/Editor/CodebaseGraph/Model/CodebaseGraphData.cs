using System.Collections.Generic;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>The complete scan result: every type, member and relation the builder found.</summary>
    public sealed class CodebaseGraphData
    {
        /// <summary>Name used for types that are declared outside any namespace.</summary>
        public const string GlobalNamespaceName = "(global)";

        /// <summary>Every scanned type, keyed by identity.</summary>
        public Dictionary<TypeKey, TypeNodeInfo> Types { get; }

        /// <summary>Every scanned member, keyed by identity.</summary>
        public Dictionary<MemberKey, MemberNodeInfo> Members { get; }

        /// <summary>Every namespace that holds at least one scanned type, keyed by name.</summary>
        public Dictionary<string, NamespaceNodeInfo> Namespaces { get; }

        /// <summary>Names of the assemblies that were scanned.</summary>
        public List<string> ScannedAssemblies { get; }

        /// <summary>Names of the scanned assemblies that ship inside a distributable package.</summary>
        public HashSet<string> PackageAssemblies { get; }

        /// <summary>
        /// Places where an ignore marker was written but matched no member. A marker that quietly does
        /// nothing is the worst failure a tool for finding silent problems can have.
        /// </summary>
        public List<string> UnmatchedIgnoreMarkers { get; }

        /// <summary>How long the scan took, in seconds.</summary>
        public float ScanSeconds { get; set; }

        /// <summary>Number of metadata tokens that could not be resolved and were skipped.</summary>
        public int UnresolvedTokenCount { get; set; }

        /// <summary>Number of types in the scan.</summary>
        public int TypeCount => Types.Count;

        /// <summary>Number of members in the scan.</summary>
        public int MemberCount => Members.Count;

        /// <summary>Creates an empty graph.</summary>
        public CodebaseGraphData()
        {
            Types = new Dictionary<TypeKey, TypeNodeInfo>();
            Members = new Dictionary<MemberKey, MemberNodeInfo>();
            Namespaces = new Dictionary<string, NamespaceNodeInfo>();
            ScannedAssemblies = new List<string>();
            PackageAssemblies = new HashSet<string>();
            UnmatchedIgnoreMarkers = new List<string>();
        }

        /// <summary>Returns the type for a key, or null when it is outside the scanned scope.</summary>
        /// <param name="key">Identity of the type.</param>
        /// <returns>The type node, or null.</returns>
        public TypeNodeInfo FindType(TypeKey key)
            => Types.TryGetValue(key, out TypeNodeInfo type)
                ? type
                : null;

        /// <summary>Returns the member for a key, or null when it is outside the scanned scope.</summary>
        /// <param name="key">Identity of the member.</param>
        /// <returns>The member node, or null.</returns>
        public MemberNodeInfo FindMember(MemberKey key)
            => Members.TryGetValue(key, out MemberNodeInfo member)
                ? member
                : null;

        /// <summary>Counts the types whose findings were suppressed as generated, sample or test code.</summary>
        /// <returns>The number of excluded types.</returns>
        public int CountExcludedTypes()
        {
            int count = 0;
            foreach (TypeNodeInfo type in Types.Values)
            {
                if (type.IsExcludedFromFindings)
                    count++;
            }

            return count;
        }

        /// <summary>Counts every member finding across the whole graph.</summary>
        /// <returns>The total number of members that carry at least one finding.</returns>
        public int CountMemberIssues()
        {
            int count = 0;
            foreach (MemberNodeInfo member in Members.Values)
            {
                if (member.HasIssues)
                    count++;
            }

            return count;
        }

        /// <summary>Counts every type finding across the whole graph.</summary>
        /// <returns>The total number of types that carry at least one finding.</returns>
        public int CountTypeIssues()
        {
            int count = 0;
            foreach (TypeNodeInfo type in Types.Values)
            {
                if (type.HasIssues)
                    count++;
            }

            return count;
        }
    }
}
