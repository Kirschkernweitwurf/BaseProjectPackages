#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Named group of menu entries. Groups control the priority gaps that draw separator lines.</summary>
    [Serializable]
    public sealed class MenuGroup
    {
        [SerializeField]
        private string name = "Ungrouped";

        [SerializeField]
        private List<MenuEntry> entries = new();

        /// <summary>Display name of the group.</summary>
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>Entries in this group.</summary>
        public List<MenuEntry> Entries => entries;

        /// <summary>Required by serialization.</summary>
        public MenuGroup() { }

        /// <summary>Creates a new group.</summary>
        public MenuGroup(string name) => this.name = name;
    }
}
#endif