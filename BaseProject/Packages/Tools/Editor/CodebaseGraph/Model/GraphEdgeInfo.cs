namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One relation the graph draws, and how much traffic it carries.</summary>
    internal sealed class GraphEdgeInfo
    {
        /// <summary>ID of the entry this relation points at.</summary>
        internal string TargetId { get; }

        /// <summary>How many usages back the relation up, which sets how heavy the line is drawn.</summary>
        internal int Weight { get; }

        /// <summary>Creates a relation.</summary>
        /// <param name="targetId">ID of the entry being pointed at.</param>
        /// <param name="weight">How many usages back it up.</param>
        public GraphEdgeInfo(string targetId, int weight)
        {
            TargetId = targetId;
            Weight = weight;
        }
    }
}