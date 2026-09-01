using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Keeps the scan result alive between window openings. Metadata tokens only stay valid for one
    /// compilation, so the cache is dropped the moment the domain reloads.
    /// </summary>
    [InitializeOnLoad]
    internal static class CodebaseGraphCache
    {
        private static CodebaseGraphData _graph;

        static CodebaseGraphCache() => AssemblyReloadEvents.beforeAssemblyReload += Clear;

        /// <summary>Returns the cached graph, or null when there is none.</summary>
        /// <returns>The cached graph.</returns>
        internal static CodebaseGraphData Get() => _graph;

        /// <summary>Stores a freshly built graph.</summary>
        /// <param name="graph">The graph to keep.</param>
        internal static void Set(CodebaseGraphData graph) => _graph = graph;

        /// <summary>Drops the cached graph.</summary>
        private static void Clear() => _graph = null;
    }
}