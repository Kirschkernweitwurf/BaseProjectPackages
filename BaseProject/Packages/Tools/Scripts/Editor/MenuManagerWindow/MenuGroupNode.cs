#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Group node holding an ordered list of child groups and entries.</summary>
    [Serializable]
    public sealed class MenuGroupNode : MenuNode
    {
        [SerializeField]
        private string name = "Ungrouped";

        [SerializeField]
        private bool expanded = true;

        [SerializeField]
        private bool merged;

        [SerializeReference]
        private List<MenuNode> children = new();

        /// <summary>Required by serialization.</summary>
        public MenuGroupNode() => Separator = true;

        /// <summary>Creates a named group.</summary>
        public MenuGroupNode(string name) : this() => this.name = name;

        /// <summary>Display name of the group.</summary>
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>Whether the group is expanded in the window.</summary>
        public bool Expanded
        {
            get => expanded;
            set => expanded = value;
        }

        /// <summary>Ordered child nodes.</summary>
        public List<MenuNode> Children => children;

        /// <summary>Converts the retired merged flag into the separator flag. Runs once during migration.</summary>
        public void MigrateMerged()
        {
            Separator = !merged;
            merged = false;
        }
    }
}
#endif
