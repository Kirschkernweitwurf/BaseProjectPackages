using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Alignment
{
    /// <summary>
    /// No offset. The prefab's authored pivot is honored.
    /// </summary>
    public class PivotAlignment : IAlignmentStrategy
    {
        public Vector3 GetOffset(Bounds prefabBounds) => Vector3.zero;
    }
}