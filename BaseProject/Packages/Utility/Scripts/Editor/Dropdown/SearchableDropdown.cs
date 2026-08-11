using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Dropdown
{
    /// <summary>
    /// A searchable, tree-shaped dropdown built from a flat list of labels. Labels containing slashes
    /// become nested submenus, matching the Add Component menu. Used wherever a plain popup would grow
    /// long enough to be unusable.
    /// </summary>
    /// <remarks>
    /// Lives in the utility package rather than next to its first consumer, because the type picker, the
    /// reference picker and the option dropdown all need it and they sit in different packages.
    /// </remarks>
    public sealed class SearchableDropdown : AdvancedDropdown
    {
        /// <summary>Number of options above which a plain popup should be replaced by this dropdown.</summary>
        public const int Threshold = 12;

        private const char PathSeparator = '/';
        private const float MinimumHeight = 300f;
        private const float MinimumWidth = 220f;

        private readonly IReadOnlyList<string> _labels;
        private readonly Action<int> _onSelected;
        private readonly string _title;

        /// <summary>Creates the dropdown.</summary>
        /// <param name="state">Scroll and search state, kept alive by the caller between openings.</param>
        /// <param name="title">Header text of the dropdown.</param>
        /// <param name="labels">The options, optionally using slashes to build submenus.</param>
        /// <param name="onSelected">Called with the index of the chosen option.</param>
        public SearchableDropdown(AdvancedDropdownState state, string title, IReadOnlyList<string> labels,
            Action<int> onSelected) : base(state)
        {
            _title = title;
            _labels = labels;
            _onSelected = onSelected;
            minimumSize = new Vector2(MinimumWidth, MinimumHeight);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new(_title);

            for (int i = 0; i < _labels.Count; i++)
                Insert(root, _labels[i] ?? string.Empty, i);

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is SearchableDropdownItem leaf)
                _onSelected?.Invoke(leaf.Index);
        }

        // Walks the slash-separated path and reuses existing group nodes, so options sharing a prefix
        // end up under the same submenu instead of creating a duplicate branch each time.
        private static void Insert(AdvancedDropdownItem root, string label, int index)
        {
            string[] parts = label.Split(PathSeparator);
            AdvancedDropdownItem parent = root;

            for (int i = 0; i < parts.Length - 1; i++)
                parent = FindOrCreateGroup(parent, parts[i]);

            SearchableDropdownItem leaf = new(parts[^1], index)
            {
                id = index
            };

            parent.AddChild(leaf);
        }

        private static AdvancedDropdownItem FindOrCreateGroup(AdvancedDropdownItem parent, string name)
        {
            foreach (AdvancedDropdownItem child in parent.children)
            {
                if (child is not SearchableDropdownItem && child.name == name)
                    return child;
            }

            AdvancedDropdownItem group = new(name);
            parent.AddChild(group);
            return group;
        }
    }
}
