using System;
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.Shared
{
    /// <summary>
    /// Turns a dependency graph into the arrangement a layered drawing needs: which nodes belong
    /// together, which column each one sits in, and in what order they sit within a column.
    /// </summary>
    /// <remarks>
    /// Nothing here knows what a node is or how big it is drawn. It works on ids and on the ids each
    /// one points at, and it answers in ids, which leaves every question of size, position and
    /// appearance to the tool that asked. Two graph windows arrived at the same four steps
    /// independently, and those four steps are what this holds.
    /// <para>
    /// The steps are: split into groups that can reach each other, give every node the length of the
    /// longest path from it down to something with no dependencies, bucket the nodes by that length,
    /// and then reorder within each bucket a few times so fewer edges cross. The last step is a
    /// heuristic and makes no promise beyond usually looking better than not doing it.
    /// </para>
    /// </remarks>
    internal sealed class GraphLayering
    {
        /// <summary>
        /// How often the columns are reordered. Each sweep alternates direction, and the gain falls
        /// off quickly, so a handful is all it is worth.
        /// </summary>
        private const int OrderingSweeps = 6;

        /// <summary>The connected groups, largest first, so the biggest lands at the top of a drawing.</summary>
        internal IReadOnlyList<GraphCluster> Clusters { get; }

        /// <summary>
        /// The nodes with no edge at all, in name order. They belong to no cluster and are usually
        /// drawn as a block of their own rather than being scattered through the layout.
        /// </summary>
        internal IReadOnlyList<string> Isolated { get; }

        private static readonly IReadOnlyList<string> NoTargets = Array.Empty<string>();

        private GraphLayering(List<GraphCluster> clusters, List<string> isolated)
        {
            Clusters = clusters;
            Isolated = isolated;
        }

        /// <summary>
        /// Arranges a graph.
        /// </summary>
        /// <param name="ids">Every node, in the order they should be walked. Duplicates are ignored.</param>
        /// <param name="outgoing">
        /// What each node points at. An id with no entry is treated as pointing at nothing, and a
        /// target that is not itself in <paramref name="ids"/> is ignored.
        /// </param>
        /// <returns>The clusters and the isolated nodes.</returns>
        internal static GraphLayering Build(IReadOnlyList<string> ids,
            IReadOnlyDictionary<string, List<string>> outgoing)
        {
            List<GraphCluster> clusters = new();
            List<string> isolated = new();

            if (ids == null || ids.Count == 0 || outgoing == null)
                return new GraphLayering(clusters, isolated);

            // Everything below assumes an edge always points at a node that is present. Establishing
            // that once here is what lets the walk trust the map instead of re-checking every hop.
            Dictionary<string, List<string>> edges = Normalize(ids, outgoing);
            Dictionary<string, List<string>> incoming = BuildIncoming(ids, edges);
            List<List<string>> groups = SplitGroups(ids, edges, incoming, isolated);

            isolated.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (List<string> group in groups)
            {
                Dictionary<string, int> levels = ComputeLevels(group, edges);
                List<List<string>> columns = BuildColumns(group, levels);

                ReduceCrossings(columns, edges, incoming, levels);

                clusters.Add(new GraphCluster(columns));
            }

            clusters.Sort(comparison: static (left, right) => right.Count.CompareTo(left.Count));

            return new GraphLayering(clusters, isolated);
        }

        private static IReadOnlyList<string> Targets(IReadOnlyDictionary<string, List<string>> edges, string id)
            => edges.TryGetValue(id, out List<string> targets)
                ? targets
                : NoTargets;

        /// <summary>
        /// Copies the edge map keeping only edges that lead somewhere real: no self edge, no target
        /// outside the given nodes, and no duplicate. Every node gets an entry, so a later lookup
        /// never has to ask whether the node is known.
        /// </summary>
        private static Dictionary<string, List<string>> Normalize(IReadOnlyList<string> ids,
            IReadOnlyDictionary<string, List<string>> outgoing)
        {
            Dictionary<string, List<string>> edges = new();

            foreach (string id in ids)
                edges[id] = new List<string>();

            foreach (string id in ids)
            {
                List<string> targets = edges[id];

                foreach (string target in Targets(outgoing, id))
                {
                    if (target == id || !edges.ContainsKey(target) || targets.Contains(target))
                        continue;

                    targets.Add(target);
                }
            }

            return edges;
        }

        // Every edge read backwards. The reordering pass needs both directions, because it looks at
        // the column in front on one sweep and the column behind on the next.
        private static Dictionary<string, List<string>> BuildIncoming(IReadOnlyList<string> ids,
            IReadOnlyDictionary<string, List<string>> outgoing)
        {
            Dictionary<string, List<string>> incoming = new();

            foreach (string id in ids)
                incoming[id] = new List<string>();

            foreach (string id in ids)
            {
                foreach (string target in Targets(outgoing, id))
                {
                    if (incoming.TryGetValue(target, out List<string> sources))
                        sources.Add(id);
                }
            }

            return incoming;
        }

        private static List<List<string>> SplitGroups(IReadOnlyList<string> ids,
            IReadOnlyDictionary<string, List<string>> outgoing,
            IReadOnlyDictionary<string, List<string>> incoming, List<string> isolated)
        {
            List<List<string>> groups = new();
            HashSet<string> visited = new();

            foreach (string id in ids)
            {
                if (visited.Contains(id))
                    continue;

                if (Targets(outgoing, id).Count == 0 && Targets(incoming, id).Count == 0)
                {
                    visited.Add(id);
                    isolated.Add(id);

                    continue;
                }

                groups.Add(Flood(id, outgoing, incoming, visited));
            }

            return groups;
        }

        // Both directions are followed, so a group is everything that hangs together at all rather
        // than only what one node can reach by walking its dependencies downwards.
        private static List<string> Flood(string start, IReadOnlyDictionary<string, List<string>> outgoing,
            IReadOnlyDictionary<string, List<string>> incoming, HashSet<string> visited)
        {
            List<string> group = new();
            Queue<string> pending = new();

            pending.Enqueue(start);
            visited.Add(start);

            while (pending.Count > 0)
            {
                string current = pending.Dequeue();
                group.Add(current);

                Enqueue(Targets(outgoing, current), visited, pending);
                Enqueue(Targets(incoming, current), visited, pending);
            }

            return group;
        }

        private static void Enqueue(IReadOnlyList<string> neighbors, HashSet<string> visited, Queue<string> pending)
        {
            foreach (string neighbor in neighbors)
            {
                if (visited.Add(neighbor))
                    pending.Enqueue(neighbor);
            }
        }

        /// <summary>
        /// Walks the group with an explicit stack rather than by recursion, because a dependency
        /// chain in a real project gets deep enough that the call stack is a real risk.
        /// </summary>
        private static Dictionary<string, int> ComputeLevels(List<string> group,
            IReadOnlyDictionary<string, List<string>> outgoing)
        {
            HashSet<string> members = new(group);
            Dictionary<string, int> levels = new();
            HashSet<string> onStack = new();
            Stack<string> pending = new();

            foreach (string root in group)
            {
                if (levels.ContainsKey(root))
                    continue;

                pending.Push(root);

                while (pending.Count > 0)
                {
                    string current = pending.Peek();

                    if (levels.ContainsKey(current))
                    {
                        pending.Pop();
                        onStack.Remove(current);

                        continue;
                    }

                    if (onStack.Add(current))
                    {
                        PushUnresolved(current, outgoing, members, levels, onStack, pending);

                        continue;
                    }

                    // Second visit: every target that could be resolved now has a level.
                    pending.Pop();
                    onStack.Remove(current);
                    levels[current] = MeasureLevel(current, outgoing, members, levels);
                }
            }

            return levels;
        }

        private static void PushUnresolved(string id, IReadOnlyDictionary<string, List<string>> outgoing,
            HashSet<string> members, Dictionary<string, int> levels, HashSet<string> onStack, Stack<string> pending)
        {
            foreach (string target in Targets(outgoing, id))
            {
                // A target already on the stack closes a cycle, which simply stops the walk there.
                if (!members.Contains(target) || levels.ContainsKey(target) || onStack.Contains(target))
                    continue;

                pending.Push(target);
            }
        }

        private static int MeasureLevel(string id, IReadOnlyDictionary<string, List<string>> outgoing,
            HashSet<string> members, Dictionary<string, int> levels)
        {
            int level = 0;

            foreach (string target in Targets(outgoing, id))
            {
                if (!members.Contains(target) || !levels.TryGetValue(target, out int targetLevel))
                    continue;

                level = Math.Max(level, targetLevel + 1);
            }

            return level;
        }

        // Sorted by name before bucketing, so a whole column comes out alphabetically when the
        // reordering pass has no edges to go on.
        private static List<List<string>> BuildColumns(List<string> group, Dictionary<string, int> levels)
        {
            int maxLevel = 0;

            foreach (string id in group)
                maxLevel = Math.Max(maxLevel, levels[id]);

            List<List<string>> columns = new(maxLevel + 1);

            for (int index = 0; index <= maxLevel; index++)
                columns.Add(new List<string>());

            List<string> sorted = new(group);
            sorted.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string id in sorted)
                columns[levels[id]].Add(id);

            return columns;
        }

        private static void ReduceCrossings(List<List<string>> columns,
            IReadOnlyDictionary<string, List<string>> outgoing,
            IReadOnlyDictionary<string, List<string>> incoming, Dictionary<string, int> levels)
        {
            for (int sweep = 0; sweep < OrderingSweeps; sweep++)
            {
                bool isForward = sweep % 2 == 0;

                Dictionary<string, int> ranks = BuildRanks(columns);

                IReadOnlyDictionary<string, List<string>> neighbors = isForward
                    ? outgoing
                    : incoming;

                for (int step = 0; step < columns.Count; step++)
                {
                    int level = isForward
                        ? step
                        : columns.Count - 1 - step;

                    SortColumn(columns[level], neighbors, ranks, levels, level, isForward);

                    // Only the column that just moved has new positions, so only it needs reindexing.
                    UpdateRanks(columns[level], ranks);
                }
            }
        }

        private static Dictionary<string, int> BuildRanks(List<List<string>> columns)
        {
            Dictionary<string, int> ranks = new();

            foreach (List<string> column in columns)
                UpdateRanks(column, ranks);

            return ranks;
        }

        private static void UpdateRanks(List<string> column, Dictionary<string, int> ranks)
        {
            for (int index = 0; index < column.Count; index++)
                ranks[column[index]] = index;
        }

        private static void SortColumn(List<string> column,
            IReadOnlyDictionary<string, List<string>> neighbors, Dictionary<string, int> ranks,
            Dictionary<string, int> levels, int level, bool isForward)
        {
            Dictionary<string, float> keys = new();

            foreach (string id in column)
                keys[id] = ComputeMedian(id, neighbors, ranks, levels, level, isForward);

            column.Sort(comparison: (left, right) =>
            {
                int compared = keys[left].CompareTo(keys[right]);

                return compared != 0
                    ? compared
                    : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
            });
        }

        /// <summary>
        /// Average rank of the neighbors in the sweep direction. A node with nothing to line up
        /// against keeps the rank it already had, so it does not drift.
        /// </summary>
        private static float ComputeMedian(string id, IReadOnlyDictionary<string, List<string>> neighbors,
            Dictionary<string, int> ranks, Dictionary<string, int> levels, int level, bool isForward)
        {
            float sum = 0f;
            int count = 0;

            foreach (string neighbor in Targets(neighbors, id))
            {
                if (!levels.TryGetValue(neighbor, out int neighborLevel))
                    continue;

                bool isAnchor = isForward
                    ? neighborLevel < level
                    : neighborLevel > level;

                if (!isAnchor)
                    continue;

                sum += ranks[neighbor];
                count++;
            }

            return count == 0
                ? ranks[id]
                : sum / count;
        }
    }
}