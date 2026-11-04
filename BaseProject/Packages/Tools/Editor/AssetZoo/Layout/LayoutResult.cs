using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Layout
{
    /// <summary>
    /// Output of a layout pass: where everything goes and how much floor it all takes. Everything is measured
    /// from the near left corner of the block, x across and y into the zoo, so no position is ever negative.
    /// </summary>
    internal readonly struct LayoutResult
    {
        /// <summary>
        /// The places, one per input, in the order they were given.
        /// </summary>
        public readonly Vector2[] Positions;

        /// <summary>
        /// The floor the whole block takes. Used to place the next block clear of this one.
        /// </summary>
        public readonly Vector2 Size;

        /// <summary>Creates a layout result from the computed positions and overall size.</summary>
        public LayoutResult(Vector2[] positions, Vector2 size)
        {
            Positions = positions;
            Size = size;
        }
    }
}