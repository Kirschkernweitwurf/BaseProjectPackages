#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Pure tree algorithms shared by the package registry and the project overlay.</summary>
    public static class MenuTree
    {
        /// <summary>Priority distance that makes Unity draw a separator line. Unity draws one when the gap is more than 10.</summary>
        public const int SeparatorGap = 11;

        /// <summary>Assigns every entry its derived priority.</summary>
        /// <remarks>
        /// Unity gives a submenu the priority of its first defined child, for ordering and for separator lines alike.
        /// So a node only ever competes with its own siblings: it takes one slot, and a group lays its children out
        /// starting at that same slot. What a group holds inside can never push a sibling group away.
        /// </remarks>
        public static void Priorities(IReadOnlyList<List<MenuNode>> roots, int start)
        {
            int slot = start;
            bool first = true;
            bool boundary = false;

            foreach (List<MenuNode> root in roots)
            {
                foreach (MenuNode node in root)
                {
                    if (!IsLive(node))
                    {
                        ClearDead(node);
                        continue;
                    }

                    if (first)
                        first = false;
                    else
                        slot += boundary || node.Separator ? SeparatorGap : 1;

                    boundary = false;
                    AssignSlot(node, slot);
                }

                boundary = !first;
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

        /// <summary>Removes every entry whose code no longer exists. Returns true when anything was removed.</summary>
        public static bool RemoveMissing(List<MenuNode> nodes)
        {
            bool changed = false;

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                MenuNode node = nodes[i];

                if (node is MenuGroupNode group)
                {
                    changed |= RemoveMissing(group.Children);
                }
                else if (node is MenuEntryNode entryNode && entryNode.Entry.Missing)
                {
                    nodes.RemoveAt(i);
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>True when the tree holds at least one entry whose code no longer exists.</summary>
        public static bool HasMissing(List<MenuNode> nodes)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    if (HasMissing(group.Children))
                        return true;
                }
                else if (node is MenuEntryNode entryNode && entryNode.Entry.Missing)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Rebuilds groups from each entry's default path, shortens every entry to its last segment, and resets asset file names.</summary>
        public static bool AutoGroup(List<MenuNode> root, IReadOnlyDictionary<string, ResolvedMenu> resolved)
        {
            List<MenuEntryNode> entries = new();
            CollectEntries(root, entries);

            List<(MenuEntryNode node, ResolvedMenu match)> movable = new();
            HashSet<string> ids = new();

            foreach (MenuEntryNode node in entries)
            {
                if (!resolved.TryGetValue(node.Entry.Id, out ResolvedMenu match))
                    continue;

                if (string.IsNullOrWhiteSpace(match.DefaultPath))
                    continue;

                movable.Add((node, match));
                ids.Add(node.Entry.Id);
            }

            if (movable.Count == 0)
                return false;

            RemoveEntries(root, id => ids.Contains(id));

            foreach ((MenuEntryNode node, ResolvedMenu match) in movable)
            {
                string[] segments = match.DefaultPath.Split('/');
                List<MenuNode> target = root;

                for (int i = 0; i < segments.Length - 1; i++)
                {
                    string name = segments[i].Trim();

                    if (name.Length == 0)
                        continue;

                    target = FindOrCreateGroup(target, name).Children;
                }

                node.Entry.Path = segments[segments.Length - 1].Trim();

                if (match.Kind == EMenuEntryKind.CreateAsset && !string.IsNullOrWhiteSpace(match.DefaultFileName))
                    node.Entry.CreateFileName = match.DefaultFileName;

                target.Add(node);
            }

            PruneEmptyGroups(root);
            return true;
        }

        /// <summary>Sorts groups and entries by display name at every level. Returns true when the order changed.</summary>
        public static bool Sort(List<MenuNode> nodes)
        {
            List<MenuNode> before = new(nodes);
            nodes.Sort(CompareNodes);
            bool changed = false;

            for (int i = 0; i < nodes.Count; i++)
            {
                if (ReferenceEquals(before[i], nodes[i]))
                    continue;

                changed = true;
                break;
            }

            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                    changed |= Sort(group.Children);
            }

            return changed;
        }

        /// <summary>Rebuilds separator flags from the retired merged flag, preserving the old implicit gaps. Runs once.</summary>
        public static void MigrateSeparators(List<MenuNode> nodes)
        {
            bool afterGroup = false;

            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                {
                    group.MigrateMerged();
                    MigrateSeparators(group.Children);
                    afterGroup = true;
                }
                else if (node is MenuEntryNode entryNode)
                {
                    entryNode.Separator = afterGroup;
                    afterGroup = false;
                }
            }
        }

        /// <summary>Removes every group that holds no entries, innermost first. Returns true when anything was removed.</summary>
        public static bool PruneEmptyGroups(List<MenuNode> nodes)
        {
            bool changed = false;

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i] is not MenuGroupNode group)
                    continue;

                changed |= PruneEmptyGroups(group.Children);

                if (group.Children.Count != 0)
                    continue;

                nodes.RemoveAt(i);
                changed = true;
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

        private static void CollectEntries(List<MenuNode> nodes, List<MenuEntryNode> result)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                    CollectEntries(group.Children, result);
                else if (node is MenuEntryNode entryNode)
                    result.Add(entryNode);
            }
        }

        private static MenuGroupNode FindOrCreateGroup(List<MenuNode> nodes, string name)
        {
            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group && string.Equals(group.Name, name, StringComparison.Ordinal))
                    return group;
            }

            MenuGroupNode created = new(name) { Expanded = true };
            nodes.Add(created);
            return created;
        }

        private static void AssignSlot(MenuNode node, int slot)
        {
            if (node is MenuEntryNode entryNode)
            {
                entryNode.Entry.Priority = slot;
                return;
            }

            if (node is MenuGroupNode group)
                LayoutChildren(group.Children, slot);
        }

        private static void LayoutChildren(List<MenuNode> nodes, int baseSlot)
        {
            int slot = baseSlot;
            bool first = true;

            foreach (MenuNode node in nodes)
            {
                if (!IsLive(node))
                {
                    ClearDead(node);
                    continue;
                }

                if (first)
                    first = false;
                else
                    slot += node.Separator ? SeparatorGap : 1;

                AssignSlot(node, slot);
            }
        }

        private static bool IsLive(MenuNode node)
        {
            if (node is MenuEntryNode entryNode)
            {
                MenuEntry entry = entryNode.Entry;
                return entry.Enabled && !entry.Missing && !string.IsNullOrWhiteSpace(entry.Path);
            }

            if (node is not MenuGroupNode group)
                return false;

            foreach (MenuNode child in group.Children)
            {
                if (IsLive(child))
                    return true;
            }

            return false;
        }

        private static void ClearDead(MenuNode node)
        {
            if (node is MenuEntryNode entryNode)
            {
                entryNode.Entry.Priority = int.MinValue;
                return;
            }

            if (node is not MenuGroupNode group)
                return;

            foreach (MenuNode child in group.Children)
                ClearDead(child);
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

        private static int CompareNodes(MenuNode left, MenuNode right) =>
            string.Compare(DisplayName(left), DisplayName(right), StringComparison.OrdinalIgnoreCase);

        private static string DisplayName(MenuNode node)
        {
            if (node is MenuGroupNode group)
                return group.Name;

            if (node is MenuEntryNode entryNode)
                return entryNode.Entry.Path;

            return string.Empty;
        }

    }
}
#endif
