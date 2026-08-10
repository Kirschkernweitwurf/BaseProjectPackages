using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Flattens the graph into the entries the window draws. Focus mode walks outward from one entry
    /// instead of applying the filters, so the picture around it always stays complete.
    /// </summary>
    public static class GraphEntryFactory
    {
        private const string AbstractModifier = "abstract ";
        private const int ColorSeedSegments = 2;
        private const string MemberIdPrefix = "me:";
        private const string MonoBehaviourNote = ", MonoBehaviour";
        private const string NamespaceIdPrefix = "ns:";
        private const string PluralSuffix = "s";
        private const string StaticModifier = "static ";
        private const string TypeIdPrefix = "ty:";

        /// <summary>Builds the id a member entry is published under.</summary>
        /// <param name="key">Identity of the member.</param>
        /// <returns>The entry id.</returns>
        public static string MakeMemberId(MemberKey key) => MemberIdPrefix + key;

        /// <summary>Builds the id a type entry is published under.</summary>
        /// <param name="key">Identity of the type.</param>
        /// <returns>The entry id.</returns>
        public static string MakeTypeId(TypeKey key) => TypeIdPrefix + key;

        /// <summary>Builds the namespace level entries.</summary>
        /// <param name="graph">Graph to read from.</param>
        /// <param name="filter">Current toolbar state.</param>
        /// <returns>The entries to draw.</returns>
        public static List<GraphEntry> BuildNamespaces(CodebaseGraphData graph, GraphFilter filter)
        {
            Dictionary<string, GraphEntry> byId = new();
            List<GraphEntry> entries = new();

            foreach (NamespaceNodeInfo group in graph.Namespaces.Values)
            {
                if (!IsVisible(group, filter))
                    continue;

                GraphEntry entry = new(MakeNamespaceId(group.Name),
                    group.Name,
                    Count(group.Types.Count, "type"),
                    BuildColorSeed(group.Name),
                    group.FanIn,
                    group.FanOut)
                {
                    Namespace = group,
                    CanDrillDown = true
                };

                FindingCatalog.Collect(group, entry.Findings);
                entry.NestedFindingCount = FindingCatalog.CountVisibleFindings(group);
                entries.Add(entry);
                byId[entry.Id] = entry;
            }

            foreach (GraphEntry entry in entries)
            {
                foreach (string target in entry.Namespace.Outgoing.Keys)
                {
                    if (byId.ContainsKey(MakeNamespaceId(target)))
                        entry.TargetIds.Add(MakeNamespaceId(target));
                }
            }

            return entries;
        }

        /// <summary>Builds the type level entries, either filtered or as the neighborhood of a focus.</summary>
        /// <param name="graph">Graph to read from.</param>
        /// <param name="filter">Current toolbar state.</param>
        /// <param name="namespaceName">Namespace to restrict to, or null for all.</param>
        /// <param name="focus">Type to center the view on, or null.</param>
        /// <returns>The entries to draw.</returns>
        public static List<GraphEntry> BuildTypes(CodebaseGraphData graph,
            GraphFilter filter,
            string namespaceName,
            TypeNodeInfo focus)
        {
            List<TypeNodeInfo> visible = focus == null
                ? CollectFilteredTypes(graph, filter, namespaceName)
                : CollectTypeNeighborhood(graph, focus, filter.Hops);

            Dictionary<TypeKey, GraphEntry> byKey = new();
            List<GraphEntry> entries = new();

            foreach (TypeNodeInfo type in visible)
            {
                GraphEntry entry = new(MakeTypeId(type.Key),
                    type.ShortName,
                    BuildTypeSubtitle(type),
                    BuildColorSeed(type.Namespace),
                    type.FanIn,
                    type.FanOut)
                {
                    Type = type,
                    CanDrillDown = true
                };

                FindingCatalog.Collect(type, entry.Findings);
                entry.NestedFindingCount = FindingCatalog.CountVisibleMemberFindings(type);

                entries.Add(entry);
                byKey[type.Key] = entry;
            }

            foreach (GraphEntry entry in entries)
            {
                foreach (TypeKey target in entry.Type.Outgoing.Keys)
                {
                    if (byKey.ContainsKey(target))
                        entry.TargetIds.Add(MakeTypeId(target));
                }
            }

            return entries;
        }

        /// <summary>Builds the member level entries for one type, or around one focused member.</summary>
        /// <param name="graph">Graph to read from.</param>
        /// <param name="filter">Current toolbar state.</param>
        /// <param name="owner">Type whose members are shown.</param>
        /// <param name="focus">Member to center the view on, or null.</param>
        /// <returns>The entries to draw.</returns>
        public static List<GraphEntry> BuildMembers(CodebaseGraphData graph,
            GraphFilter filter,
            TypeNodeInfo owner,
            MemberNodeInfo focus)
        {
            if (owner == null)
                return new List<GraphEntry>();

            List<MemberNodeInfo> visible = focus == null
                ? CollectFilteredMembers(owner, filter)
                : CollectMemberNeighborhood(graph, focus, filter.Hops);

            Dictionary<MemberKey, GraphEntry> byKey = new();
            List<GraphEntry> entries = new();

            foreach (MemberNodeInfo member in visible)
            {
                TypeNodeInfo declaring = graph.FindType(member.DeclaringTypeKey);
                bool isForeign = declaring != null && !declaring.Key.Equals(owner.Key);

                GraphEntry entry = new(MakeMemberId(member.Key),
                    isForeign
                        ? $"{declaring.ShortName}.{member.Name}"
                        : member.Signature,
                    BuildMemberSubtitle(member),
                    BuildColorSeed(declaring == null
                        ? owner.Namespace
                        : declaring.Namespace),
                    member.FanIn,
                    member.FanOut)
                {
                    Member = member,
                    Type = declaring
                };

                FindingCatalog.Collect(member, declaring, entry.Findings);
                entries.Add(entry);
                byKey[member.Key] = entry;
            }

            foreach (GraphEntry entry in entries)
            {
                foreach (UsageEdgeInfo edge in entry.Member.Outgoing)
                {
                    if (byKey.ContainsKey(edge.TargetKey))
                        entry.TargetIds.Add(MakeMemberId(edge.TargetKey));
                }
            }

            return entries;
        }

        private static bool IsVisible(NamespaceNodeInfo group, GraphFilter filter)
        {
            if (!FindingCatalog.IsMatch(filter.Finding, group))
                return false;

            if (!filter.IsMatch(group.Name))
                return false;

            if (string.IsNullOrEmpty(filter.AssemblyName))
                return true;

            foreach (TypeNodeInfo type in group.Types)
            {
                if (type.AssemblyName == filter.AssemblyName)
                    return true;
            }

            return false;
        }

        private static List<TypeNodeInfo> CollectFilteredTypes(CodebaseGraphData graph,
            GraphFilter filter,
            string namespaceName)
        {
            List<TypeNodeInfo> result = new();

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                if (namespaceName != null && type.Namespace != namespaceName)
                    continue;

                if (!string.IsNullOrEmpty(filter.AssemblyName) && type.AssemblyName != filter.AssemblyName)
                    continue;

                if (!FindingCatalog.IsMatch(filter.Finding, type))
                    continue;

                if (!filter.IsMatch(type.FullName))
                    continue;

                result.Add(type);
            }

            return result;
        }

        private static List<TypeNodeInfo> CollectTypeNeighborhood(CodebaseGraphData graph,
            TypeNodeInfo focus,
            int hops)
        {
            HashSet<TypeKey> seen = new() { focus.Key };
            List<TypeNodeInfo> result = new() { focus };
            List<TypeNodeInfo> frontier = new() { focus };

            for (int step = 0; step < hops; step++)
            {
                List<TypeNodeInfo> next = new();

                foreach (TypeNodeInfo current in frontier)
                {
                    AddTypes(graph, current.Outgoing.Keys, seen, result, next);
                    AddTypes(graph, current.Incoming.Keys, seen, result, next);
                }

                frontier = next;
            }

            return result;
        }

        private static void AddTypes(CodebaseGraphData graph,
            IEnumerable<TypeKey> keys,
            HashSet<TypeKey> seen,
            List<TypeNodeInfo> result,
            List<TypeNodeInfo> next)
        {
            foreach (TypeKey key in keys)
            {
                if (!seen.Add(key))
                    continue;

                TypeNodeInfo type = graph.FindType(key);
                if (type == null)
                    continue;

                result.Add(type);
                next.Add(type);
            }
        }

        private static List<MemberNodeInfo> CollectFilteredMembers(TypeNodeInfo owner, GraphFilter filter)
        {
            List<MemberNodeInfo> result = new();

            foreach (MemberNodeInfo member in owner.Members)
            {
                if (!filter.ShowPrivate && member.Access == EAccessLevel.Private)
                    continue;

                if (!filter.ShowDataMembers && member.IsDataMember)
                    continue;

                if (!FindingCatalog.IsMatch(filter.Finding, member, owner))
                    continue;

                if (!filter.IsMatch(member.Name))
                    continue;

                result.Add(member);
            }

            return result;
        }

        private static List<MemberNodeInfo> CollectMemberNeighborhood(CodebaseGraphData graph,
            MemberNodeInfo focus,
            int hops)
        {
            HashSet<MemberKey> seen = new() { focus.Key };
            List<MemberNodeInfo> result = new() { focus };
            List<MemberNodeInfo> frontier = new() { focus };

            for (int step = 0; step < hops; step++)
            {
                List<MemberNodeInfo> next = new();

                foreach (MemberNodeInfo current in frontier)
                {
                    foreach (UsageEdgeInfo edge in current.Outgoing)
                        AddMember(graph, edge.TargetKey, seen, result, next);

                    foreach (UsageEdgeInfo edge in current.Incoming)
                        AddMember(graph, edge.SourceKey, seen, result, next);
                }

                frontier = next;
            }

            return result;
        }

        private static void AddMember(CodebaseGraphData graph,
            MemberKey key,
            HashSet<MemberKey> seen,
            List<MemberNodeInfo> result,
            List<MemberNodeInfo> next)
        {
            if (!seen.Add(key))
                return;

            MemberNodeInfo member = graph.FindMember(key);
            if (member == null)
                return;

            result.Add(member);
            next.Add(member);
        }

        private static string BuildMemberSubtitle(MemberNodeInfo member)
        {
            string prefix = member.IsStatic
                ? StaticModifier
                : string.Empty;

            string size = member.IlSize > 0
                ? $", {member.IlSize} bytes"
                : string.Empty;

            return $"{prefix}{member.Access} {member.Kind}{size}";
        }

        private static string MakeNamespaceId(string name) => NamespaceIdPrefix + name;

        private static string BuildTypeSubtitle(TypeNodeInfo type)
        {
            string note = type.IsMonoBehaviour
                ? MonoBehaviourNote
                : string.Empty;

            return $"{type.Access} {BuildTypeModifier(type)}{type.Kind}{note}, "
                + $"{Count(type.Members.Count, "member")}, "
                + $"{Count(type.ExternalReferenceCount, "external ref")}";
        }

        private static string BuildTypeModifier(TypeNodeInfo type)
        {
            if (type.IsStatic)
                return StaticModifier;

            return type.IsAbstract
                ? AbstractModifier
                : string.Empty;
        }

        private static string Count(int value, string singular)
        {
            string suffix = value == 1
                ? string.Empty
                : PluralSuffix;

            return $"{value} {singular}{suffix}";
        }

        /// <summary>
        /// Colors are keyed on the first two namespace segments, so each package reads as its own
        /// family instead of every Base namespace collapsing onto one tint.
        /// </summary>
        private static string BuildColorSeed(string name)
        {
            string[] segments = name.Split('.');
            int take = segments.Length < ColorSeedSegments
                ? segments.Length
                : ColorSeedSegments;

            return string.Join(".", segments, 0, take);
        }
    }
}
