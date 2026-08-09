using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Turns the raw graph into findings. Everything here is a candidate, never a verdict: reflection,
    /// SendMessage, UnityEvent bindings and asset references are invisible to a code scan.
    /// </summary>
    public static class CodebaseGraphAnalyzer
    {
        private const int MinimumInterestingCycle = 2;
        private const string PairSeparator = " <-> ";

        /// <summary>Runs every check and writes the findings onto the nodes.</summary>
        /// <param name="graph">Graph to analyze.</param>
        public static void Analyze(CodebaseGraphData graph)
        {
            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                // Generated output, sample fixtures and tests are not code anyone is going to clean up.
                if (type.IsExcludedFromFindings)
                    continue;

                foreach (MemberNodeInfo member in type.Members)
                    AnalyzeMember(member, type, graph);

                AnalyzeType(type);
            }

            HashSet<string> namespacePairs = new();
            MarkTypeCycles(graph, namespacePairs);
            MarkNamespaceCycles(graph, namespacePairs);
        }

        private static bool IsStructural(UsageEdgeInfo edge)
            => edge.Kind == EUsageKind.Override || edge.Kind == EUsageKind.InterfaceImplementation;

        private static void AnalyzeMember(MemberNodeInfo member, TypeNodeInfo declaring, CodebaseGraphData graph)
        {
            // The ignore marker in a comment is the deliberate escape hatch for a known false positive.
            if (member.IsSuppressed)
                return;

            int realIncoming = CountRealIncoming(member);

            ApplyUnusedFinding(member, declaring, realIncoming);

            if (member.Kind == EMemberKind.SerializedField && !member.HasIncomingRead())
                member.Issues |= EMemberIssue.SerializedNeverRead;

            if (IsWriteOnly(member))
                member.Issues |= EMemberIssue.WriteOnlyField;

            if (IsReadOnlyCandidate(member, graph))
                member.Issues |= EMemberIssue.ReadOnlyCandidate;

            if (IsStaticMutableState(member, declaring, graph))
                member.Issues |= EMemberIssue.StaticMutableState;

            if (IsPrivateCandidate(member, declaring, realIncoming, graph))
                member.Issues |= EMemberIssue.PrivateCandidate;
            else if (!declaring.IsPackageAssembly && IsPublicButInternalOnly(member, realIncoming, graph))
                member.Issues |= EMemberIssue.PublicButInternalOnly;
        }

        private static void AnalyzeType(TypeNodeInfo type)
        {
            if (IsDeadTypeCandidate(type))
            {
                type.Issues |= type.IsPackageAssembly && type.Access == EAccessLevel.Public
                    ? ETypeIssue.UnusedPublicType
                    : ETypeIssue.DeadType;
            }

            if (CouplingMetrics.IsGodClass(type))
                type.Issues |= ETypeIssue.GodClass;

            if (CouplingMetrics.IsUnstableDependency(type))
                type.Issues |= ETypeIssue.HighInstability;
        }

        private static int CountRealIncoming(MemberNodeInfo member)
        {
            int count = 0;

            foreach (UsageEdgeInfo edge in member.Incoming)
            {
                if (!IsStructural(edge))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Decides which flavour of "nothing calls this" applies. The plain one is a defect. On the
        /// public surface of a distributable package it is expected, and on an interface it is a
        /// question about the contract rather than about dead code, so each gets its own wording.
        /// </summary>
        private static void ApplyUnusedFinding(MemberNodeInfo member, TypeNodeInfo declaring, int realIncoming)
        {
            if (!IsUnusedCandidate(member, realIncoming))
                return;

            if (declaring.Kind == ETypeKind.Interface)
            {
                // A default interface method carries its own body, so it is never waiting to be written.
                member.Issues |= member.ImplementationCount == 0 && member.IsAbstract
                    ? EMemberIssue.UnimplementedInterfaceMember
                    : EMemberIssue.UnusedInterfaceMember;

                return;
            }

            if (declaring.IsPackageAssembly && member.Access == EAccessLevel.Public)
            {
                member.Issues |= EMemberIssue.UnusedPublicApi;
                return;
            }

            member.Issues |= EMemberIssue.DeadMember;
        }

        private static bool IsUnusedCandidate(MemberNodeInfo member, int realIncoming)
        {
            if (realIncoming > 0)
                return false;

            if (member.IsEntryPoint || member.HasTextUsage || member.IsOverride)
                return false;

            // Constructors are reached through object creation and by the runtime rather than by name,
            // a serialized field gets its own more precise finding, and an enum member picked in the
            // inspector is stored as an integer in YAML that no code scan can ever see.
            return member.Kind != EMemberKind.SerializedField
                && member.Kind != EMemberKind.Constructor
                && member.Kind != EMemberKind.EnumMember;
        }

        private static bool IsDeadTypeCandidate(TypeNodeInfo type)
        {
            if (type.FanIn > 0 || type.IsEntryPoint)
                return false;

            // Unity objects are wired up in scenes, prefabs and assets, none of which a code scan sees.
            if (type.IsUnityObject)
                return false;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.IsEntryPoint || member.IsOverride)
                    return false;

                // Consts are inlined at every call site, so the only trace left is in the source text.
                if (member.HasTextUsage)
                    return false;

                if (CountRealIncoming(member) > 0)
                    return false;
            }

            return true;
        }

        private static bool IsWriteOnly(MemberNodeInfo member)
        {
            if (!member.IsDataMember || member.Kind == EMemberKind.Const)
                return false;

            return member.HasIncomingWrite() && !member.HasIncomingRead();
        }

        private static bool IsReadOnlyCandidate(MemberNodeInfo member, CodebaseGraphData graph)
        {
            if (member.Kind != EMemberKind.Field || member.IsReadOnly || member.IsEntryPoint)
                return false;

            bool foundWrite = false;

            foreach (UsageEdgeInfo edge in member.Incoming)
            {
                if (edge.Kind != EUsageKind.FieldWrite)
                    continue;

                foundWrite = true;

                MemberNodeInfo writer = graph.FindMember(edge.SourceKey);
                if (writer == null || writer.Kind != EMemberKind.Constructor)
                    return false;

                if (!writer.DeclaringTypeKey.Equals(member.DeclaringTypeKey))
                    return false;
            }

            return foundWrite;
        }

        private static bool IsStaticMutableState(MemberNodeInfo member,
            TypeNodeInfo declaring,
            CodebaseGraphData graph)
        {
            if (!member.IsStatic || member.IsReadOnly || member.Kind != EMemberKind.Field)
                return false;

            // Readonly is the better report for a field that is only ever built once.
            if (member.Issues.HasFlag(EMemberIssue.ReadOnlyCandidate))
                return false;

            // Editor only state never ships in a build, and the editor tears it down on its own terms.
            if (declaring.IsEditorOnly)
                return false;

            return !IsClearedOnPlayMode(member, graph);
        }

        private static bool IsClearedOnPlayMode(MemberNodeInfo member, CodebaseGraphData graph)
        {
            foreach (UsageEdgeInfo edge in member.Incoming)
            {
                if (edge.Kind != EUsageKind.FieldWrite)
                    continue;

                MemberNodeInfo writer = graph.FindMember(edge.SourceKey);
                if (writer != null && writer.IsStateReset)
                    return true;
            }

            return false;
        }

        private static bool IsPublicButInternalOnly(MemberNodeInfo member,
            int realIncoming,
            CodebaseGraphData graph)
        {
            if (member.Access != EAccessLevel.Public || realIncoming == 0)
                return false;

            if (member.IsEntryPoint || member.IsOverride)
                return false;

            if (member.IsVirtual || member.IsAbstract || HasImplementors(member))
                return false;

            if (member.ImplementsInterfaceMember)
                return false;

            TypeNodeInfo owner = graph.FindType(member.DeclaringTypeKey);
            if (owner == null || owner.Access != EAccessLevel.Public)
                return false;

            foreach (UsageEdgeInfo edge in member.Incoming)
            {
                if (IsStructural(edge))
                    continue;

                MemberNodeInfo caller = graph.FindMember(edge.SourceKey);
                if (caller == null)
                    continue;

                TypeNodeInfo callerType = graph.FindType(caller.DeclaringTypeKey);
                if (callerType == null)
                    continue;

                if (callerType.AssemblyName != owner.AssemblyName)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// True when every caller sits inside the declaring type itself. Private is the right answer
        /// there, and it is a stronger statement than internal, so it wins over the other finding.
        /// </summary>
        private static bool IsPrivateCandidate(MemberNodeInfo member,
            TypeNodeInfo declaring,
            int realIncoming,
            CodebaseGraphData graph)
        {
            if (member.Access == EAccessLevel.Private || realIncoming == 0)
                return false;

            if (member.IsEntryPoint || member.IsOverride || member.Kind == EMemberKind.Constructor)
                return false;

            // Private virtual does not compile, and private abstract does not either. A template method
            // hook is called only by its own base type by design, which is exactly this shape.
            if (member.IsVirtual || member.IsAbstract || HasImplementors(member))
                return false;

            if (member.ImplementsInterfaceMember)
                return false;

            foreach (UsageEdgeInfo edge in member.Incoming)
            {
                if (IsStructural(edge))
                    continue;

                MemberNodeInfo caller = graph.FindMember(edge.SourceKey);
                if (caller == null)
                    continue;

                if (!caller.DeclaringTypeKey.Equals(declaring.Key))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Reports the cycles that are actually fixable. A nested type always references the type it
        /// sits inside, which is not a loop anyone can break, and a two type cycle inside one namespace
        /// is ordinary ownership between a manager and the thing it manages.
        /// </summary>
        private static bool HasImplementors(MemberNodeInfo member)
        {
            if (member.ImplementationCount > 0)
                return true;

            foreach (UsageEdgeInfo edge in member.Incoming)
            {
                if (IsStructural(edge))
                    return true;
            }

            return false;
        }

        private static void MarkTypeCycles(CodebaseGraphData graph, HashSet<string> namespacePairs)
        {
            List<List<TypeKey>> cycles = CycleFinder.FindCycles(graph.Types.Keys,
                key => ReadCycleTargets(graph, key));

            foreach (List<TypeKey> cycle in cycles)
            {
                if (!IsReportableCycle(graph, cycle, namespacePairs))
                    continue;

                string cycleId = BuildCycleId(ReadTypeNames(graph, cycle));

                foreach (TypeKey key in cycle)
                    MarkCycleMember(graph, cycle, key, cycleId);
            }
        }

        private static IEnumerable<TypeKey> ReadCycleTargets(CodebaseGraphData graph, TypeKey key)
        {
            TypeNodeInfo source = graph.FindType(key);

            foreach (TypeKey target in source.Outgoing.Keys)
            {
                if (IsNestingPair(graph, source, target))
                    continue;

                yield return target;
            }
        }

        private static bool IsNestingPair(CodebaseGraphData graph, TypeNodeInfo source, TypeKey targetKey)
        {
            if (source.DeclaringTypeKey.Equals(targetKey))
                return true;

            TypeNodeInfo target = graph.FindType(targetKey);
            return target != null && target.DeclaringTypeKey.Equals(source.Key);
        }

        private static bool IsReportableCycle(CodebaseGraphData graph,
            List<TypeKey> cycle,
            HashSet<string> namespacePairs)
        {
            HashSet<string> namespaces = new();

            foreach (TypeKey key in cycle)
            {
                TypeNodeInfo type = graph.FindType(key);
                if (type != null)
                    namespaces.Add(type.Namespace);
            }

            // A pair inside one namespace is ownership, not a design problem worth a report line.
            if (cycle.Count <= MinimumInterestingCycle && namespaces.Count <= 1)
                return false;

            if (namespaces.Count == 2)
                namespacePairs.Add(BuildPairKey(namespaces));

            return true;
        }

        private static string BuildPairKey(IEnumerable<string> names)
        {
            List<string> sorted = new(names);
            sorted.Sort(StringComparer.Ordinal);

            return string.Join(PairSeparator, sorted);
        }

        private static List<string> ReadTypeNames(CodebaseGraphData graph, List<TypeKey> cycle)
        {
            List<string> names = new();

            foreach (TypeKey key in cycle)
            {
                TypeNodeInfo type = graph.FindType(key);
                if (type != null)
                    names.Add(type.ShortName);
            }

            return names;
        }

        private static string BuildCycleId(List<string> names)
        {
            List<string> sorted = new(names);
            sorted.Sort(StringComparer.Ordinal);

            return string.Join(PairSeparator, sorted);
        }

        private static void MarkCycleMember(CodebaseGraphData graph,
            List<TypeKey> cycle,
            TypeKey key,
            string cycleId)
        {
            TypeNodeInfo type = graph.FindType(key);
            if (type == null || type.IsExcludedFromFindings)
                return;

            type.Issues |= ETypeIssue.TypeCycle;
            type.CycleId = cycleId;

            foreach (TypeKey partner in cycle)
            {
                if (partner.Equals(key))
                    continue;

                type.CyclePartners.Add(graph.FindType(partner).ShortName);
            }
        }

        private static void MarkNamespaceCycles(CodebaseGraphData graph, HashSet<string> namespacePairs)
        {
            List<List<string>> cycles = CycleFinder.FindCycles(graph.Namespaces.Keys,
                name => graph.Namespaces[name].Outgoing.Keys);

            foreach (List<string> cycle in cycles)
            {
                if (cycle.Count == 2 && namespacePairs.Contains(BuildPairKey(cycle)))
                    continue;

                string cycleId = BuildCycleId(cycle);

                foreach (string name in cycle)
                {
                    NamespaceNodeInfo group = graph.Namespaces[name];
                    group.CycleId = cycleId;

                    foreach (string partner in cycle)
                    {
                        if (partner == name)
                            continue;

                        group.CyclePartners.Add(partner);
                    }
                }
            }
        }
    }
}
