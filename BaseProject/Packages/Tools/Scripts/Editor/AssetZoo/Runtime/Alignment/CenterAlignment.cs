using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Alignment
{
    /// <summary>
    /// Bounds center sits on the slot. Best for floating items.
    /// </summary>
    public class CenterAlignment : IAlignmentStrategy
    {
        public Vector3 GetOffset(Bounds prefabBounds) => -prefabBounds.center;
    }
}