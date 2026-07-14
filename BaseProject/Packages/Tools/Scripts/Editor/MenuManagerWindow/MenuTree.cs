#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Pure tree algorithms shared by the package registry and the project overlay.</summary>
    public static class MenuTree
    {
        /// <summary>Assigns derived priorities across a sequence of roots sharing one counter.</summary>
        public static void Priorities(IReadOnlyList<List<MenuNode>> roots, int start, int gap)
        {
            int priority = start;
            bool first = true;
            bool pending = false;

            foreach (List<MenuNode> root in roots)
            {
                pending = true;
                WalkNodes(root, gap, ref priority, ref first, ref pending);
            }
        }

        /// <summary>Marks entries as present or missing and returns true when serialized data changed.</summary>
        public static bool Mark(List<MenuNode> nodes, IReadOnlyDictionary<string, ResolvedMenu> resolved, HashSet<string> known)
        {
            bool changed = false;

            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    changed |= Mark(group.Children, resolved, known);
                    continue;
                }

                if (node is not MenuEntryNode entryNode)
                    continue;

                MenuEntry entry = entryNode.Entry;
                known.Add(entry.Id);
                bool present = resolved.TryGetValue(entry.Id, out ResolvedMenu match);
                entry.Missing = !present;

                if (!present)
                    continue;

                if (entry.Kind != match.Kind)
                {
                    entry.Kind = match.Kind;
                    changed = true;
                }

                if (match.Kind == EMenuEntryKind.CreateAsset && string.IsNullOrWhiteSpace(entry.CreateFileName))
                {
                    entry.CreateFileName = match.DefaultFileName;
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>Collects every entry id in the tree.</summary>
        public static void CollectIds(List<MenuNode> nodes, HashSet<string> ids)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                    CollectIds(group.Children, ids);
                else if (node is MenuEntryNode entryNode)
                    ids.Add(entryNode.Entry.Id);
            }
        }

        /// <summary>Removes entries whose id matches the predicate. Returns true when anything was removed.</summary>
        public static bool RemoveEntries(List<MenuNode> nodes, Predicate<string> removeById)
        {
            bool changed = false;

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                MenuNode node = nodes[i];

                if (node is MenuGroupNode group)
                {
                    changed |= RemoveEntries(group.Children, removeById);
                }
                else if (node is MenuEntryNode entryNode && removeById(entryNode.Entry.Id))
                {
                    nodes.RemoveAt(i);
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>Collects entries paired with their full resolved paths across a sequence of roots.</summary>
        public static void Collect(IReadOnlyList<List<MenuNode>> roots, EMenuEntryKind kind, List<(MenuEntry entry, string path)> result)
        {
            string prefixRoot = MenuPath.Prefix(kind);

            foreach (List<MenuNode> root in roots)
            {
                List<string> prefix = new();

                if (!string.IsNullOrEmpty(prefixRoot))
                    prefix.Add(prefixRoot);

                CollectPaths(root, prefix, result);
            }
        }

        private static void WalkNodes(List<MenuNode> nodes, int gap, ref int priority, ref bool first, ref bool pending)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    pending = true;
                    WalkNodes(group.Children, gap, ref priority, ref first, ref pending);
                    pending = true;
                }
                else if (node is MenuEntryNode entryNode)
                {
                    Emit(entryNode.Entry, gap, ref priority, ref first, ref pending);
                }
            }
        }

        private static void Emit(MenuEntry entry, int gap, ref int priority, ref bool first, ref bool pending)
        {
            if (!entry.Enabled || entry.Missing || string.IsNullOrWhiteSpace(entry.Path))
            {
                entry.Priority = int.MinValue;
                return;
            }

            if (first)
                first = false;
            else if (pending)
                priority += gap;

            pending = false;
            entry.Priority = priority;
            priority++;
        }

        private static void CollectPaths(List<MenuNode> nodes, List<string> prefix, List<(MenuEntry, string)> result)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    prefix.Add(group.Name);
                    CollectPaths(group.Children, prefix, result);
                    prefix.RemoveAt(prefix.Count - 1);
                }
                else if (node is MenuEntryNode entryNode)
                {
                    prefix.Add(entryNode.Entry.Path);
                    result.Add((entryNode.Entry, MenuPath.Combine(prefix)));
                    prefix.RemoveAt(prefix.Count - 1);
                }
            }
        }
    }
}
#endif
