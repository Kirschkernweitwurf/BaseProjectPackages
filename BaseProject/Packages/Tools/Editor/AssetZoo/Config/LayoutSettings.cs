using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// How the assets stand in the zoo. All of it can change while the zoo is standing.
    /// </summary>
    [Serializable]
    internal class LayoutSettings
    {
        [field: Tooltip("How the assets of a group stand on the floor.")]
        [field: SerializeField] public ELayoutType Type { get; private set; } = ELayoutType.Rows;

        [field: Tooltip("Which point of an asset stands on its place: the bottom of what you see, its own pivot "
            + "or the middle of what you see. The prefab itself never changes.")]
        [field: SerializeField] public EAlignmentMode Alignment { get; private set; } = EAlignmentMode.Ground;

        [field: Tooltip("Small assets in front and big ones behind them, or the order of the list.")]
        [field: SerializeField] public EItemOrder Order { get; private set; } = EItemOrder.SmallInFront;

        [field: Tooltip("How the groups stand next to each other.")]
        [field: SerializeField] public EGroupArrangement Groups { get; private set; } = EGroupArrangement.Blocks;

        [field: Tooltip("Gap between two assets, in meters.")]
        [field: Min(0f)]
        [field: SerializeField] public float Spacing { get; private set; } = 0.6f;

        [field: Tooltip("Gap between two groups, in meters.")]
        [field: Min(0f)]
        [field: SerializeField] public float CategorySpacing { get; private set; } = 3f;

        [field: Tooltip("How many assets stand in one row.")]
        [field: Min(1)]
        [field: FormerlySerializedAs("<GridColumns>k__BackingField")]
        [field: SerializeField] public int ItemsPerRow { get; private set; } = 6;

        [field: Tooltip("Smallest radius of a ring, in meters. It grows when the assets would not fit.")]
        [field: Min(0f)]
        [field: SerializeField] public float CircleRadius { get; private set; } = 10f;
    }
}