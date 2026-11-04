using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// A named group of prefabs. Each group gets its own block in the zoo.
    /// </summary>
    /// <remarks>
    /// Groups are only ever created in code. This used to apply default values after loading, for the case of Unity's
    /// own plus button, and took every group made by a scan for such a case: after the next reload all of them were
    /// called "Category" and were cyan.
    /// </remarks>
    [Serializable]
    internal class ZooCategory
    {
        [field: Tooltip("Name of this group, shown on its zone in the Scene view.")]
        [field: SerializeField] public string Name { get; private set; } = "Category";

        [field: Tooltip("Color of the group's zone and of the dot next to its name.")]
        [field: SerializeField] public Color LabelColor { get; private set; } = Color.cyan;

        [field: Tooltip("Prefabs in this group. Each prefab gets its own place in the zoo.")]
        [field: SerializeField] public List<GameObject> Entries { get; private set; } = new();

        [field: Tooltip("Leaves the whole group out of the zoo. A rescan keeps it that way.")]
        [field: SerializeField] public bool IsHidden { get; private set; }

        [field: Tooltip("Assets of this group left out of the zoo. A rescan keeps them left out.")]
        [field: SerializeField] public List<GameObject> HiddenEntries { get; private set; } = new();

        /// <summary>
        /// Creates an empty category with default values.
        /// </summary>
        public ZooCategory() { }

        /// <summary>
        /// Creates a filled category. Used by tooling that generates categories.
        /// </summary>
        public ZooCategory(string name, Color labelColor, IEnumerable<GameObject> entries)
        {
            Name = name;
            LabelColor = labelColor;
            Entries = new List<GameObject>(entries);
        }

        /// <summary>
        /// Adds a prefab if it is not already in this category. Returns true when it was added.
        /// </summary>
        public bool TryAddEntry(GameObject prefab)
        {
            Entries ??= new List<GameObject>();

            if (prefab == null
                || Entries.Contains(prefab))
                return false;

            Entries.Add(prefab);
            return true;
        }

        /// <summary>Takes a prefab out of this group altogether.</summary>
        /// <param name="prefab">The prefab to remove.</param>
        public void RemoveEntry(GameObject prefab)
        {
            Entries?.Remove(prefab);
            HiddenEntries?.Remove(prefab);
        }

        /// <summary>True when the asset is in this group but left out of the zoo.</summary>
        /// <param name="prefab">The asset to ask about.</param>
        public bool IsEntryHidden(GameObject prefab) => HiddenEntries != null && HiddenEntries.Contains(prefab);

        /// <summary>Leaves an asset of this group out of the zoo or brings it back.</summary>
        /// <param name="prefab">The asset to change.</param>
        /// <param name="isEntryHidden">True to leave it out.</param>
        public void SetEntryHidden(GameObject prefab, bool isEntryHidden)
        {
            HiddenEntries ??= new List<GameObject>();
            HiddenEntries.Remove(prefab);

            if (isEntryHidden)
                HiddenEntries.Add(prefab);
        }

        /// <summary>Leaves the whole group out of the zoo or brings it back.</summary>
        /// <param name="isGroupHidden">True to leave it out.</param>
        public void SetHidden(bool isGroupHidden) => IsHidden = isGroupHidden;

        /// <summary>
        /// Forgets assets that were deleted from the project. Returns true when one was.
        /// </summary>
        public bool TryRemoveMissing()
        {
            HiddenEntries?.RemoveAll(entry => entry == null);

            return Entries != null
                && Entries.RemoveAll(entry => entry == null) > 0;
        }

        /// <summary>
        /// Overwrites the label color. Used by tooling that generates categories.
        /// </summary>
        public void SetLabelColor(Color color) => LabelColor = color;
    }
}