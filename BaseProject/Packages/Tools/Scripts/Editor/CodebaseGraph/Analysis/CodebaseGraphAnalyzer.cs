using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Turns the raw graph into findings. Everything here is a candidate, never a verdict: reflection,
    /// SendMessage, UnityEvent bindings and asset references are invisible to a code scan.
    /// </summary>
    internal static class CodebaseGraphAnalyzer
    {
        private const string EdgeSeparator = " -> ";
        private const string ManyUsagesSuffix = " usages";
        private const int MinimumCycleLength = 2;
        private const int MinimumInterestingCycle = 2;
        private const char NestingSeparator = '.';
        private const string PairSeparator = " <-> ";
        private const string SingleUsageText = "1 usage";

        /// <summary>Runs every check and writes the findings onto the nodes.</summary>
        /// <param name="graph">Graph to analyze.</param>
        /// <param name="includeExcludedScopes">
        /// True to analyze generated, sample and test code as well. Only the tool's own test fixture
        /// asks for this: the shapes it checks the liveness rules against live in a test assembly, which
        /// is precisely the scope those findings are normally suppressed for.
        /// </param>
        public static void Analyze(CodebaseGraphData graph, bool includeExcludedScopes = false)
        {
            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                // Generated output, sample fixtures and tests are not code anyone is going to clean up.
                if (type.IsExcludedFromFindings && !includeExcludedScopes)
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

            // A published field written by design and read from another project is unused API, not a
            // defect. Written but never read reads as a mistake, and for a package surface it is not.
            if (IsWriteOnly(member))
                member.Issues |= PackageApi.IsSurface(member, declaring)
                    ? EMemberIssue.UnusedPublicApi
                    : EMemberIssue.WriteOnlyField;

            if (IsReadOnlyCandidate(member, graph))
                member.Issues |= EMemberIssue.ReadOnlyCandidate;

            if (IsStaticMutableState(member, declaring, graph))
                member.Issues |= EMemberIssue.StaticMutableState;

            if (IsPrivateCandidate(member, declaring, realIncoming, graph))
                member.Issues |= EMemberIssue.PrivateCandidate;
            else if (!PackageApi.IsSurface(member, declaring)
                     && IsPublicButInternalOnly(member, realIncoming, graph))
                member.Issues |= EMemberIssue.PublicButInternalOnly;
        }

        private static void AnalyzeType(TypeNodeInfo type)
        {
            if (IsDeadTypeCandidate(type))
                type.Issues |= PackageApi.IsSurface(type)
                    ? ETypeIssue.UnusedPublicType
                    : ETypeIssue.DeadType;

            if (CouplingMetrics.IsGodClass(type))
                type.Issues |= ETypeIssue.GodClass;

            if (CouplingMetrics.IsHardToChange(type))
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

            if (PackageApi.IsSurface(member, declaring))
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

            if (member.ImplementsInterfaceMember || PackageApi.IsSurface(member, declaring))
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
            List<List<TypeKey>> components = CycleFinder.FindCycles(graph.Types.Keys,
                getTargets: key => ReadCycleTargets(graph, key));

            foreach (List<TypeKey> component in components)
            {
                List<TypeKey> cycle = CycleFinder.FindShortestCycle(component,
                    getTargets: key => ReadCycleTargets(graph, key));

                if (cycle.Count < MinimumCycleLength
                    || !IsReportableCycle(graph, cycle, component.Count, namespacePairs))
                    continue;

                string cycleId = BuildSortedKey(ReadTypeNames(graph, cycle));
                string description = DescribeTypeCycle(graph, cycle);
                string cut = SuggestTypeCut(graph, cycle);

                foreach (TypeKey key in cycle)
                    MarkCycleMember(graph, cycle, key, cycleId, description, cut, component.Count);
            }
        }

        /// <summary>
        /// Writes the loop out as the path it actually is. A component holds many overlapping loops, so
        /// naming all of its members says nothing about which arrow to remove.
        /// </summary>
        private static string DescribeTypeCycle(CodebaseGraphData graph, List<TypeKey> cycle)
        {
            List<string> names = ReadTypeNames(graph, cycle);
            names.Add(names[0]);

            return string.Join(EdgeSeparator, names);
        }

        /// <summary>
        /// Picks the edge in the loop that the fewest usages hold together. It is the cheapest one to
        /// break, and it is offered as a hint rather than a verdict: the count says how much code has to
        /// move, not whether that is the dependency which should never have existed.
        /// </summary>
        private static string SuggestTypeCut(CodebaseGraphData graph, List<TypeKey> cycle)
        {
            string best = string.Empty;
            int lowest = int.MaxValue;

            for (int index = 0; index < cycle.Count; index++)
            {
                TypeNodeInfo source = graph.FindType(cycle[index]);
                TypeKey targetKey = cycle[(index + 1) % cycle.Count];
                TypeNodeInfo target = graph.FindType(targetKey);

                if (source == null || target == null)
                    continue;

                if (!source.Outgoing.TryGetValue(targetKey, out int weight) || weight >= lowest)
                    continue;

                lowest = weight;
                best = $"{source.ShortName} -> {target.ShortName} ({DescribeWeight(weight)})";
            }

            return best;
        }

        /// <summary>
        /// True when the loop is a namespace and its own sub namespace inside one assembly. The finding
        /// exists because a cycle blocks splitting code into separate assemblies, and nobody is ever
        /// going to ship a sub folder as its own assembly, so there is nothing here to act on.
        /// </summary>
        private static bool IsFolderPair(CodebaseGraphData graph,
            List<string> component,
            List<string> cycle)
        {
            // Only when the whole tangle is that pair. Judging the shortest loop on its own would drop
            // a real three namespace cycle whenever two of its three edges happen to be parent to child,
            // taking the finding that mattered along with the two that did not.
            if (component.Count != MinimumInterestingCycle || cycle.Count != MinimumInterestingCycle)
                return false;

            if (!IsUnder(cycle[0], cycle[1]) && !IsUnder(cycle[1], cycle[0]))
                return false;

            return ReadAssembly(graph, cycle[0]) == ReadAssembly(graph, cycle[1]);
        }

        private static bool IsUnder(string name, string prefix)
            => name.StartsWith($"{prefix}{NestingSeparator}", StringComparison.Ordinal);

        /// <summary>Returns the assembly a namespace lives in, or null when it is spread across more.</summary>
        private static string ReadAssembly(CodebaseGraphData graph, string name)
        {
            string assembly = null;

            foreach (TypeNodeInfo type in graph.Namespaces[name].Types)
            {
                if (assembly != null && assembly != type.AssemblyName)
                    return null;

                assembly = type.AssemblyName;
            }

            return assembly;
        }

        private static string DescribeNamespaceCycle(List<string> cycle)
        {
            List<string> names = new(cycle)
            {
                cycle[0]
            };

            return string.Join(EdgeSeparator, names);
        }

        private static string SuggestNamespaceCut(CodebaseGraphData graph, List<string> cycle)
        {
            string best = string.Empty;
            int lowest = int.MaxValue;

            for (int index = 0; index < cycle.Count; index++)
            {
                NamespaceNodeInfo source = graph.Namespaces[cycle[index]];
                string target = cycle[(index + 1) % cycle.Count];

                if (!source.Outgoing.TryGetValue(target, out int weight) || weight >= lowest)
                    continue;

                lowest = weight;
                best = $"{source.Name} -> {target} ({DescribeWeight(weight)})";
            }

            return best;
        }

        private static string DescribeWeight(int weight) => weight == 1
            ? SingleUsageText
            : $"{weight}{ManyUsagesSuffix}";

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

        /// <summary>
        /// Builds a key that does not depend on the order the names arrived in. Two of these existed
        /// under different names doing exactly the same thing, one for identifying a cycle and one for
        /// remembering a pair, which are the same question asked twice.
        /// </summary>
        private static string BuildSortedKey(IEnumerable<string> names)
        {
            List<string> sorted = new(names);
            sorted.Sort(StringComparer.Ordinal);

            return string.Join(PairSeparator, sorted);
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

        /// <summary>
        /// Decides whether a loop is worth a line. The ownership test only applies when the whole
        /// component is that pair and nothing else. Judging the tightest loop on its own would let a
        /// forty type tangle go unreported whenever its shortest loop happens to be two types in one
        /// namespace, which is the common case and the opposite of what the filter is for.
        /// </summary>
        private static bool IsReportableCycle(CodebaseGraphData graph,
            List<TypeKey> cycle,
            int componentSize,
            HashSet<string> namespacePairs)
        {
            HashSet<string> namespaces = new();

            foreach (TypeKey key in cycle)
            {
                TypeNodeInfo type = graph.FindType(key);
                if (type != null)
                    namespaces.Add(type.Namespace);
            }

            bool isOwnershipPair = componentSize <= MinimumInterestingCycle
                && cycle.Count <= MinimumInterestingCycle
                && namespaces.Count <= 1;

            if (isOwnershipPair)
                return false;

            if (namespaces.Count == 2)
                namespacePairs.Add(BuildSortedKey(namespaces));

            return true;
        }

        private static void MarkCycleMember(CodebaseGraphData graph,
            List<TypeKey> cycle,
            TypeKey key,
            string cycleId,
            string description,
            string cut,
            int componentSize)
        {
            TypeNodeInfo type = graph.FindType(key);
            if (type == null || type.IsExcludedFromFindings)
                return;

            type.Issues |= ETypeIssue.TypeCycle;
            type.CycleId = cycleId;
            type.CycleDescription = description;
            type.CycleCutHint = cut;
            type.CycleComponentSize = componentSize;

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
                getTargets: name => graph.Namespaces[name].Outgoing.Keys);

            foreach (List<string> component in cycles)
            {
                List<string> cycle = CycleFinder.FindShortestCycle(component,
                    getTargets: name => graph.Namespaces[name].Outgoing.Keys);

                if (cycle.Count < MinimumCycleLength)
                    continue;

                // The same reasoning as for types: only a component that is exactly the pair can be
                // the thing a type cycle already said.
                if (component.Count == 2 && cycle.Count == 2 && namespacePairs.Contains(BuildSortedKey(cycle)))
                    continue;

                if (IsFolderPair(graph, component, cycle))
                    continue;

                string cycleId = BuildSortedKey(cycle);
                string description = DescribeNamespaceCycle(cycle);
                string cut = SuggestNamespaceCut(graph, cycle);

                foreach (string name in cycle)
                {
                    NamespaceNodeInfo group = graph.Namespaces[name];
                    group.CycleId = cycleId;
                    group.CycleDescription = description;
                    group.CycleCutHint = cut;
                    group.CycleComponentSize = component.Count;

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