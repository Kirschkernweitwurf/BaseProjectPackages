using Base.ToolPackage.Editor.AssetZoo.Config;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Layout
{
    /// <summary>
    /// Layout strategy that arranges items in a straight line along the X-axis, with spacing defined in the settings.
    /// </summary>
    public class LineLayoutStrategy : ILayoutStrategy
    {
        /// <summary>
        /// Lays out items in a line along the X-axis. Each item is spaced
        /// according to the cell size and the spacing defined in the settings.
        /// </summary>
        /// <param name="itemCount">The number of items to layout.</param>
        /// <param name="cellSize">The size of each cell, used to calculate spacing between items.</param>
        /// <param name="settings">The layout settings containing spacing information.</param>
        /// <returns>
        /// A <see cref="LayoutResult"/> containing the positions of each item and the total size of the layout.
        /// </returns>
        public LayoutResult Layout(int itemCount, Vector3 cellSize, LayoutSettings settings)
        {
            float step = cellSize.x + settings.Spacing;
            Vector3[] positions = new Vector3[itemCount];
            for (int i = 0; i < itemCount; i++)
                positions[i] = new Vector3(i * step, 0f, 0f);

            return new LayoutResult(positions, new Vector3(itemCount * step, cellSize.y, cellSize.z));
        }
    }
}