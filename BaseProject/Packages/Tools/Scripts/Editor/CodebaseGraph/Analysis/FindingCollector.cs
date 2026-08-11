using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using Base.ToolPackage.Editor.CodebaseGraph.Scanning;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Turns a graph into a flat, ranked list of findings, one entry per finding rather than one per
    /// entity. Both the report and the findings window read from here, which is the point: a list you
    /// work through and a file you hand to somebody have to agree about what was found.
    /// </summary>
    internal static class FindingCollector
    {
        /// <summary>Gathers every finding still showing, ranked, one entry per finding.</summary>
        /// <param name="graph">Graph to read.</param>
        /// <returns>The entries, worst first.</returns>
        public static List<FindingEntry> Collect(CodebaseGraphData graph)
        {
            List<FindingEntry> entries = new();
            Dictionary<string, string[]> sources = new(StringComparer.Ordinal);
            HashSet<string> reportedCycles = new(StringComparer.Ordinal);

            foreach (NamespaceNodeInfo group in graph.Namespaces.Values)
                CollectNamespace(group, entries, reportedCycles);

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                CollectType(type, entries, sources, reportedCycles);
                CollectMembers(type, entries, sources);
            }

            entries.Sort(Compare);
            return entries;
        }

        private static int Compare(FindingEntry left, FindingEntry right)
        {
            int bySeverity = left.Severity.CompareTo(right.Severity);
            if (bySeverity != 0)
                return bySeverity;

            int byFinding = left.Finding.CompareTo(right.Finding);

            return byFinding != 0
                ? byFinding
                : string.Compare(left.Id, right.Id, StringComparison.OrdinalIgnoreCase);
        }

        private static void CollectNamespace(NamespaceNodeInfo group,
            List<FindingEntry> entries,
            HashSet<string> reportedCycles)
        {
            if (group.CyclePartners.Count == 0 || FindingCatalog.IsHidden(group))
                return;

            if (!reportedCycles.Add(group.CycleId))
                return;

            entries.Add(new FindingEntry(EFinding.NamespaceCycle,
                ESeverity.Medium,
                GraphIdentity.ForFinding(group.DismissalId, EFinding.NamespaceCycle),
                string.Empty,
                BuildCycleDetail(group.CycleDescription, group.CycleCutHint, group.CycleComponentSize)));
        }

        private static void CollectType(TypeNodeInfo type,
            List<FindingEntry> entries,
            Dictionary<string, string[]> sources,
            HashSet<string> reportedCycles)
        {
            if (type.Issues == ETypeIssue.None || FindingCatalog.IsHidden(type))
                return;

            List<EFinding> findings = new();
            FindingCatalog.Collect(type, findings);

            string location = BuildLocation(type, null, sources);

            foreach (EFinding finding in findings)
            {
                if (finding == EFinding.TypeCycle && !reportedCycles.Add(type.CycleId))
                    continue;

                string detail = finding == EFinding.TypeCycle
                    ? BuildCycleDetail(type.CycleDescription, type.CycleCutHint, type.CycleComponentSize)
                    : string.Empty;

                entries.Add(new FindingEntry(finding,
                    FindingSeverity.Resolve(finding, type),
                    GraphIdentity.ForFinding(type.DismissalId, finding),
                    location,
                    detail)
                {
                    Type = type
                });
            }
        }

        private static void CollectMembers(TypeNodeInfo type,
            List<FindingEntry> entries,
            Dictionary<string, string[]> sources)
        {
            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.Issues == EMemberIssue.None)
                    continue;

                List<EFinding> findings = new();
                FindingCatalog.Collect(member, type, findings);

                string location = BuildLocation(type, member, sources);

                foreach (EFinding finding in findings)
                {
                    entries.Add(new FindingEntry(finding,
                        FindingSeverity.Resolve(finding, member, type),
                        GraphIdentity.ForFinding(member.DismissalId, finding),
                        location,
                        BuildAssetDetail(finding, member))
                    {
                        Type = type,
                        Member = member
                    });
                }
            }
        }

        private static string BuildCycleDetail(string description, string cut, int componentSize)
        {
            string tangle = componentSize > 2
                ? $" It sits inside a tangle of {componentSize}."
                : string.Empty;

            string hint = string.IsNullOrEmpty(cut)
                ? string.Empty
                : $" Cheapest edge to cut: {cut}.";

            return $" Loop: {description}.{hint}{tangle}";
        }

        private static string BuildAssetDetail(EFinding finding, MemberNodeInfo member)
        {
            if (finding != EFinding.SerializedNeverRead)
                return string.Empty;

            if (member.AssetUsageCount == 0)
                return " no prefab, scene or asset sets it either";

            return member.AssetUsageCount == 1
                ? " set on 1 prefab, scene or asset"
                : $" set on {member.AssetUsageCount} prefabs, scenes or assets";
        }

        private static string BuildLocation(TypeNodeInfo type,
            MemberNodeInfo member,
            Dictionary<string, string[]> sources)
        {
            if (string.IsNullOrEmpty(type.ScriptPath))
                return string.Empty;

            if (!sources.TryGetValue(type.ScriptPath, out string[] lines))
            {
                lines = SourceLineLocator.Split(type.ScriptPath);
                sources[type.ScriptPath] = lines;
            }

            int line = SourceLineLocator.Find(lines, member, type.ShortName, type.Kind == ETypeKind.Interface);

            return $" - {type.ScriptPath}:{line}";
        }
    }
}