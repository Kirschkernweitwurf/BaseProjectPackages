using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Config
{
    /// <summary>
    /// All settings related to how prefabs are arranged in space.
    /// </summary>
    [Serializable]
    public class LayoutSettings
    {
        [field: Tooltip("How items are arranged in the zoo.")]
        [field: SerializeField] public ELayoutType Type { get; private set; } = ELayoutType.Grid;

        [field: Tooltip("Whether items are aligned to the ground or centered on their pivot.")]
        [field: SerializeField] public EAlignmentMode Alignment { get; private set; } = EAlignmentMode.Ground;

        [field: Tooltip("Gap between items inside a category.")]
        [field: Min(0f)]
        [field: SerializeField] public float Spacing { get; private set; } = 2f;

        [field: Tooltip("Gap between categories (along Z).")]
        [field: Min(0f)]
        [field: SerializeField] public float CategorySpacing { get; private set; } = 4f;

        [field: Tooltip("Columns used by Grid layout.")]
        [field: Min(1)]
        [field: SerializeField] public int GridColumns { get; private set; } = 5;

        [field: Tooltip("Minimum radius for Circle layout. Auto-grows if items don't fit.")]
        [field: Min(0f)]
        [field: SerializeField] public float CircleRadius { get; private set; } = 10f;
    }
}