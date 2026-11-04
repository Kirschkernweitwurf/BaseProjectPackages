using System.Collections.Generic;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Layout
{
    /// <summary>
    /// Stands assets in rows like products on a shelf: every asset takes only the room it needs, and everything
    /// in a row lines up on the row's front edge. A shared cell the size of the biggest asset left a mug alone
    /// in a space made for a building, and made a whole library run for kilometers.
    /// </summary>
    internal sealed class RowLayoutStrategy : ILayoutStrategy
    {
        private readonly bool _isSingleRow;

        /// <summary>Creates the strategy.</summary>
        /// <param name="isSingleRow">True to keep every asset in one row, whatever the settings say.</param>
        public RowLayoutStrategy(bool isSingleRow) => _isSingleRow = isSingleRow;

        /// <inheritdoc/>
        public LayoutResult Layout(IReadOnlyList<Vector2> footprints, LayoutSettings settings)
        {
            int itemsPerRow = _isSingleRow
                ? Mathf.Max(1, footprints.Count)
                : Mathf.Max(1, settings.ItemsPerRow);

            Vector2[] positions = new Vector2[footprints.Count];
            float rowStart = 0f;
            float width = 0f;
            int index = 0;

            while (index < footprints.Count)
            {
                int rowEnd = Mathf.Min(index + itemsPerRow, footprints.Count);
                float across = 0f;
                float rowDepth = 0f;

                for (; index < rowEnd; index++)
                {
                    Vector2 footprint = footprints[index];
                    positions[index] = new Vector2(across + footprint.x * 0.5f, rowStart + footprint.y * 0.5f);

                    across += footprint.x + settings.Spacing;
                    rowDepth = Mathf.Max(rowDepth, footprint.y);
                }

                width = Mathf.Max(width, across - settings.Spacing);
                rowStart += rowDepth + settings.Spacing;
            }

            float depth = footprints.Count > 0
                ? rowStart - settings.Spacing
                : 0f;

            return new LayoutResult(positions, new Vector2(width, depth));
        }
    }
}