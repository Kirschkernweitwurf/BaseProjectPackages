using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Base.ToolsPackage.Editor.MenuManagerModel
{
    /// <summary>Group node holding an ordered list of child groups and entries.</summary>
    /// <remarks>
    /// The [SerializeReference] records in the shipped registry asset name the namespace and the
    /// assembly this type was written under, so both have to keep resolving after the model was
    /// split off or every stored node loads as null and the asset silently empties itself.
    /// </remarks>
    [Serializable]
    [MovedFrom(false, "Base.ToolsPackage.Editor.MenuManagerWindows",
        "Base.ToolsPackage.Editor", "MenuGroupNode")]
    internal sealed class MenuGroupNode : MenuNode
    {
        [SerializeField]
        private string name = "Ungrouped";

        [SerializeField]
        private bool expanded = true;

        [SerializeField]
        private bool merged;

        [SerializeReference]
        private List<MenuNode> children = new();

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
        internal List<MenuNode> Children => children;

        /// <summary>Required by serialization.</summary>
        public MenuGroupNode() => Separator = true;

        /// <summary>Creates a named group.</summary>
        public MenuGroupNode(string name) : this() => this.name = name;

        /// <summary>Converts the retired merged flag into the separator flag. Runs once during migration.</summary>
        internal void MigrateMerged()
        {
            Separator = !merged;
            merged = false;
        }
    }
}