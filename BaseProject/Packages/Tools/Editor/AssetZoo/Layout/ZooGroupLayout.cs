using System.Collections.Generic;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Layout
{
    /// <summary>
    /// Places whole groups next to each other.
    /// </summary>
    internal static class ZooGroupLayout
    {
        /// <summary>
        /// How much wider than a perfect square a zoo of blocks gets. A screen is wider than it is tall, so a
        /// slightly wide zoo fills the Scene view better when framed.
        /// </summary>
        private const float WidthBias = 1.25f;

        /// <summary>Finds the near left corner of every block.</summary>
        /// <param name="blocks">The floor each group takes, in the order they should stand in.</param>
        /// <param name="arrangement">How the groups stand next to each other.</param>
        /// <param name="gap">The gap between two groups.</param>
        /// <returns>The corner of every block and the floor the whole zoo takes.</returns>
        public static LayoutResult Arrange(IReadOnlyList<Vector2> blocks, EGroupArrangement arrangement, float gap)
        {
            float wrapWidth = CalculateWrapWidth(blocks, arrangement, gap);

            Vector2[] corners = new Vector2[blocks.Count];
            float across = 0f;
            float rowStart = 0f;
            float rowDepth = 0f;
            float width = 0f;

            for (int i = 0; i < blocks.Count; i++)
            {
                Vector2 block = blocks[i];

                // The first block of a row always stays, however wide it is, or a block wider than the
                // zoo would never find a row it fits in.
                if (across > 0f
                    && across + block.x > wrapWidth)
                {
                    across = 0f;
                    rowStart += rowDepth + gap;
                    rowDepth = 0f;
                }

                corners[i] = new Vector2(across, rowStart);
                width = Mathf.Max(width, across + block.x);
                rowDepth = Mathf.Max(rowDepth, block.y);
                across += block.x + gap;
            }

            return new LayoutResult(corners, new Vector2(width, rowStart + rowDepth));
        }

        private static float CalculateWrapWidth(IReadOnlyList<Vector2> blocks, EGroupArrangement arrangement,
            float gap)
        {
            if (arrangement == EGroupArrangement.Row)
                return float.PositiveInfinity;

            if (arrangement == EGroupArrangement.Column)
                return 0f;

            float area = 0f;

            foreach (Vector2 block in blocks)
                area += (block.x + gap) * (block.y + gap);

            return Mathf.Sqrt(area) * WidthBias;
        }
    }
}