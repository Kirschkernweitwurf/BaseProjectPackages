using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Flattens the graph into the entries the window draws. Focus mode walks outward from one entry
    /// instead of applying the filters, so the surrounding picture always stays complete.
    /// Searching ignores the current level entirely, because looking for a class you cannot place
    /// is exactly when you do not know which namespace to be standing in.
    /// </summary>
    internal static class GraphEntryFactory
    {
        private const string AbstractModifier = "abstract ";
        private const int ColorSeedSegments = 2;
        private const int MaximumTypes = 150;
        private const int MaxRowsPerType = 14;
        private const int MaxSearchResults = 150;
        private const string MemberIdPrefix = "me:";
        private const string MonoBehaviourNote = ", MonoBehaviour";
        private const string NamespaceIdPrefix = "ns:";
        private const string StaticModifier = "static ";
        private const string SubtitleSeparator = "  \u00b7  ";
        private const string TypeIdPrefix = "ty:";

        /// <summary>Builds the id a member entry is published under.</summary>
        /// <param name="key">Identity of the member.</param>
        /// <returns>The entry id.</returns>
        internal static string MakeMemberId(MemberKey key) => MemberIdPrefix + key;

        /// <summary>Builds the id a type entry is published under.</summary>
        /// <param name="key">Identity of the type.</param>
        /// <returns>The entry id.</returns>
        internal static string MakeTypeId(TypeKey key) => TypeIdPrefix + key;

        /// <summary>Builds the id a namespace entry is published under.</summary>
        /// <param name="name">Full namespace name.</param>
        /// <returns>The entry id.</returns>
        internal static string MakeNamespaceId(string name) => NamespaceIdPrefix + name;

        /// <summary>Builds the namespace level entries, or the neighborhood of a focused one.</summary>
        /// <param name="graph">Graph to read from.</param>
        /// <param name="filter">Current toolbar state.</param>
        /// <param name="focus">Namespace to center the view on, or null.</param>
        /// <returns>The entries to draw.</returns>
        internal static List<GraphEntry> BuildNamespaces(CodebaseGraphData graph,
            GraphFilter filter,
            NamespaceNodeInfo focus)
        {
            List<NamespaceNodeInfo> visible = focus == null
                ? CollectFilteredNamespaces(graph, filter)
                : CollectNamespaceNeighborhood(graph, focus, filter.Hops);

            Dictionary<string, GraphEntry> byId = new();
            List<GraphEntry> entries = new();

            foreach (NamespaceNodeInfo group in visible)
            {
                GraphEntry entry = BuildNamespaceEntry(group);
                entries.Add(entry);
                byId[entry.Id] = entry;
            }

            foreach (GraphEntry entry in entries)
                LinkNamespace(entry, byId);

            return entries;
        }

        /// <summary>Builds the type level entries, either filtered or as the neighborhood of a focus.</summary>
        /// <param name="graph">Graph to read from.</param>
        /// <param name="filter">Current toolbar state.</param>
        /// <param name="namespaceName">Namespace to restrict to, or null for all.</param>
        /// <param name="focus">Type to center the view on, or null.</param>
        /// <returns>The entries to draw.</returns>
        internal static List<GraphEntry> BuildTypes(CodebaseGraphData graph,
            GraphFilter filter,
            string namespaceName,
            TypeNodeInfo focus,
            out int total)
        {
            List<TypeNodeInfo> visible = focus == null
                ? CollectFilteredTypes(graph, filter, namespaceName)
                : CollectTypeNeighborhood(graph, focus, filter.Hops);

            total = visible.Count;

            Dictionary<TypeKey, GraphEntry> byKey = new();
            List<GraphEntry> entries = new();

            foreach (TypeNodeInfo type in visible)
            {
                // Every node carries member rows and goes through the layout, so an unfiltered
                // namespace of a few hundred types is the one way left to hang the editor.
                if (entries.Count == MaximumTypes)
                    break;

                GraphEntry entry = BuildTypeEntry(type, filter);
                entries.Add(entry);
                byKey[type.Key] = entry;
            }

            foreach (GraphEntry entry in entries)
                LinkType(entry, byKey);

            return entries;
        }

        /// <summary>Builds the member level entries for one type, or around one focused member.</summary>
        /// <param name="graph">Graph to read from.</param>
        /// <param name="filter">Current toolbar state.</param>
        /// <param name="owner">Type whose members are shown.</param>
        /// <param name="focus">Member to center the view on, or null.</param>
        /// <returns>The entries to draw.</returns>
        internal static List<GraphEntry> BuildMembers(CodebaseGraphData graph,
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
                GraphEntry entry = BuildMemberEntry(member, graph.FindType(member.DeclaringTypeKey), owner);
                entries.Add(entry);
                byKey[member.Key] = entry;
            }

            foreach (GraphEntry entry in entries)
                LinkMember(entry, byKey);

            return entries;
        }

        /// <summary>
        /// Builds matches from every level at once. Searching is what you reach for when you know a name
        /// and not where it lives, so restricting it to the level you happen to be standing on defeats
        /// the point.
        /// </summary>
        /// <param name="graph">Graph to read from.</param>
        /// <param name="filter">Current toolbar state, carrying the search text.</param>
        /// <param name="total">Receives how many matched before the cap was applied.</param>
        /// <returns>The entries to draw.</returns>
        internal static List<GraphEntry> BuildSearch(CodebaseGraphData graph, GraphFilter filter, out int total)
        {
            bool wantsTypes = filter.SearchScope != ESearchScope.Members;
            bool wantsMembers = filter.SearchScope != ESearchScope.Types;
            bool wantsNamespaces = filter.SearchScope == ESearchScope.Everywhere;

            List<GraphEntry> entries = new();
            Dictionary<string, GraphEntry> byId = new();
            Dictionary<TypeKey, GraphEntry> byType = new();
            Dictionary<MemberKey, GraphEntry> byMember = new();
            total = 0;

            if (wantsNamespaces)
                foreach (NamespaceNodeInfo group in graph.Namespaces.Values)
                {
                    if (!filter.IsMatch(group.Name) || !FindingCatalog.IsMatch(filter.Finding, group))
                        continue;

                    total++;
                    Accept(entries, byId, BuildNamespaceEntry(group));
                }

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                if (wantsTypes
                    && filter.IsMatch(type.FullName)
                    && FindingCatalog.IsMatch(filter.Finding, type))
                {
                    total++;
                    GraphEntry entry = BuildTypeEntry(type, filter);

                    if (Accept(entries, byId, entry))
                        byType[type.Key] = entry;
                }

                if (wantsMembers)
                    CollectSearchMembers(type, filter, entries, byId, byMember, ref total);
            }

            foreach (GraphEntry entry in entries)
                LinkSearch(entry, byId, byType, byMember);

            return entries;
        }

        private static void CollectSearchMembers(TypeNodeInfo type,
            GraphFilter filter,
            List<GraphEntry> entries,
            Dictionary<string, GraphEntry> byId,
            Dictionary<MemberKey, GraphEntry> byMember,
            ref int total)
        {
            foreach (MemberNodeInfo member in type.Members)
            {
                if (!filter.IsMatch(member.Name) || !FindingCatalog.IsMatch(filter.Finding, member, type))
                    continue;

                total++;
                GraphEntry entry = BuildMemberEntry(member, type, type);

                if (Accept(entries, byId, entry))
                    byMember[member.Key] = entry;
            }
        }

        private static bool Accept(List<GraphEntry> entries,
            Dictionary<string, GraphEntry> byId,
            GraphEntry entry)
        {
            if (entries.Count >= MaxSearchResults)
                return false;

            entries.Add(entry);
            byId[entry.Id] = entry;
            return true;
        }

        private static GraphEntry BuildNamespaceEntry(NamespaceNodeInfo group)
        {
            GraphEntry entry = new(MakeNamespaceId(group.Name),
                group.Name,
                Count(group.Types.Count, "type"),
                BuildColorSeed(group.Name),
                group.FanIn,
                group.FanOut,
                EGraphScope.Namespace)
            {
                Namespace = group,
                CanDrillDown = true,
                Glyph = GraphSymbols.NamespaceGlyph,
                Access = EAccessLevel.Public
            };

            FindingCatalog.Collect(group, entry.Findings);
            entry.NestedFindingCount = FindingCatalog.CountVisibleFindings(group);
            entry.IsDismissed = FindingCatalog.IsHidden(group);
            entry.DismissedNestedCount = FindingCatalog.CountDismissedFindings(group);

            return entry;
        }

        private static GraphEntry BuildTypeEntry(TypeNodeInfo type, GraphFilter filter)
        {
            GraphEntry entry = new(MakeTypeId(type.Key),
                type.ShortName,
                BuildTypeSubtitle(type),
                BuildColorSeed(type.Namespace),
                type.FanIn,
                type.FanOut,
                EGraphScope.Type)
            {
                Type = type,
                CanDrillDown = true,
                Glyph = GraphSymbols.GetGlyph(type.Kind),
                Access = type.Access,
                IsContract = type.Kind == ETypeKind.Interface
            };

            FindingCatalog.Collect(type, entry.Findings);
            entry.NestedFindingCount = FindingCatalog.CountVisibleMemberFindings(type);
            entry.IsDismissed = FindingCatalog.IsHidden(type);
            entry.DismissedNestedCount = FindingCatalog.CountDismissedMemberFindings(type);
            AppendRows(type, filter, entry);

            return entry;
        }

        private static GraphEntry BuildMemberEntry(MemberNodeInfo member,
            TypeNodeInfo declaring,
            TypeNodeInfo owner)
        {
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
                member.FanOut,
                EGraphScope.Member)
            {
                Member = member,
                Type = declaring,
                Glyph = GraphSymbols.GetGlyph(member.Kind),
                Access = member.Access,
                IsContract = declaring is
                {
                    Kind: ETypeKind.Interface
                }
            };

            FindingCatalog.Collect(member, declaring, entry.Findings);
            entry.IsDismissed = member.HasIssues
                && declaring != null
                && FindingCatalog.IsHidden(declaring, member);

            return entry;
        }

        private static void LinkNamespace(GraphEntry entry, Dictionary<string, GraphEntry> byId)
        {
            foreach (KeyValuePair<string, int> target in entry.Namespace.Outgoing)
            {
                if (byId.ContainsKey(MakeNamespaceId(target.Key)))
                    entry.Targets.Add(new GraphEdgeInfo(MakeNamespaceId(target.Key), target.Value));
            }
        }

        private static void LinkType(GraphEntry entry, Dictionary<TypeKey, GraphEntry> byKey)
        {
            foreach (KeyValuePair<TypeKey, int> target in entry.Type.Outgoing)
            {
                if (byKey.ContainsKey(target.Key))
                    entry.Targets.Add(new GraphEdgeInfo(MakeTypeId(target.Key), target.Value));
            }
        }

        private static void LinkMember(GraphEntry entry, Dictionary<MemberKey, GraphEntry> byKey)
        {
            foreach (UsageEdgeInfo edge in entry.Member.Outgoing)
            {
                if (byKey.ContainsKey(edge.TargetKey))
                    entry.Targets.Add(new GraphEdgeInfo(MakeMemberId(edge.TargetKey), edge.Count));
            }
        }

        private static void LinkSearch(GraphEntry entry,
            Dictionary<string, GraphEntry> byId,
            Dictionary<TypeKey, GraphEntry> byType,
            Dictionary<MemberKey, GraphEntry> byMember)
        {
            if (entry.Member != null)
            {
                LinkMember(entry, byMember);
                return;
            }

            if (entry.Namespace != null)
            {
                LinkNamespace(entry, byId);
                return;
            }

            LinkType(entry, byType);
        }

        private static List<NamespaceNodeInfo> CollectFilteredNamespaces(CodebaseGraphData graph,
            GraphFilter filter)
        {
            List<NamespaceNodeInfo> result = new();

            foreach (NamespaceNodeInfo group in graph.Namespaces.Values)
            {
                if (IsVisible(group, filter) && (!filter.OnlyNew || group.HasNewFindings))
                    result.Add(group);
            }

            return result;
        }

        private static List<NamespaceNodeInfo> CollectNamespaceNeighborhood(CodebaseGraphData graph,
            NamespaceNodeInfo focus,
            int hops)
        {
            HashSet<string> seen = new()
            {
                focus.Name
            };

            List<NamespaceNodeInfo> result = new()
            {
                focus
            };

            List<NamespaceNodeInfo> frontier = new()
            {
                focus
            };

            for (int step = 0; step < hops; step++)
            {
                List<NamespaceNodeInfo> next = new();

                foreach (NamespaceNodeInfo current in frontier)
                {
                    AddNamespaces(graph, current.Outgoing.Keys, seen, result, next);
                    AddNamespaces(graph, current.Incoming.Keys, seen, result, next);
                }

                frontier = next;
            }

            return result;
        }

        private static void AddNamespaces(CodebaseGraphData graph,
            IEnumerable<string> names,
            HashSet<string> seen,
            List<NamespaceNodeInfo> result,
            List<NamespaceNodeInfo> next)
        {
            foreach (string name in names)
            {
                if (!seen.Add(name) || !graph.Namespaces.TryGetValue(name, out NamespaceNodeInfo group))
                    continue;

                result.Add(group);
                next.Add(group);
            }
        }

        /// <summary>True when the type itself or anything it declares was newly reported.</summary>
        private static bool HasNewFindings(TypeNodeInfo type)
        {
            if (type.HasNewFindings)
                return true;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.HasNewFindings)
                    return true;
            }

            return false;
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

                if (filter.OnlyNew && !HasNewFindings(type))
                    continue;

                result.Add(type);
            }

            return result;
        }

        private static List<TypeNodeInfo> CollectTypeNeighborhood(CodebaseGraphData graph,
            TypeNodeInfo focus,
            int hops)
        {
            HashSet<TypeKey> seen = new()
            {
                focus.Key
            };

            List<TypeNodeInfo> result = new()
            {
                focus
            };

            List<TypeNodeInfo> frontier = new()
            {
                focus
            };

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

                if (filter.OnlyNew && !member.HasNewFindings)
                    continue;

                result.Add(member);
            }

            return result;
        }

        private static List<MemberNodeInfo> CollectMemberNeighborhood(CodebaseGraphData graph,
            MemberNodeInfo focus,
            int hops)
        {
            HashSet<MemberKey> seen = new()
            {
                focus.Key
            };

            List<MemberNodeInfo> result = new()
            {
                focus
            };

            List<MemberNodeInfo> frontier = new()
            {
                focus
            };

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

        /// <summary>
        /// Fills the member list a type node draws. Reading a class means reading its members, so the
        /// node shows them rather than only counting them, capped so one large type cannot dominate.
        /// </summary>
        private static void AppendRows(TypeNodeInfo type, GraphFilter filter, GraphEntry entry)
        {
            if (!filter.ShowMembersOnTypes)
                return;

            List<MemberNodeInfo> members = new(type.Members);
            members.Sort(CompareMembers);

            foreach (MemberNodeInfo member in members)
            {
                if (entry.Rows.Count == MaxRowsPerType)
                {
                    entry.HiddenRowCount = members.Count - MaxRowsPerType;
                    return;
                }

                bool isDismissed = member.HasIssues && FindingCatalog.IsHidden(type, member);

                entry.Rows.Add(new GraphMemberRow(GraphSymbols.GetGlyph(member.Kind),
                    member.Signature,
                    member.Access,
                    member.HasIssues && !isDismissed,
                    isDismissed));
            }
        }

        private static int CompareMembers(MemberNodeInfo left, MemberNodeInfo right)
        {
            int byKind = left.Kind.CompareTo(right.Kind);

            return byKind != 0
                ? byKind
                : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildTypeSubtitle(TypeNodeInfo type)
        {
            string note = type.IsMonoBehaviour
                ? MonoBehaviourNote
                : string.Empty;

            string members = Count(type.Members.Count, CodebaseGraphStyle.MemberClass);

            return $"{type.Access} {BuildTypeModifier(type)}{type.Kind}{note}{SubtitleSeparator}{members}";
        }

        private static string BuildTypeModifier(TypeNodeInfo type)
        {
            if (type.IsStatic)
                return StaticModifier;

            return type.IsAbstract
                ? AbstractModifier
                : string.Empty;
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

        private static string Count(int value, string singular)
        {
            string suffix = value == 1
                ? string.Empty
                : CodebaseGraphStyle.SClass;

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