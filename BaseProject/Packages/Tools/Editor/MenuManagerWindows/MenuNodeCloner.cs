using System.Collections.Generic;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Deep copies menu trees. The undo stack keeps whole snapshots, so it needs a copy that
    /// shares nothing with the live tree, down to the entries themselves.
    /// </summary>
    internal static class MenuNodeCloner
    {
        /// <summary>Copies a whole list of nodes, dropping the ones that failed to deserialize.</summary>
        /// <param name="nodes">The list to copy.</param>
        /// <returns>A new list holding new nodes.</returns>
        internal static List<MenuNode> CloneNodes(List<MenuNode> nodes)
        {
            List<MenuNode> copy = new(nodes.Count);

            foreach (MenuNode node in nodes)
            {
                MenuNode clone = CloneNode(node);

                if (clone == null)
                    continue;

                copy.Add(clone);
            }

            return copy;
        }

        private static MenuNode CloneNode(MenuNode node)
        {
            if (node is MenuGroupNode group)
            {
                MenuGroupNode clone = new(group.Name)
                {
                    Expanded = group.Expanded,
                    Separator = group.Separator
                };

                foreach (MenuNode child in group.Children)
                    clone.Children.Add(CloneNode(child));

                return clone;
            }

            if (node is MenuEntryNode entryNode
                && entryNode.Entry != null)
                return new MenuEntryNode(CloneEntry(entryNode.Entry))
                {
                    Separator = entryNode.Separator
                };

            return null;
        }

        private static MenuEntry CloneEntry(MenuEntry entry) => new(entry.Id, entry.Path, entry.Kind)
        {
            Enabled = entry.Enabled,
            CreateFileName = entry.CreateFileName,
            OverridePriority = entry.OverridePriority,
            OverrideValue = entry.OverrideValue
        };
    }
}