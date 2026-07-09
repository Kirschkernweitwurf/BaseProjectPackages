using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Alignment
{
    /// <summary>
    /// Bottom of the bounding box rests on the slot. Best for props on a floor.
    /// </summary>
    public class GroundAlignment : IAlignmentStrategy
    {
        public Vector3 GetOffset(Bounds prefabBounds)
            => new(-prefabBounds.center.x, -prefabBounds.min.y, -prefabBounds.center.z);
    }
}