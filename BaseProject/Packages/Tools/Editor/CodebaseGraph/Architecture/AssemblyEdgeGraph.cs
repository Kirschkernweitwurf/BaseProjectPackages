using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Architecture
{
    /// <summary>
    /// The type graph rolled up to assemblies: every weighted edge, plus a lookup by source and target.
    /// Every rule in the architecture analysis reads this and nothing else, so there is one place where
    /// an edge weight is decided and one place to check when a finding looks wrong.
    /// </summary>
    internal sealed class AssemblyEdgeGraph
    {
        /// <summary>Every edge found, sorted by source then target.</summary>
        internal IReadOnlyList<AssemblyEdgeInfo> Edges { get; }

        /// <summary>Names of every assembly that holds at least one scanned type, sorted.</summary>
        internal IReadOnlyList<string> Assemblies { get; }

        private readonly Dictionary<AssemblyEdgeKey, AssemblyEdgeInfo> _byKey;
        private readonly Dictionary<string, int> _typeCounts;

        /// <summary>Creates the rolled up graph.</summary>
        /// <param name="edges">Every edge, already sorted.</param>
        /// <param name="assemblies">Every assembly that holds scanned types, already sorted.</param>
        /// <param name="typeCounts">How many top level types each assembly declares.</param>
        internal AssemblyEdgeGraph(IReadOnlyList<AssemblyEdgeInfo> edges,
            IReadOnlyList<string> assemblies,
            Dictionary<string, int> typeCounts)
        {
            Edges = edges;
            Assemblies = assemblies;
            _typeCounts = typeCounts;
            _byKey = new Dictionary<AssemblyEdgeKey, AssemblyEdgeInfo>(edges.Count);

            foreach (AssemblyEdgeInfo edge in edges)
                _byKey[edge.Key] = edge;
        }

        /// <summary>Returns the edge between two assemblies, or null when there is none.</summary>
        /// <param name="source">Name of the depending assembly.</param>
        /// <param name="target">Name of the depended upon assembly.</param>
        /// <returns>The edge, or null.</returns>
        internal AssemblyEdgeInfo Find(string source, string target)
            => _byKey.GetValueOrDefault(new AssemblyEdgeKey(source, target));

        /// <summary>Returns how many top level types an assembly declares.</summary>
        /// <param name="assembly">Name of the assembly.</param>
        /// <returns>The number of types, or zero when the assembly was not scanned.</returns>
        internal int CountTypes(string assembly) => _typeCounts.GetValueOrDefault(assembly);
    }
}