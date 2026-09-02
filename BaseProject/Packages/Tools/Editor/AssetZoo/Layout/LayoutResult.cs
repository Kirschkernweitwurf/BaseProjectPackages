using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Layout
{
    /// <summary>
    /// Output of a layout pass: where each item goes and how big the whole block is.
    /// </summary>
    internal struct LayoutResult
    {
        /// <summary>
        /// Local positions, one per item, in the category's local space.
        /// </summary>
        public readonly Vector3[] Positions;

        /// <summary>
        /// Bounding extent of the whole layout. Used to offset the next category.
        /// </summary>
        public readonly Vector3 TotalSize;

        /// <summary>Creates a layout result from the computed positions and overall size.</summary>
        public LayoutResult(Vector3[] positions, Vector3 totalSize)
        {
            Positions = positions;
            TotalSize = totalSize;
        }
    }
}