using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Layout
{
    /// <summary>
    /// Output of a layout pass: where each item goes and how big the whole block is.
    /// </summary>
    public struct LayoutResult
    {
        /// <summary>
        /// Local positions, one per item, in the category's local space.
        /// </summary>
        public readonly Vector3[] Positions;

        /// <summary>
        /// Bounding extent of the whole layout. Used to offset the next category.
        /// </summary>
        public readonly Vector3 TotalSize;

        public LayoutResult(Vector3[] positions, Vector3 totalSize)
        {
            Positions = positions;
            TotalSize = totalSize;
        }
    }
}