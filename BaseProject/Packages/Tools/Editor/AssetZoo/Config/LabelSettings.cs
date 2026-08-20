using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// All settings related to item / category labels in the zoo.
    /// </summary>
    [Serializable]
    public class LabelSettings
    {
        /// <summary>
        /// World-space scale applied to category labels.
        /// </summary>
        public const float CategoryWorldScale = 0.08f;

        /// <summary>
        /// World-space scale applied to item labels.
        /// </summary>
        public const float ItemWorldScale = 0.08f;

        [field: Header("Category Label Settings")]

        [field: Tooltip("Whether to show per-category labels above each group.")]
        [field: SerializeField] public bool ShowCategoryLabels { get; private set; } = true;

        [field: Tooltip("Vertical offset of labels above the item's top.")]
        [field: SerializeField] public float CategoryLabelHeight { get; private set; } = 2f;

        [field: Tooltip("Font size used for item labels. Category labels scale from this.")]
        [field: SerializeField] public int CategoryFontSize { get; private set; } = 50;

        [field: Header("Item Label Settings")]

        [field: Tooltip("Whether to show per-item labels above each prefab.")]
        [field: SerializeField] public bool ShowItemLabels { get; private set; } = true;

        [field: Tooltip("Vertical offset of labels above the item's top.")]
        [field: SerializeField] public float ItemLabelHeight { get; private set; } = 0.5f;

        [field: Tooltip("Font size used for item labels. Category labels scale from this.")]
        [field: SerializeField] public int ItemFontSize { get; private set; } = 25;

        [field: Tooltip("Color of item labels.")]
        [field: SerializeField] public Color ItemColor { get; private set; } = Color.white;
    }
}