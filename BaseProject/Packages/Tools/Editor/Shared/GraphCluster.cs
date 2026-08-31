using System.Collections.Generic;

namespace Base.ToolPackage.Editor.Shared
{
    /// <summary>
    /// One group of nodes that are reachable from each other, arranged into the columns they are
    /// drawn in. Column zero holds the nodes that depend on nothing else in the group, and each
    /// column after it holds nodes that depend on the one before, so a cluster reads left to right in
    /// the order it would have to be built.
    /// </summary>
    internal sealed class GraphCluster
    {
        /// <summary>The columns, left to right. Within a column the order is the drawing order.</summary>
        internal IReadOnlyList<IReadOnlyList<string>> Columns { get; }

        /// <summary>How many nodes the cluster holds in all.</summary>
        internal int Count { get; }

        /// <summary>Creates a cluster from its finished columns.</summary>
        /// <param name="columns">The columns, left to right.</param>
        internal GraphCluster(List<List<string>> columns)
        {
            Columns = columns;

            int total = 0;

            foreach (List<string> column in columns)
                total += column.Count;

            Count = total;
        }
    }
}