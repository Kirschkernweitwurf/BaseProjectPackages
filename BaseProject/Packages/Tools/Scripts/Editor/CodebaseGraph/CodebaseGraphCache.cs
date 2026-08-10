using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Keeps the scan result alive between window openings. Metadata tokens only stay valid for one
    /// compilation, so the cache is dropped the moment the domain reloads.
    /// </summary>
    [InitializeOnLoad]
    public static class CodebaseGraphCache
    {
        private static CodebaseGraphData _graph;
        private static HashSet<string> _findingIds;

        static CodebaseGraphCache() => AssemblyReloadEvents.beforeAssemblyReload += Clear;

        /// <summary>Returns the cached graph, or null when there is none.</summary>
        /// <returns>The cached graph.</returns>
        public static CodebaseGraphData Get() => _graph;

        /// <summary>Stores a freshly built graph.</summary>
        /// <param name="graph">The graph to keep.</param>
        public static void Set(CodebaseGraphData graph) => _graph = graph;

        /// <summary>
        /// Takes the finding ids of the last scan and hands them over. They are read once, by the scan
        /// that is about to replace them, which is the only thing that ever wants them.
        /// </summary>
        /// <returns>The previous ids, or null on the first scan of a session.</returns>
        public static HashSet<string> TakeFindingIds()
        {
            HashSet<string> previous = _findingIds;
            _findingIds = null;

            return previous;
        }

        /// <summary>Stores the finding ids of a freshly built graph.</summary>
        /// <param name="ids">Ids the scan raised.</param>
        public static void SetFindingIds(HashSet<string> ids) => _findingIds = ids;

        /// <summary>Drops the cached graph.</summary>
        public static void Clear()
        {
            _graph = null;
            _findingIds = null;
        }
    }
}
