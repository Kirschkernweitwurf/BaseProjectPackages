using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Base.ToolsPackage.Editor.MenuManagerModel
{
    /// <summary>Leaf node wrapping a single managed entry.</summary>
    /// <remarks>
    /// The [SerializeReference] records in the shipped registry asset name the namespace and the
    /// assembly this type was written under, so both have to keep resolving after the model was
    /// split off or every stored node loads as null and the asset silently empties itself.
    /// </remarks>
    [Serializable]
    [MovedFrom(false, "Base.ToolsPackage.Editor.MenuManagerWindows",
        "Base.ToolsPackage.Editor", "MenuEntryNode")]
    internal sealed class MenuEntryNode : MenuNode
    {
        [SerializeField]
        private MenuEntry entry;

        /// <summary>The wrapped entry.</summary>
        internal MenuEntry Entry => entry;

        /// <summary>Required by serialization.</summary>
        public MenuEntryNode() { }

        /// <summary>Wraps an existing entry.</summary>
        public MenuEntryNode(MenuEntry entry) => this.entry = entry;
    }
}