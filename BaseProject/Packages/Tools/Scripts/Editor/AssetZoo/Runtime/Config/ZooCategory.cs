using System;
using System.Collections.Generic;
using Base.UtilityPackage.Editor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Config
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
        [field: SerializeField] public List<ZooEntry> Entries { get; private set; } = new();

        protected override void ApplyDefaults()
        {
            Name = "Category";
            LabelColor = Color.cyan;
            Entries ??= new List<ZooEntry>();
        }
    }
}