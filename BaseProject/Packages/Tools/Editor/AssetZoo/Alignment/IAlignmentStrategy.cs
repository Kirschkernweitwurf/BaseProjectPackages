using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Alignment
{
    /// <summary>
    /// Computes a local-space offset for one prefab so it sits correctly on its slot.
    /// </summary>
    internal interface IAlignmentStrategy
    {
        Vector3 GetOffset(Bounds prefabBounds);
    }
}