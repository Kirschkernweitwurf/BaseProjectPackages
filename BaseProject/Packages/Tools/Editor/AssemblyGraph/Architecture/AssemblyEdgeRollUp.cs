using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.AssemblyGraph.Architecture
{
    /// <summary>
    /// Rolls the scanned type graph up to assemblies. The Codebase Graph already records, for every
    /// type, which other types it uses and how often. Grouping those by declaring assembly turns them
    /// into weighted assembly edges together with the exact type list behind each one, without a second
    /// scan of anything.
    /// <br/><br/>
    /// Two decisions here shape every rule built on top, so they are worth knowing before reading a
    /// finding. Nested types fold into the outermost type that declares them, because a helper struct
    /// inside a class is not a second reason for the dependency to exist. And an edge is only counted
    /// once per distinct target type, however many call sites there are, because the question a rule
    /// asks is how much of the other assembly this one actually needs.
    /// <br/><br/>
    /// The third decision is what an inherited interface counts as. The scan records the relation from
    /// <c>Type.GetInterfaces</c>, which returns everything a base type carries as well as what the type
    /// declares itself, so a subclass looks like it reaches an interface it never names. The compiler
    /// needs no reference for that, which is why an edge built from it lands in the report's
    /// "no declared reference" section. Those relations are dropped here rather than in the scanner,
    /// where the full set is what keeps interface members off the dead code list.
    /// </summary>
    internal static class AssemblyEdgeRollUp
    {
        /// <summary>How many relations a purely inherited interface leaves behind: the inheritance one.</summary>
        private const int InheritedInterfaceRelationCount = 1;

        /// <summary>Rolls a scan result up to weighted assembly edges.</summary>
        /// <param name="graph">The scan result to read. A null graph yields an empty result.</param>
        /// <returns>The assembly level graph.</returns>
        internal static AssemblyEdgeGraph Build(CodebaseGraphData graph)
        {
            Dictionary<AssemblyEdgeKey, EdgeBuilder> builders = new();
            Dictionary<TypeKey, TypeNodeInfo> outermost = new();
            Dictionary<string, int> typeCounts = new(StringComparer.Ordinal);
            SortedSet<string> assemblies = new(StringComparer.Ordinal);

            if (graph == null)
                return new AssemblyEdgeGraph(Array.Empty<AssemblyEdgeInfo>(), Array.Empty<string>(), typeCounts);

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                assemblies.Add(type.AssemblyName);

                if (ResolveOutermost(graph, type, outermost) == type)
                    Increment(typeCounts, type.AssemblyName);
            }

            foreach (TypeNodeInfo source in graph.Types.Values)
                CollectFrom(graph, source, outermost, builders);

            return new AssemblyEdgeGraph(BuildEdges(builders), ToSortedList(assemblies), typeCounts);
        }

        private static void CollectFrom(CodebaseGraphData graph,
            TypeNodeInfo source,
            Dictionary<TypeKey, TypeNodeInfo> outermost,
            Dictionary<AssemblyEdgeKey, EdgeBuilder> builders)
        {
            foreach (KeyValuePair<TypeKey, int> usage in source.Outgoing)
            {
                TypeNodeInfo target = graph.FindType(usage.Key);

                if (target == null)
                    continue;

                // A nested type always lives in the assembly of the type declaring it, so comparing
                // before folding is the same answer for less work.
                if (string.Equals(source.AssemblyName, target.AssemblyName, StringComparison.Ordinal))
                    continue;

                if (IsInheritedInterface(graph, source, target, usage.Value))
                    continue;

                AssemblyEdgeKey key = new(source.AssemblyName, target.AssemblyName);

                if (!builders.TryGetValue(key, out EdgeBuilder builder))
                {
                    builder = new EdgeBuilder(key);
                    builders[key] = builder;
                }

                builder.Add(ResolveOutermost(graph, target, outermost).FullName,
                    usage.Value,
                    source.IsExcludedFromFindings);
            }
        }

        /// <summary>
        /// Whether the only thing behind this relation is an interface the source's base type already
        /// carries. The source names nothing from the interface's assembly in that case, so the compiler
        /// requires no reference to it and neither should a rule.
        /// <para>
        /// A source that uses the interface itself has more than the single inheritance relation behind
        /// the count, which is what keeps a real usage from being dropped with the inherited one. The
        /// case this cannot separate is a subclass that re-declares an interface its base already
        /// implements: that names the interface and does need the reference, but leaves the same one
        /// relation behind, so the edge is dropped. The report then agrees with the asmdef, which is
        /// the safe direction to be wrong in.
        /// </para>
        /// </summary>
        /// <param name="graph">The scan result the base type is looked up in.</param>
        /// <param name="source">The type the relation starts at.</param>
        /// <param name="target">The type the relation points at.</param>
        /// <param name="usageCount">How many relations the scan recorded for this pair.</param>
        /// <returns>True when the relation is inherited and must not become an edge.</returns>
        private static bool IsInheritedInterface(CodebaseGraphData graph,
            TypeNodeInfo source,
            TypeNodeInfo target,
            int usageCount)
        {
            if (target.Kind != ETypeKind.Interface)
                return false;

            if (usageCount != InheritedInterfaceRelationCount)
                return false;

            if (!source.BaseTypeKey.IsValid)
                return false;

            TypeNodeInfo baseType = graph.FindType(source.BaseTypeKey);

            // A base outside the scan cannot answer the question, so the edge is kept rather than
            // guessed away. That is the one case where an inherited interface still reaches the report.
            if (baseType == null)
                return false;

            // The base recorded its own inherited set as well, so one hop covers the whole chain.
            return baseType.Outgoing.ContainsKey(target.Key);
        }

        /// <summary>Walks outward through nested types until the type that declares them all is reached.</summary>
        private static TypeNodeInfo ResolveOutermost(CodebaseGraphData graph,
            TypeNodeInfo type,
            Dictionary<TypeKey, TypeNodeInfo> cache)
        {
            if (cache.TryGetValue(type.Key, out TypeNodeInfo cached))
                return cached;

            TypeNodeInfo current = type;

            while (current.DeclaringTypeKey.IsValid)
            {
                TypeNodeInfo outer = graph.FindType(current.DeclaringTypeKey);

                // A nested type whose owner was not scanned is as far out as this walk can go.
                if (outer == null)
                    break;

                current = outer;
            }

            cache[type.Key] = current;

            return current;
        }

        private static void Increment(Dictionary<string, int> counts, string assemblyName)
        {
            counts.TryGetValue(assemblyName, out int count);
            counts[assemblyName] = count + 1;
        }

        private static List<AssemblyEdgeInfo> BuildEdges(Dictionary<AssemblyEdgeKey, EdgeBuilder> builders)
        {
            List<AssemblyEdgeInfo> edges = new(builders.Count);

            foreach (EdgeBuilder builder in builders.Values)
                edges.Add(builder.ToEdge());

            edges.Sort(comparison: static (left, right) =>
            {
                int bySource = string.Compare(left.SourceName, right.SourceName, StringComparison.Ordinal);

                return bySource != 0
                    ? bySource
                    : string.Compare(left.TargetName, right.TargetName, StringComparison.Ordinal);
            });

            return edges;
        }

        private static List<string> ToSortedList(SortedSet<string> names) => new(names);

        /// <summary>Gathers one edge while the type graph is being walked.</summary>
        private sealed class EdgeBuilder
        {
            private readonly AssemblyEdgeKey _key;
            private readonly SortedSet<string> _targetTypes = new(StringComparer.Ordinal);

            private int _usageCount;
            private bool _hasIncludedSource;

            /// <summary>Starts collecting the usages that cross one edge.</summary>
            /// <param name="key">The source and target assemblies the edge runs between.</param>
            internal EdgeBuilder(AssemblyEdgeKey key) => _key = key;

            /// <summary>Records one type level usage that crosses this edge.</summary>
            /// <param name="targetTypeName">Outermost target type being reached.</param>
            /// <param name="usageCount">How many member level usages back this pair up.</param>
            /// <param name="isSourceExcluded">Whether the source is generated, sample or test code.</param>
            internal void Add(string targetTypeName, int usageCount, bool isSourceExcluded)
            {
                _targetTypes.Add(targetTypeName);
                _usageCount += usageCount;

                if (!isSourceExcluded)
                    _hasIncludedSource = true;
            }

            /// <summary>Freezes the gathered data into an immutable edge.</summary>
            /// <returns>The finished edge.</returns>
            internal AssemblyEdgeInfo ToEdge() => new(_key,
                new List<string>(_targetTypes),
                _usageCount,
                !_hasIncludedSource);
        }
    }
}