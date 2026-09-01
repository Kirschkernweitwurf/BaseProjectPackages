using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Alignment
{
    /// <summary>
    /// No offset. The prefab's authored pivot is honored.
    /// </summary>
    internal class PivotAlignment : IAlignmentStrategy
    {
        /// <inheritdoc/>
        public Vector3 GetOffset(Bounds prefabBounds) => Vector3.zero;
    }
}