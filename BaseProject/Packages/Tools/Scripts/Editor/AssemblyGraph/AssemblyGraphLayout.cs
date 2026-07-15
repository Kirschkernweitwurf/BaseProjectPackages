using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>
    /// Calculates node positions. Assemblies are split into connected clusters, layered by dependency
    /// depth, and ordered to reduce edge crossings. Orphans dock to the cluster that shares their root
    /// name. Orphans without any relative form their own horizontal block above the clusters.
    /// </summary>
    public static class AssemblyGraphLayout
    {
        private const float BaseNodeHeight = 108f;
        private const float ClusterGap = 140f;
        private const float ColumnGap = 110f;
        private const float HomelessBlockGap = 200f;
        private const int HomelessColumns = 6;
        private const float NodeWidth = 260f;
        private const int OrderingSweeps = 6;
        private const float OrphanColumnGap = 70f;
        private const float RowGap = 34f;
        private const float UnusedHeaderHeight = 46f;
        private const float UnusedLineHeight = 15f;

        /// <summary>Returns a placement rect for every given node.</summary>
        public static Dictionary<string, Rect> Calculate(IReadOnlyList<AssemblyNodeInfo> nodes)
        {
            Dictionary<string, Rect> result = new();
            if (nodes == null || nodes.Count == 0)
                return result;

            Dictionary<string, AssemblyNodeInfo> byName = BuildLookup(nodes);
            Dictionary<string, List<string>> outgoing = BuildOutgoing(nodes, byName);
            Dictionary<string, List<string>> incoming = BuildIncoming(outgoing);

            List<List<string>> clusters = FindClusters(nodes, outgoing, incoming);
            List<string> orphans = ExtractOrphans(clusters, outgoing, incoming);

            clusters.Sort((left, right) => right.Count.CompareTo(left.Count));

            List<List<string>> dockedOrphans = DockOrphans(orphans, clusters, byName, out List<string> homeless);

            PlaceHomelessBlock(homeless, byName, result);

            float cursorY = 0f;
            for (int i = 0; i < clusters.Count; i++)
            {
                Vector2 size = PlaceCluster(clusters[i], dockedOrphans[i], byName, outgoing, incoming, cursorY, result);
                cursorY += size.y + ClusterGap;
            }

            return result;
        }

        private static Dictionary<string, AssemblyNodeInfo> BuildLookup(IReadOnlyList<AssemblyNodeInfo> nodes)
        {
            Dictionary<string, AssemblyNodeInfo> byName = new();
            foreach (AssemblyNodeInfo info in nodes)
                byName[info.Name] = info;

            return byName;
        }

        private static Dictionary<string, List<string>> BuildOutgoing(IReadOnlyList<AssemblyNodeInfo> nodes,
            Dictionary<string, AssemblyNodeInfo> byName)
        {
            Dictionary<string, List<string>> outgoing = new();

            foreach (AssemblyNodeInfo info in nodes)
            {
                List<string> targets = new();

                foreach (AssemblyReferenceInfo reference in info.References)
                {
                    if (reference.TargetName == info.Name)
                        continue;

                    if (!byName.ContainsKey(reference.TargetName))
                        continue;

                    if (targets.Contains(reference.TargetName))
                        continue;

                    targets.Add(reference.TargetName);
                }

                outgoing[info.Name] = targets;
            }

            return outgoing;
        }

        private static Dictionary<string, List<string>> BuildIncoming(Dictionary<string, List<string>> outgoing)
        {
            Dictionary<string, List<string>> incoming = new();
            foreach (string name in outgoing.Keys)
                incoming[name] = new List<string>();

            foreach (KeyValuePair<string, List<string>> entry in outgoing)
            {
                foreach (string target in entry.Value)
                    incoming[target].Add(entry.Key);
            }

            return incoming;
        }

        /// <summary>Groups nodes into weakly connected clusters.</summary>
        private static List<List<string>> FindClusters(IReadOnlyList<AssemblyNodeInfo> nodes,
            Dictionary<string, List<string>> outgoing,
            Dictionary<string, List<string>> incoming)
        {
            List<List<string>> clusters = new();
            HashSet<string> visited = new();

            foreach (AssemblyNodeInfo info in nodes)
            {
                if (!visited.Add(info.Name))
                    continue;

                List<string> cluster = new();
                Queue<string> queue = new();
                queue.Enqueue(info.Name);

                while (queue.Count > 0)
                {
                    string current = queue.Dequeue();
                    cluster.Add(current);

                    EnqueueUnvisited(outgoing[current], visited, queue);
                    EnqueueUnvisited(incoming[current], visited, queue);
                }

                clusters.Add(cluster);
            }

            return clusters;
        }

        private static void EnqueueUnvisited(List<string> neighbors, HashSet<string> visited, Queue<string> queue)
        {
            foreach (string neighbor in neighbors)
                if (visited.Add(neighbor))
                    queue.Enqueue(neighbor);
        }

        /// <summary>Pulls fully unconnected nodes out of the cluster list.</summary>
        private static List<string> ExtractOrphans(List<List<string>> clusters,
            Dictionary<string, List<string>> outgoing,
            Dictionary<string, List<string>> incoming)
        {
            List<string> orphans = new();

            for (int i = clusters.Count - 1; i >= 0; i--)
            {
                List<string> cluster = clusters[i];
                if (cluster.Count != 1)
                    continue;

                string name = cluster[0];
                if (outgoing[name].Count > 0 || incoming[name].Count > 0)
                    continue;

                orphans.Add(name);
                clusters.RemoveAt(i);
            }

            orphans.Sort(StringComparer.OrdinalIgnoreCase);
            return orphans;
        }

        /// <summary>
        /// Assigns every orphan to the cluster that holds the most assemblies sharing its root name.
        /// Orphans without any relative end up in the homeless list.
        /// </summary>
        private static List<List<string>> DockOrphans(List<string> orphans,
            List<List<string>> clusters,
            Dictionary<string, AssemblyNodeInfo> byName,
            out List<string> homeless)
        {
            List<List<string>> docked = new(clusters.Count);
            for (int i = 0; i < clusters.Count; i++)
                docked.Add(new List<string>());

            Dictionary<string, int[]> rootCounts = BuildRootCounts(clusters, byName);
            homeless = new List<string>();

            foreach (string orphan in orphans)
            {
                int target = FindBestCluster(byName[orphan].RootName, rootCounts);

                if (target < 0)
                {
                    homeless.Add(orphan);
                    continue;
                }

                docked[target].Add(orphan);
            }

            return docked;
        }

        private static Dictionary<string, int[]> BuildRootCounts(List<List<string>> clusters,
            Dictionary<string, AssemblyNodeInfo> byName)
        {
            Dictionary<string, int[]> rootCounts = new();

            for (int i = 0; i < clusters.Count; i++)
            {
                foreach (string name in clusters[i])
                {
                    string root = byName[name].RootName;

                    if (!rootCounts.TryGetValue(root, out int[] counts))
                    {
                        counts = new int[clusters.Count];
                        rootCounts[root] = counts;
                    }

                    counts[i]++;
                }
            }

            return rootCounts;
        }

        private static int FindBestCluster(string rootName, Dictionary<string, int[]> rootCounts)
        {
            if (!rootCounts.TryGetValue(rootName, out int[] counts))
                return -1;

            int best = -1;
            int bestCount = 0;

            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] <= bestCount)
                    continue;

                bestCount = counts[i];
                best = i;
            }

            return best;
        }

        /// <summary>Lays out a single cluster plus its docked orphans and returns the bounding size.</summary>
        private static Vector2 PlaceCluster(List<string> cluster,
            List<string> dockedOrphans,
            Dictionary<string, AssemblyNodeInfo> byName,
            Dictionary<string, List<string>> outgoing,
            Dictionary<string, List<string>> incoming,
            float offsetY,
            Dictionary<string, Rect> result)
        {
            Dictionary<string, int> levels = ComputeLevels(cluster, outgoing);
            List<List<string>> columns = BuildColumns(cluster, levels);

            ReduceCrossings(columns, outgoing, incoming, levels);

            float orphanHeight = MeasureColumnHeight(dockedOrphans, byName);
            float bandHeight = Mathf.Max(MeasureTallestColumn(columns, byName), orphanHeight);

            for (int level = 0; level < columns.Count; level++)
            {
                float columnHeight = MeasureColumnHeight(columns[level], byName);
                float x = level * (NodeWidth + ColumnGap);
                PlaceColumn(columns[level], byName, x, offsetY + (bandHeight - columnHeight) * 0.5f, result);
            }

            float width = columns.Count * (NodeWidth + ColumnGap) - ColumnGap;

            if (dockedOrphans.Count > 0)
            {
                float orphanX = width + OrphanColumnGap;
                PlaceColumn(dockedOrphans, byName, orphanX, offsetY + (bandHeight - orphanHeight) * 0.5f, result);
                width = orphanX + NodeWidth;
            }

            return new Vector2(width, bandHeight);
        }

        /// <summary>
        /// Places orphans without relatives in their own horizontal block above the clusters,
        /// so they read as a separate group.
        /// </summary>
        private static void PlaceHomelessBlock(List<string> homeless,
            Dictionary<string, AssemblyNodeInfo> byName,
            Dictionary<string, Rect> result)
        {
            if (homeless.Count == 0)
                return;

            List<List<string>> rows = BuildGridRows(homeless);
            float startY = -(MeasureGridHeight(rows, byName) + HomelessBlockGap);

            foreach (List<string> row in rows)
            {
                float rowHeight = MeasureTallestNode(row, byName);

                for (int column = 0; column < row.Count; column++)
                {
                    float x = column * (NodeWidth + ColumnGap);
                    result[row[column]] = new Rect(x, startY, NodeWidth, EstimateHeight(byName[row[column]]));
                }

                startY += rowHeight + RowGap;
            }
        }

        private static List<List<string>> BuildGridRows(List<string> names)
        {
            List<List<string>> rows = new();

            for (int i = 0; i < names.Count; i++)
            {
                if (i % HomelessColumns == 0)
                    rows.Add(new List<string>());

                rows[rows.Count - 1].Add(names[i]);
            }

            return rows;
        }

        private static float MeasureGridHeight(List<List<string>> rows, Dictionary<string, AssemblyNodeInfo> byName)
        {
            float total = 0f;
            foreach (List<string> row in rows)
                total += MeasureTallestNode(row, byName) + RowGap;

            return total - RowGap;
        }

        private static float MeasureTallestNode(List<string> names, Dictionary<string, AssemblyNodeInfo> byName)
        {
            float tallest = 0f;
            foreach (string name in names)
                tallest = Mathf.Max(tallest, EstimateHeight(byName[name]));

            return tallest;
        }

        /// <summary>Stacks the given assemblies vertically at a fixed x.</summary>
        private static void PlaceColumn(List<string> names,
            Dictionary<string, AssemblyNodeInfo> byName,
            float x,
            float startY,
            Dictionary<string, Rect> result)
        {
            float cursorY = startY;

            foreach (string name in names)
            {
                float height = EstimateHeight(byName[name]);
                result[name] = new Rect(x, cursorY, NodeWidth, height);
                cursorY += height + RowGap;
            }
        }

        private static List<List<string>> BuildColumns(List<string> cluster, Dictionary<string, int> levels)
        {
            int maxLevel = 0;
            foreach (string name in cluster)
                maxLevel = Mathf.Max(maxLevel, levels[name]);

            List<List<string>> columns = new(maxLevel + 1);
            for (int i = 0; i <= maxLevel; i++)
                columns.Add(new List<string>());

            List<string> sorted = new(cluster);
            sorted.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string name in sorted)
                columns[levels[name]].Add(name);

            return columns;
        }

        /// <summary>Median heuristic. Repeatedly reorders each column by the average position of its neighbors.</summary>
        private static void ReduceCrossings(List<List<string>> columns,
            Dictionary<string, List<string>> outgoing,
            Dictionary<string, List<string>> incoming,
            Dictionary<string, int> levels)
        {
            for (int sweep = 0; sweep < OrderingSweeps; sweep++)
            {
                bool forward = sweep % 2 == 0;

                for (int step = 0; step < columns.Count; step++)
                {
                    int level = forward
                        ? step
                        : columns.Count - 1 - step;

                    Dictionary<string, int> ranks = BuildRanks(columns);
                    Dictionary<string, List<string>> neighbors = forward
                        ? outgoing
                        : incoming;

                    SortColumn(columns[level], neighbors, ranks, levels, level, forward);
                }
            }
        }

        private static Dictionary<string, int> BuildRanks(List<List<string>> columns)
        {
            Dictionary<string, int> ranks = new();

            foreach (List<string> column in columns)
                for (int i = 0; i < column.Count; i++)
                    ranks[column[i]] = i;

            return ranks;
        }

        private static void SortColumn(List<string> column,
            Dictionary<string, List<string>> neighbors,
            Dictionary<string, int> ranks,
            Dictionary<string, int> levels,
            int level,
            bool forward)
        {
            Dictionary<string, float> keys = new();

            foreach (string name in column)
                keys[name] = ComputeMedian(name, neighbors, ranks, levels, level, forward);

            column.Sort((left, right) =>
            {
                int compared = keys[left].CompareTo(keys[right]);
                return compared != 0
                    ? compared
                    : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
            });
        }

        /// <summary>Average rank of the neighbors in the sweep direction. Anchorless nodes keep their rank.</summary>
        private static float ComputeMedian(string name,
            Dictionary<string, List<string>> neighbors,
            Dictionary<string, int> ranks,
            Dictionary<string, int> levels,
            int level,
            bool forward)
        {
            float sum = 0f;
            int count = 0;

            foreach (string neighbor in neighbors[name])
            {
                if (!levels.TryGetValue(neighbor, out int neighborLevel))
                    continue;

                bool isAnchor = forward
                    ? neighborLevel < level
                    : neighborLevel > level;

                if (!isAnchor)
                    continue;

                sum += ranks[neighbor];
                count++;
            }

            return count == 0
                ? ranks[name]
                : sum / count;
        }

        private static float MeasureTallestColumn(List<List<string>> columns,
            Dictionary<string, AssemblyNodeInfo> byName)
        {
            float tallest = 0f;
            foreach (List<string> column in columns)
                tallest = Mathf.Max(tallest, MeasureColumnHeight(column, byName));

            return tallest;
        }

        private static float MeasureColumnHeight(List<string> column, Dictionary<string, AssemblyNodeInfo> byName)
        {
            if (column.Count == 0)
                return 0f;

            float total = 0f;
            foreach (string name in column)
                total += EstimateHeight(byName[name]) + RowGap;

            return total - RowGap;
        }

        /// <summary>Approximate node height, used only for spacing.</summary>
        private static float EstimateHeight(AssemblyNodeInfo info)
        {
            int unused = 0;
            foreach (AssemblyReferenceInfo reference in info.References)
                if (reference.IsUnused)
                    unused++;

            if (unused == 0)
                return BaseNodeHeight;

            return BaseNodeHeight + UnusedHeaderHeight + unused * UnusedLineHeight;
        }

        /// <summary>Longest path depth from the cluster's dependency roots.</summary>
        private static Dictionary<string, int> ComputeLevels(List<string> cluster,
            Dictionary<string, List<string>> outgoing)
        {
            HashSet<string> members = new(cluster);
            Dictionary<string, int> levels = new();

            foreach (string name in cluster)
                ResolveLevel(name, outgoing, members, levels, new HashSet<string>());

            return levels;
        }

        private static int ResolveLevel(string name,
            Dictionary<string, List<string>> outgoing,
            HashSet<string> members,
            Dictionary<string, int> levels,
            HashSet<string> guard)
        {
            if (levels.TryGetValue(name, out int cached))
                return cached;

            if (!guard.Add(name))
                return 0;

            int level = 0;
            foreach (string target in outgoing[name])
            {
                if (!members.Contains(target))
                    continue;

                level = Mathf.Max(level, ResolveLevel(target, outgoing, members, levels, guard) + 1);
            }

            guard.Remove(name);
            levels[name] = level;
            return level;
        }
    }
}