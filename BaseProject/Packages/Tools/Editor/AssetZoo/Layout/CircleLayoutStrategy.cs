using System.Collections.Generic;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Layout
{
    /// <summary>
    /// Stands assets on a ring, for looking at them from the middle.
    /// </summary>
    internal sealed class CircleLayoutStrategy : ILayoutStrategy
    {
        /// <inheritdoc/>
        public LayoutResult Layout(IReadOnlyList<Vector2> footprints, LayoutSettings settings)
        {
            int count = footprints.Count;
            float longest = 0f;

            foreach (Vector2 footprint in footprints)
                longest = Mathf.Max(longest, footprint.x, footprint.y);

            // A ring has to be big enough that its assets do not run into each other, and a single asset
            // has no ring to stand on at all.
            float neededRadius = count * (longest + settings.Spacing) / (2f * Mathf.PI);

            float radius = count > 1
                ? Mathf.Max(settings.CircleRadius, neededRadius)
                : 0f;

            float middle = radius + longest * 0.5f;
            Vector2[] positions = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                float angle = i / (float)count * Mathf.PI * 2f;
                positions[i] = new Vector2(middle + Mathf.Cos(angle) * radius, middle + Mathf.Sin(angle) * radius);
            }

            return new LayoutResult(positions, new Vector2(middle * 2f, middle * 2f));
        }
    }
}