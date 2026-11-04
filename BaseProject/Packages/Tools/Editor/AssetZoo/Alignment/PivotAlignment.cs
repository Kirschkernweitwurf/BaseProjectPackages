using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Alignment
{
    /// <summary>
    /// No offset. The prefab's authored pivot is honored.
    /// </summary>
    internal class PivotAlignment : IAlignmentStrategy
    {
        /// <inheritdoc/>
        public Vector3 GetOffset(Bounds prefabBounds) => Vector3.zero;

        /// <inheritdoc/>
        public Vector2 GetFootprint(Bounds prefabBounds)
        {
            // The pivot stands on the middle of the footprint, so a pivot off to one side needs room on both
            // sides of it, or the asset reaches into its neighbor.
            Vector3 min = prefabBounds.min;
            Vector3 max = prefabBounds.max;

            return new Vector2(2f * Mathf.Max(Mathf.Abs(min.x), Mathf.Abs(max.x)),
                2f * Mathf.Max(Mathf.Abs(min.z), Mathf.Abs(max.z)));
        }
    }
}