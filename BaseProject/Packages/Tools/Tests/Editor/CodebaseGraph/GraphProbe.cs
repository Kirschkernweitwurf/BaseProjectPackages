using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.Tests
{
    /// <summary>
    /// Asks a scanned graph what it decided about one member. The tests read as statements about
    /// liveness rather than as dictionary lookups, which is the only way a suite about being right stays
    /// readable as the shapes it covers grow.
    /// </summary>
    public sealed class GraphProbe
    {
        private const string MissingText = "the scan never saw it";
        private const char NameSeparator = '.';
        private const string NothingText = "nothing reported";

        private readonly CodebaseGraphData _graph;

        /// <summary>Creates a probe over a scanned graph.</summary>
        /// <param name="graph">Graph to question.</param>
        public GraphProbe(CodebaseGraphData graph) => _graph = graph;

        /// <summary>Finds a type by its namespace qualified name.</summary>
        /// <param name="fullName">Full name of the type.</param>
        /// <returns>The type, or null when the scan never saw it.</returns>
        public TypeNodeInfo FindType(string fullName)
        {
            foreach (TypeNodeInfo type in _graph.Types.Values)
            {
                if (type.FullName == fullName)
                    return type;
            }

            return null;
        }

        /// <summary>
        /// Finds a member by name. The name is matched on its last segment, so an explicit interface
        /// implementation can be asked about as Explicit rather than by the fully qualified name the
        /// compiler gave it.
        /// </summary>
        /// <param name="typeName">Full name of the declaring type.</param>
        /// <param name="memberName">Plain member name.</param>
        /// <returns>The member, or null when the scan never saw it.</returns>
        public MemberNodeInfo FindMember(string typeName, string memberName)
        {
            TypeNodeInfo type = FindType(typeName);
            if (type == null)
                return null;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (ReadLastSegment(member.Name) == memberName)
                    return member;
            }

            return null;
        }

        /// <summary>Lists the findings on a member, for a readable failure message.</summary>
        /// <param name="typeName">Full name of the declaring type.</param>
        /// <param name="memberName">Plain member name.</param>
        /// <returns>The findings as text.</returns>
        public string Describe(string typeName, string memberName)
        {
            MemberNodeInfo member = FindMember(typeName, memberName);

            return member == null
                ? MissingText
                : member.Issues.ToString();
        }

        /// <summary>True when the analyzer reported a particular finding on a member.</summary>
        /// <param name="typeName">Full name of the declaring type.</param>
        /// <param name="memberName">Plain member name.</param>
        /// <param name="issue">Finding to look for.</param>
        /// <returns>True when the finding is present.</returns>
        public bool HasIssue(string typeName, string memberName, EMemberIssue issue)
        {
            MemberNodeInfo member = FindMember(typeName, memberName);

            return member != null && member.Issues.HasFlag(issue);
        }

        /// <summary>Lists every member of a type that carries a finding, for a readable failure.</summary>
        /// <param name="typeName">Full name of the type.</param>
        /// <returns>The reported members as text.</returns>
        public string DescribeType(string typeName)
        {
            TypeNodeInfo type = FindType(typeName);
            if (type == null)
                return MissingText;

            List<string> reported = new();

            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.HasIssues)
                    reported.Add($"{member.Name} [{member.Issues}]");
            }

            return reported.Count == 0
                ? NothingText
                : string.Join(", ", reported);
        }

        private static string ReadLastSegment(string name)
        {
            int separator = name.LastIndexOf(NameSeparator);

            return separator < 0
                ? name
                : name[(separator + 1)..];
        }
    }
}
