#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Leaf node wrapping a single managed entry.</summary>
    [Serializable]
    public sealed class MenuEntryNode : MenuNode
    {
        [SerializeField]
        private MenuEntry entry;

        /// <summary>The wrapped entry.</summary>
        public MenuEntry Entry => entry;

        /// <summary>Required by serialization.</summary>
        public MenuEntryNode() { }

        /// <summary>Wraps an existing entry.</summary>
        public MenuEntryNode(MenuEntry entry) => this.entry = entry;
    }
}
#endif