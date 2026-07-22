using System;
using System.Collections.Generic;
using Base.UtilityPackage.Editor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// A named group of prefabs. Each category gets its own row/section in the zoo.
    /// </summary>
    [Serializable]
    public class ZooCategory : SerializableDefaults
    {
        [field: Tooltip("Name of this category. Displayed as a label above the category's prefabs.")]
        [field: SerializeField] public string Name { get; private set; } = "Category";

        [field: Tooltip("Color of the category label.")]
        [field: SerializeField] public Color LabelColor { get; private set; } = Color.cyan;

        [field: Tooltip("Prefabs in this category. Each prefab gets its own entry in the zoo.")]
        [field: SerializeField] public List<GameObject> Entries { get; private set; } = new();

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

            if (prefab == null || Entries.Contains(prefab))
                return false;

            Entries.Add(prefab);
            return true;
        }

        /// <summary>
        /// Overwrites the label color. Used by tooling that generates categories.
        /// </summary>
        public void SetLabelColor(Color color) => LabelColor = color;

        protected override void ApplyDefaults()
        {
            Name = "Category";
            LabelColor = Color.cyan;
            Entries ??= new List<GameObject>();
        }
    }
}