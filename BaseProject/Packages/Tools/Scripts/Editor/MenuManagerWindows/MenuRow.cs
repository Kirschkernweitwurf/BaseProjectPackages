using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// One drawn line of a menu manager window. The tree is flattened into rows once per frame, so
    /// drawing, hit testing and the drop target search all walk the same list.
    /// </summary>
    internal sealed class MenuRow
    {
        /// <summary>Node behind the row, null for a section header and a placeholder.</summary>
        public MenuNode Node;

        /// <summary>List the node lives in, which is also the list a drop would insert into.</summary>
        public List<MenuNode> ParentList;

        /// <summary>Index of the node inside <see cref="ParentList"/>.</summary>
        public int Index;

        /// <summary>Nesting level, used for the indent and the guides.</summary>
        public int Depth;

        /// <summary>Whether the row draws a group.</summary>
        public bool IsGroup;

        /// <summary>Whether the row is the drop hint of an empty list.</summary>
        public bool IsPlaceholder;

        /// <summary>Whether the row separates the shipped tree from the project tree.</summary>
        public bool IsSectionHeader;

        /// <summary>Whether the row is the clickable gap between two entries.</summary>
        public bool IsDivider;

        /// <summary>Whether the row belongs to the read only shipped tree.</summary>
        public bool Locked;

        /// <summary>Whether a section header can be folded away.</summary>
        public bool Collapsible;

        /// <summary>Caption of a section header.</summary>
        public string Header;

        /// <summary>Group behind the row, set only when <see cref="IsGroup"/> is true.</summary>
        public MenuGroupNode Group;

        /// <summary>Entry behind the row, set only for entry rows.</summary>
        public MenuEntry Entry;

        /// <summary>Full menu path the entry resolves to, shown in the status bar.</summary>
        public string FullPath;

        /// <summary>Screen rect the row was last drawn at.</summary>
        public Rect Rect;
    }
}