using System.Collections.Generic;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Flattens the shipped tree and the project overlay into the row list a window draws. Groups
    /// that are folded away contribute nothing, an empty list gets a placeholder row, and the gap
    /// between two drawn siblings becomes a divider row so a separator can be toggled there.
    /// </summary>
    internal static class MenuRowBuilder
    {
        private const string PackageHeader = "Shipped layout (read only)";
        private const string ProjectHeader = "Project";

        /// <summary>Rebuilds the row list and the set of lists that must not be written to.</summary>
        /// <param name="registry">Registry holding the shipped tree.</param>
        /// <param name="overlay">Overlay holding the project tree.</param>
        /// <param name="kind">Kind of entries the window manages.</param>
        /// <param name="rows">Row list to refill.</param>
        /// <param name="lockedLists">Set of read only node lists to refill.</param>
        public static void Build(MenuRegistry registry, MenuOverlay overlay, EMenuEntryKind kind,
            List<MenuRow> rows, HashSet<List<MenuNode>> lockedLists)
        {
            rows.Clear();
            lockedLists.Clear();

            if (registry == null
                || overlay == null)
                return;

            List<MenuNode> packageRoot = registry.RootFor(kind);
            List<MenuNode> overlayRoot = overlay.RootFor(kind);

            if (!registry.IsReadOnly)
            {
                AddSectionRows(packageRoot, false, kind, rows, lockedLists);

                if (overlayRoot.Count == 0)
                    return;

                rows.Add(NewSectionHeader(ProjectHeader, false, false));
                AddSectionRows(overlayRoot, false, kind, rows, lockedLists);

                return;
            }

            rows.Add(NewSectionHeader(PackageHeader, true, true));

            if (!overlay.ShippedCollapsed)
                AddSectionRows(packageRoot, true, kind, rows, lockedLists);

            rows.Add(NewSectionHeader(ProjectHeader, false, false));
            AddSectionRows(overlayRoot, false, kind, rows, lockedLists);
        }

        private static MenuRow NewSectionHeader(string header, bool collapsible, bool locked) => new()
        {
            IsSectionHeader = true,
            Header = header,
            Collapsible = collapsible,
            Locked = locked
        };

        private static void AddSectionRows(List<MenuNode> root, bool locked, EMenuEntryKind kind,
            List<MenuRow> rows, HashSet<List<MenuNode>> lockedLists)
        {
            if (locked)
                MarkListsLocked(root, lockedLists);

            if (root.Count == 0)
            {
                rows.Add(new MenuRow
                {
                    ParentList = root,
                    Index = 0,
                    Depth = 0,
                    IsPlaceholder = true,
                    Locked = locked
                });

                return;
            }

            List<string> prefix = new();
            string prefixRoot = MenuPath.Prefix(kind);

            if (!string.IsNullOrEmpty(prefixRoot))
                prefix.Add(prefixRoot);

            BuildNodes(root, 0, prefix, locked, rows);
        }

        private static void MarkListsLocked(List<MenuNode> nodes, HashSet<List<MenuNode>> lockedLists)
        {
            lockedLists.Add(nodes);

            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                    MarkListsLocked(group.Children, lockedLists);
            }
        }

        private static void BuildNodes(List<MenuNode> nodes, int depth, List<string> prefix, bool locked,
            List<MenuRow> output)
        {
            bool anyDrawn = false;

            for (int i = 0; i < nodes.Count; i++)
            {
                MenuNode node = nodes[i];

                // A node that failed to deserialize leaves a null slot behind. It has no row, so it gets no divider.
                if (node == null)
                    continue;

                if (anyDrawn)
                    output.Add(new MenuRow
                    {
                        Node = node,
                        ParentList = nodes,
                        Index = i,
                        Depth = depth,
                        IsDivider = true,
                        Locked = locked
                    });

                anyDrawn = true;

                if (node is MenuGroupNode group)
                {
                    output.Add(new MenuRow
                    {
                        Node = node,
                        ParentList = nodes,
                        Index = i,
                        Depth = depth,
                        IsGroup = true,
                        Group = group,
                        Locked = locked
                    });

                    if (!group.Expanded)
                        continue;

                    prefix.Add(group.Name);

                    if (group.Children.Count == 0)
                        output.Add(new MenuRow
                        {
                            ParentList = group.Children,
                            Index = 0,
                            Depth = depth + 1,
                            IsPlaceholder = true,
                            Locked = locked
                        });
                    else
                        BuildNodes(group.Children, depth + 1, prefix, locked, output);

                    prefix.RemoveAt(prefix.Count - 1);
                }
                else if (node is MenuEntryNode entryNode)
                {
                    prefix.Add(entryNode.Entry.Path);
                    string full = MenuPath.Combine(prefix);
                    prefix.RemoveAt(prefix.Count - 1);

                    output.Add(new MenuRow
                    {
                        Node = node,
                        ParentList = nodes,
                        Index = i,
                        Depth = depth,
                        Entry = entryNode.Entry,
                        FullPath = full,
                        Locked = locked
                    });
                }
            }
        }
    }
}