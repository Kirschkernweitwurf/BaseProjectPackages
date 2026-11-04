using UnityEngine;

namespace Base.ToolsPackage.AssetZoo
{
    /// <summary>
    /// One block of a built zoo. The Scene view reads its name, color and size to draw the zone and the name
    /// on the floor, and they have to survive the scene being closed and opened again.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")] // hidden from the Add Component menu; only the builder adds this
    public sealed class ZooGroup : MonoBehaviour
    {
        [Tooltip("How many assets of the group are showing.")]
        [SerializeField] private int assetCount;

        [Tooltip("The color of the zone and of the dot next to the name.")]
        [SerializeField] private Color color = Color.white;

        [Tooltip("The name of the group.")]
        [SerializeField] private string groupName;

        [Tooltip("The footprint of the block in meters, x across and y into the zoo.")]
        [SerializeField] private Vector2 size;

        /// <summary>How many assets of the group are showing.</summary>
        public int AssetCount => assetCount;

        /// <summary>The color of the zone and of the dot next to the name.</summary>
        public Color Color => color;

        /// <summary>The name of the group.</summary>
        public string GroupName => groupName;

        /// <summary>The footprint of the block in meters, x across and y into the zoo.</summary>
        public Vector2 Size => size;

        /// <summary>Fills in the group when it is built.</summary>
        /// <param name="newName">The name of the group.</param>
        /// <param name="newColor">The color of its zone.</param>
        public void Setup(string newName, Color newColor)
        {
            groupName = newName;
            color = newColor;
        }

        /// <summary>Remembers the result of the last layout.</summary>
        /// <param name="newSize">The footprint of the block in meters.</param>
        /// <param name="newAssetCount">How many assets are showing.</param>
        public void SetLayout(Vector2 newSize, int newAssetCount)
        {
            size = newSize;
            assetCount = newAssetCount;
        }
    }
}