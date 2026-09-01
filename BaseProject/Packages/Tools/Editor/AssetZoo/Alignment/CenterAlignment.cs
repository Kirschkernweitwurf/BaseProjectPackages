using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Alignment
{
    /// <summary>
    /// Bounds center sits on the slot. Best for floating items.
    /// </summary>
    internal class CenterAlignment : IAlignmentStrategy
    {
        /// <inheritdoc/>
        public Vector3 GetOffset(Bounds prefabBounds) => -prefabBounds.center;
    }
}