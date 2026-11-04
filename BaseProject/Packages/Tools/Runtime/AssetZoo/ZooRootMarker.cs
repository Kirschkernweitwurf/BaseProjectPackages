using UnityEngine;

namespace Base.ToolsPackage.AssetZoo
{
    /// <summary>
    /// Sits on the root of a built zoo. Lets the builder find "its" object by type instead of by name, which
    /// a rename would break, and keeps what the Scene view should draw, so the labels come back when the scene
    /// is opened again, with or without the builder window.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")] // hidden from the Add Component menu; only the builder adds this
    public sealed class ZooRootMarker : MonoBehaviour
    {
        [Tooltip("The floor built under the zoo. Empty while the zoo has none.")]
        [SerializeField] private Transform floor;

        [Tooltip("Draws the group names and zones in the Scene view.")]
        [SerializeField] private bool hasGroupNames = true;

        [Tooltip("Draws the asset names in the Scene view.")]
        [SerializeField] private bool hasItemNames = true;

        [Tooltip("Draws a ruler at the start of every group in the Scene view.")]
        [SerializeField] private bool hasRuler = true;

        [Tooltip("Adds size and triangle count to the asset names without having to hover them.")]
        [SerializeField] private bool hasSizes;

        /// <summary>The floor built under the zoo, or null while it has none.</summary>
        public Transform Floor => floor;

        /// <summary>True when the Scene view draws group names and zones.</summary>
        public bool HasGroupNames => hasGroupNames;

        /// <summary>True when the Scene view draws asset names.</summary>
        public bool HasItemNames => hasItemNames;

        /// <summary>True when the Scene view draws a ruler at the start of every group.</summary>
        public bool HasRuler => hasRuler;

        /// <summary>True when asset names also show the size and triangle count.</summary>
        public bool HasSizes => hasSizes;

        /// <summary>Remembers which labels the Scene view draws.</summary>
        /// <param name="groupNames">Draw group names and zones.</param>
        /// <param name="itemNames">Draw asset names.</param>
        /// <param name="sizes">Add the size and triangle count to the asset names.</param>
        /// <param name="ruler">Draw a ruler at the start of every group.</param>
        public void SetLabels(bool groupNames, bool itemNames, bool sizes, bool ruler)
        {
            hasGroupNames = groupNames;
            hasItemNames = itemNames;
            hasSizes = sizes;
            hasRuler = ruler;
        }

        /// <summary>Remembers the floor built under the zoo.</summary>
        /// <param name="newFloor">The floor object.</param>
        public void SetFloor(Transform newFloor) => floor = newFloor;
    }
}