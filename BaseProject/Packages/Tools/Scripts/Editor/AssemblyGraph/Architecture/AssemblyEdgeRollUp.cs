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
    /// </summary>
    internal static class AssemblyEdgeRollUp
    {
        /// <summary>Rolls a scan result up to weighted assembly edges.</summary>
        /// <param name="graph">The scan result to read. A null graph yields an empty result.</param>
        /// <returns>The assembly level graph.</returns>
        public static AssemblyEdgeGraph Build(CodebaseGraphData graph)
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

                AssemblyEdgeKey key = new(source.AssemblyName, target.AssemblyName);

                if (!builders.TryGetValue(key, out EdgeBuilder builder))
                {
                    builder = new EdgeBuilder(key);
                    builders[key] = builder;
                }

                builder.Add(ResolveOutermost(graph, source, outermost).FullName,
                    ResolveOutermost(graph, target, outermost).FullName,
                    usage.Value,
                    source.IsExcludedFromFindings);
            }
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
            private readonly SortedSet<string> _sourceTypes = new(StringComparer.Ordinal);

            private int _usageCount;
            private bool _hasIncludedSource;

            public EdgeBuilder(AssemblyEdgeKey key) => _key = key;

            /// <summary>Records one type level usage that crosses this edge.</summary>
            /// <param name="sourceTypeName">Outermost source type reaching across.</param>
            /// <param name="targetTypeName">Outermost target type being reached.</param>
            /// <param name="usageCount">How many member level usages back this pair up.</param>
            /// <param name="isSourceExcluded">Whether the source is generated, sample or test code.</param>
            public void Add(string sourceTypeName, string targetTypeName, int usageCount, bool isSourceExcluded)
            {
                _sourceTypes.Add(sourceTypeName);
                _targetTypes.Add(targetTypeName);
                _usageCount += usageCount;

                if (!isSourceExcluded)
                    _hasIncludedSource = true;
            }

            /// <summary>Freezes the gathered data into an immutable edge.</summary>
            /// <returns>The finished edge.</returns>
            public AssemblyEdgeInfo ToEdge() => new(_key,
                new List<string>(_targetTypes),
                new List<string>(_sourceTypes),
                _usageCount,
                !_hasIncludedSource);
        }
    }
}