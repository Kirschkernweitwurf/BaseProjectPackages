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
        internal MenuNode Node;

        /// <summary>List the node lives in, which is also the list a drop would insert into.</summary>
        internal List<MenuNode> ParentList;

        /// <summary>Index of the node inside <see cref="ParentList"/>.</summary>
        internal int Index;

        /// <summary>Nesting level, used for the indent and the guides.</summary>
        internal int Depth;

        /// <summary>Whether the row draws a group.</summary>
        internal bool IsGroup;

        /// <summary>Whether the row is the drop hint of an empty list.</summary>
        internal bool IsPlaceholder;

        /// <summary>Whether the row separates the shipped tree from the project tree.</summary>
        internal bool IsSectionHeader;

        /// <summary>Whether the row is the clickable gap between two entries.</summary>
        internal bool IsDivider;

        /// <summary>Whether the row belongs to the read only shipped tree.</summary>
        internal bool Locked;

        /// <summary>Whether a section header can be folded away.</summary>
        internal bool Collapsible;

        /// <summary>Caption of a section header.</summary>
        internal string Header;

        /// <summary>Group behind the row, set only when <see cref="IsGroup"/> is true.</summary>
        internal MenuGroupNode Group;

        /// <summary>Entry behind the row, set only for entry rows.</summary>
        internal MenuEntry Entry;

        /// <summary>Full menu path the entry resolves to, shown in the status bar.</summary>
        internal string FullPath;

        /// <summary>Screen rect the row was last drawn at.</summary>
        internal Rect Rect;
    }
}