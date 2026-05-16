using Base.ToolPackage.Editor.AssetZoo.Runtime.Config;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Layout
{
    /// <summary>
    /// Layout strategy that arranges items in a grid pattern,
    /// with a specified number of columns and spacing defined in the settings.
    /// </summary>
    public class GridLayoutStrategy : ILayoutStrategy
    {
        /// <summary>
        /// Lays out items in a grid pattern. The number of columns is determined by the settings,
        /// and the number of rows is calculated based on the total item count and the number of columns.
        /// Each item is spaced according to the cell size and the spacing defined in the settings.
        /// </summary>
        /// <param name="itemCount">The total number of items to layout in the grid.</param>
        /// <param name="cellSize">The size of each cell in the grid, used to calculate spacing between items.</param>
        /// <param name="settings">The layout settings containing the number of columns and spacing information.</param>
        /// <returns></returns>
        public LayoutResult Layout(int itemCount, Vector3 cellSize, LayoutSettings settings)
        {
            int cols = Mathf.Max(1, settings.GridColumns);
            int rows = Mathf.CeilToInt(itemCount / (float)cols);

            float stepX = cellSize.x + settings.Spacing;
            float stepZ = cellSize.z + settings.Spacing;

            Vector3[] positions = new Vector3[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                int row = i / cols;
                int col = i % cols;
                positions[i] = new Vector3(col * stepX, 0f, row * stepZ);
            }

            return new LayoutResult(positions, new Vector3(cols * stepX, cellSize.y, rows * stepZ));
        }
    }
}