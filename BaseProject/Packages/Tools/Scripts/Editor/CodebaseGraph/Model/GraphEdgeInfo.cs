namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One relation the graph draws, and how much traffic it carries.</summary>
    public sealed class GraphEdgeInfo
    {
        /// <summary>Id of the entry this relation points at.</summary>
        public string TargetId { get; }

        /// <summary>How many usages back the relation up, which sets how heavy the line is drawn.</summary>
        public int Weight { get; }

        /// <summary>Creates a relation.</summary>
        /// <param name="targetId">Id of the entry being pointed at.</param>
        /// <param name="weight">How many usages back it up.</param>
        public GraphEdgeInfo(string targetId, int weight)
        {
            TargetId = targetId;
            Weight = weight;
        }
    }
}
