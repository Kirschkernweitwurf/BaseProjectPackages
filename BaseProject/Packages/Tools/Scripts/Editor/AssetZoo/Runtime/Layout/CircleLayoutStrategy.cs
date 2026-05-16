using Base.ToolPackage.Editor.AssetZoo.Runtime.Config;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Layout
{
    /// <summary>
    /// Layout strategy that arranges items in a circle around the origin, with a radius defined in the settings.
    /// </summary>
    public class CircleLayoutStrategy : ILayoutStrategy
    {
        /// <summary>
        /// Lays out items in a circle around the origin.
        /// The radius of the circle is determined by the settings,
        /// but is automatically increased if necessary to prevent items
        /// from overlapping based on their size and the spacing defined in the settings.
        /// </summary>
        /// <param name="itemCount"></param>
        /// <param name="cellSize"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public LayoutResult Layout(int itemCount, Vector3 cellSize, LayoutSettings settings)
        {
            // Make sure the ring is big enough that items don't overlap.
            float minRadius = itemCount > 1
                ? itemCount * (cellSize.x + settings.Spacing) / (2f * Mathf.PI)
                : 0f;
            float radius = Mathf.Max(settings.CircleRadius, minRadius);

            Vector3[] positions = new Vector3[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                float angle = i / Mathf.Max(1f, itemCount) * Mathf.PI * 2f;
                positions[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }

            float diameter = radius * 2f + cellSize.x;
            return new LayoutResult(positions, new Vector3(diameter, cellSize.y, diameter));
        }
    }
}