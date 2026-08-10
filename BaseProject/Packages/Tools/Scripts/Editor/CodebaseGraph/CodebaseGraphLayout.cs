using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEngine;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Places the entries. Connected entries are grouped into clusters, layered by how deep their
    /// dependencies go, and reordered a few times to cut down on crossing edges. Entries without any
    /// relation form a grid above the clusters so they read as a separate group.
    /// </summary>
    public static class CodebaseGraphLayout
    {
        private const float BadgeRowHeight = 30f;

        /// <summary>
        /// Rough height of a node body. Nodes size themselves to their content, so this only has to be
        /// close enough to keep rows from touching.
        /// </summary>
        private const float BaseNodeHeight = 138f;

        private const float ClusterGap = 160f;
        private const float ColumnGap = 110f;
        private const float LonelyBlockGap = 200f;
        private const int LonelyColumns = 6;
        private const int MaxColumnRows = 12;
        private const float MemberListPadding = 16f;
        private const float MemberRowHeight = 18f;

        /// <summary>
        /// Each level is drawn at its own width so the silhouette alone says which one you are looking
        /// at. The node applies these to itself rather than the stylesheet carrying its own copy,
        /// because the layout has to place nodes at exactly the width they render.
        /// </summary>
        private const float MemberWidth = 280f;

        private const float NamespaceWidth = 380f;
        private const int OrderingSweeps = 6;
        private const float RowGap = 30f;
        private const float SubColumnGap = 24f;
        private const float TypeWidth = 320f;

        /// <summary>Returns a placement rect for every entry.</summary>
        /// <param name="entries">Entries to place.</param>
        /// <returns>The rects, keyed by entry id.</returns>
        public static Dictionary<string, Rect> Calculate(IReadOnlyList<GraphEntry> entries, ELayoutMode mode)
        {
            Dictionary<string, Rect> result = new();
            if (entries == null || entries.Count == 0)
                return result;

            Dictionary<string, GraphEntry> byId = BuildLookup(entries);

            if (mode == ELayoutMode.Grouped)
                return CalculateGrouped(entries, byId);
            Dictionary<string, List<string>> outgoing = BuildOutgoing(entries, byId);
            Dictionary<string, List<string>> incoming = BuildIncoming(outgoing);

            List<List<string>> clusters = new();
            List<string> lonely = new();
            SplitClusters(entries, outgoing, incoming, clusters, lonely);

            clusters.Sort((left, right) => right.Count.CompareTo(left.Count));

            float offsetY = 0f;
            foreach (List<string> cluster in clusters)
            {
                offsetY += PlaceCluster(cluster, byId, outgoing, incoming, offsetY, result) + ClusterGap;
            }

            PlaceLonelyBlock(lonely, byId, result);
            return result;
        }

        /// <summary>
        /// Lays everything out by name, in families, ignoring dependencies entirely. The layered
        /// arrangement answers what depends on what, which is the wrong question when you are trying to
        /// find something: it scatters one package across the width of the graph according to how deep
        /// each part sits. This puts a package back together and sorts it, at the cost of every edge
        /// being longer.
        /// </summary>
        private static Dictionary<string, Rect> CalculateGrouped(IReadOnlyList<GraphEntry> entries,
            Dictionary<string, GraphEntry> byId)
        {
            Dictionary<string, Rect> result = new();
            Dictionary<string, List<string>> families = new(StringComparer.Ordinal);

            foreach (GraphEntry entry in entries)
            {
                if (!families.TryGetValue(entry.ColorSeed, out List<string> members))
                {
                    members = new List<string>();
                    families[entry.ColorSeed] = members;
                }

                members.Add(entry.Id);
            }

            List<string> seeds = new(families.Keys);
            seeds.Sort(StringComparer.OrdinalIgnoreCase);

            float x = 0f;

            foreach (string seed in seeds)
            {
                List<string> members = families[seed];
                members.Sort((left, right) => string.Compare(byId[left].Title,
                    byId[right].Title,
                    StringComparison.OrdinalIgnoreCase));

                x += PlaceFamily(members, byId, x, result) + ClusterGap;
            }

            return result;
        }

        private static float PlaceFamily(List<string> members,
            Dictionary<string, GraphEntry> byId,
            float startX,
            Dictionary<string, Rect> result)
        {
            float width = MeasureColumnWidth(members, byId);
            float x = startX;
            float cursorY = 0f;
            int rowIndex = 0;

            foreach (string id in members)
            {
                if (rowIndex == MaxColumnRows)
                {
                    rowIndex = 0;
                    cursorY = 0f;
                    x += width + SubColumnGap;
                }

                float height = EstimateHeight(byId[id]);
                result[id] = new Rect(x, cursorY, MeasureWidth(byId[id]), height);
                cursorY += height + RowGap;
                rowIndex++;
            }

            return x - startX + width;
        }

        private static Dictionary<string, GraphEntry> BuildLookup(IReadOnlyList<GraphEntry> entries)
        {
            Dictionary<string, GraphEntry> byId = new();
            foreach (GraphEntry entry in entries)
                byId[entry.Id] = entry;

            return byId;
        }

        private static Dictionary<string, List<string>> BuildOutgoing(IReadOnlyList<GraphEntry> entries,
            Dictionary<string, GraphEntry> byId)
        {
            Dictionary<string, List<string>> outgoing = new();

            foreach (GraphEntry entry in entries)
            {
                List<string> targets = new();

                foreach (GraphEdgeInfo target in entry.Targets)
                {
                    if (target.TargetId != entry.Id
                        && byId.ContainsKey(target.TargetId)
                        && !targets.Contains(target.TargetId))
                        targets.Add(target.TargetId);
                }

                outgoing[entry.Id] = targets;
            }

            return outgoing;
        }

        private static Dictionary<string, List<string>> BuildIncoming(Dictionary<string, List<string>> outgoing)
        {
            Dictionary<string, List<string>> incoming = new();

            foreach (string id in outgoing.Keys)
                incoming[id] = new List<string>();

            foreach (KeyValuePair<string, List<string>> pair in outgoing)
            {
                foreach (string target in pair.Value)
                    incoming[target].Add(pair.Key);
            }

            return incoming;
        }

        private static void SplitClusters(IReadOnlyList<GraphEntry> entries,
            Dictionary<string, List<string>> outgoing,
            Dictionary<string, List<string>> incoming,
            List<List<string>> clusters,
            List<string> lonely)
        {
            HashSet<string> visited = new();

            foreach (GraphEntry entry in entries)
            {
                if (visited.Contains(entry.Id))
                    continue;

                if (outgoing[entry.Id].Count == 0 && incoming[entry.Id].Count == 0)
                {
                    visited.Add(entry.Id);
                    lonely.Add(entry.Id);
                    continue;
                }

                clusters.Add(Flood(entry.Id, outgoing, incoming, visited));
            }
        }

        private static List<string> Flood(string start,
            Dictionary<string, List<string>> outgoing,
            Dictionary<string, List<string>> incoming,
            HashSet<string> visited)
        {
            List<string> cluster = new();
            Queue<string> pending = new();

            pending.Enqueue(start);
            visited.Add(start);

            while (pending.Count > 0)
            {
                string current = pending.Dequeue();
                cluster.Add(current);

                Enqueue(outgoing[current], visited, pending);
                Enqueue(incoming[current], visited, pending);
            }

            return cluster;
        }

        private static void Enqueue(List<string> neighbors, HashSet<string> visited, Queue<string> pending)
        {
            foreach (string neighbor in neighbors)
            {
                if (visited.Add(neighbor))
                    pending.Enqueue(neighbor);
            }
        }

        private static float PlaceCluster(List<string> cluster,
            Dictionary<string, GraphEntry> byId,
            Dictionary<string, List<string>> outgoing,
            Dictionary<string, List<string>> incoming,
            float offsetY,
            Dictionary<string, Rect> result)
        {
            Dictionary<string, int> levels = ComputeLevels(cluster, outgoing);
            List<List<string>> columns = BuildColumns(cluster, levels);

            ReduceCrossings(columns, outgoing, incoming, levels);

            float bandHeight = MeasureTallestColumn(columns, byId);
            float cursorX = 0f;

            foreach (List<string> column in columns)
            {
                float columnHeight = MeasureColumnHeight(column, byId);
                float startY = offsetY + (bandHeight - columnHeight) * 0.5f;
                cursorX += PlaceColumn(column, byId, cursorX, startY, result) + ColumnGap;
            }

            return bandHeight;
        }

        /// <summary>
        /// Walks the cluster iteratively, for the same reason the cycle finder does: a long dependency
        /// chain in a real project is deep enough to be worth not putting on the call stack.
        /// </summary>
        private static Dictionary<string, int> ComputeLevels(List<string> cluster,
            Dictionary<string, List<string>> outgoing)
        {
            HashSet<string> members = new(cluster);
            Dictionary<string, int> levels = new();
            HashSet<string> onStack = new();
            Stack<string> pending = new();

            foreach (string root in cluster)
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

        private static void PushUnresolved(string id,
            Dictionary<string, List<string>> outgoing,
            HashSet<string> members,
            Dictionary<string, int> levels,
            HashSet<string> onStack,
            Stack<string> pending)
        {
            foreach (string target in outgoing[id])
            {
                // A target already on the stack closes a cycle, which simply stops the walk there.
                if (!members.Contains(target) || levels.ContainsKey(target) || onStack.Contains(target))
                    continue;

                pending.Push(target);
            }
        }

        private static int MeasureLevel(string id,
            Dictionary<string, List<string>> outgoing,
            HashSet<string> members,
            Dictionary<string, int> levels)
        {
            int level = 0;

            foreach (string target in outgoing[id])
            {
                if (!members.Contains(target) || !levels.TryGetValue(target, out int targetLevel))
                    continue;

                level = Mathf.Max(level, targetLevel + 1);
            }

            return level;
        }

        private static List<List<string>> BuildColumns(List<string> cluster, Dictionary<string, int> levels)
        {
            int maxLevel = 0;
            foreach (string id in cluster)
                maxLevel = Mathf.Max(maxLevel, levels[id]);

            List<List<string>> columns = new(maxLevel + 1);
            for (int index = 0; index <= maxLevel; index++)
                columns.Add(new List<string>());

            List<string> sorted = new(cluster);
            sorted.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string id in sorted)
                columns[levels[id]].Add(id);

            return columns;
        }

        private static void ReduceCrossings(List<List<string>> columns,
            Dictionary<string, List<string>> outgoing,
            Dictionary<string, List<string>> incoming,
            Dictionary<string, int> levels)
        {
            for (int sweep = 0; sweep < OrderingSweeps; sweep++)
            {
                bool forward = sweep % 2 == 0;

                Dictionary<string, int> ranks = BuildRanks(columns);
                Dictionary<string, List<string>> neighbors = forward
                    ? outgoing
                    : incoming;

                for (int step = 0; step < columns.Count; step++)
                {
                    int level = forward
                        ? step
                        : columns.Count - 1 - step;

                    SortColumn(columns[level], neighbors, ranks, levels, level, forward);

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
            Dictionary<string, List<string>> neighbors,
            Dictionary<string, int> ranks,
            Dictionary<string, int> levels,
            int level,
            bool forward)
        {
            Dictionary<string, float> keys = new();

            foreach (string id in column)
                keys[id] = ComputeMedian(id, neighbors, ranks, levels, level, forward);

            column.Sort((left, right) =>
            {
                int compared = keys[left].CompareTo(keys[right]);
                return compared != 0
                    ? compared
                    : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
            });
        }

        private static float ComputeMedian(string id,
            Dictionary<string, List<string>> neighbors,
            Dictionary<string, int> ranks,
            Dictionary<string, int> levels,
            int level,
            bool forward)
        {
            float sum = 0f;
            int count = 0;

            foreach (string neighbor in neighbors[id])
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
                ? ranks[id]
                : sum / count;
        }

        /// <summary>
        /// Places one level and returns how wide it ended up. A level with a hundred nodes would
        /// otherwise become a single unreadable vertical stack, so it wraps into side by side runs.
        /// </summary>
        private static float PlaceColumn(List<string> ids,
            Dictionary<string, GraphEntry> byId,
            float startX,
            float startY,
            Dictionary<string, Rect> result)
        {
            float width = MeasureColumnWidth(ids, byId);
            float x = startX;
            float cursorY = startY;
            int rowIndex = 0;

            foreach (string id in ids)
            {
                if (rowIndex == MaxColumnRows)
                {
                    rowIndex = 0;
                    cursorY = startY;
                    x += width + SubColumnGap;
                }

                float height = EstimateHeight(byId[id]);
                result[id] = new Rect(x, cursorY, MeasureWidth(byId[id]), height);
                cursorY += height + RowGap;
                rowIndex++;
            }

            return x - startX + width;
        }

        private static float MeasureColumnWidth(List<string> ids, Dictionary<string, GraphEntry> byId)
        {
            float widest = 0f;

            foreach (string id in ids)
                widest = Mathf.Max(widest, MeasureWidth(byId[id]));

            return widest;
        }

        private static void PlaceLonelyBlock(List<string> lonely,
            Dictionary<string, GraphEntry> byId,
            Dictionary<string, Rect> result)
        {
            if (lonely.Count == 0)
                return;

            List<List<string>> rows = BuildGridRows(lonely);
            float startY = -(MeasureGridHeight(rows, byId) + LonelyBlockGap);

            foreach (List<string> row in rows)
            {
                float rowHeight = MeasureTallestNode(row, byId);

                float width = MeasureColumnWidth(row, byId);

                for (int column = 0; column < row.Count; column++)
                {
                    float x = column * (width + ColumnGap);
                    GraphEntry entry = byId[row[column]];
                    result[row[column]] = new Rect(x, startY, MeasureWidth(entry), EstimateHeight(entry));
                }

                startY += rowHeight + RowGap;
            }
        }

        private static List<List<string>> BuildGridRows(List<string> ids)
        {
            List<List<string>> rows = new();

            for (int index = 0; index < ids.Count; index++)
            {
                if (index % LonelyColumns == 0)
                    rows.Add(new List<string>());

                rows[^1].Add(ids[index]);
            }

            return rows;
        }

        private static float MeasureGridHeight(List<List<string>> rows, Dictionary<string, GraphEntry> byId)
        {
            float total = 0f;
            foreach (List<string> row in rows)
                total += MeasureTallestNode(row, byId) + RowGap;

            return total - RowGap;
        }

        private static float MeasureTallestNode(List<string> ids, Dictionary<string, GraphEntry> byId)
        {
            float tallest = 0f;
            foreach (string id in ids)
                tallest = Mathf.Max(tallest, EstimateHeight(byId[id]));

            return tallest;
        }

        private static float MeasureTallestColumn(List<List<string>> columns, Dictionary<string, GraphEntry> byId)
        {
            float tallest = 0f;
            foreach (List<string> column in columns)
                tallest = Mathf.Max(tallest, MeasureColumnHeight(column, byId));

            return tallest;
        }

        private static float MeasureColumnHeight(List<string> column, Dictionary<string, GraphEntry> byId)
        {
            if (column.Count == 0)
                return 0f;

            // Only the first run matters, since every wrapped run starts at the same height.
            int rows = column.Count < MaxColumnRows
                ? column.Count
                : MaxColumnRows;

            float total = 0f;
            for (int index = 0; index < rows; index++)
                total += EstimateHeight(byId[column[index]]) + RowGap;

            return total - RowGap;
        }

        /// <summary>Returns the width a node of this level is drawn at.</summary>
        /// <param name="entry">Entry to measure.</param>
        /// <returns>The node width.</returns>
        public static float MeasureWidth(GraphEntry entry)
        {
            switch (entry.Level)
            {
                case EGraphScope.Namespace:
                    return NamespaceWidth;

                case EGraphScope.Member:
                    return MemberWidth;

                default:
                    return TypeWidth;
            }
        }

        private static float EstimateHeight(GraphEntry entry)
        {
            float rows = entry.Rows.Count * MemberRowHeight;
            float overflow = entry.HiddenRowCount > 0
                ? MemberRowHeight
                : 0f;

            float list = entry.Rows.Count > 0
                ? MemberListPadding
                : 0f;

            return BaseNodeHeight + entry.BadgeCount * BadgeRowHeight + rows + overflow + list;
        }
    }
}
