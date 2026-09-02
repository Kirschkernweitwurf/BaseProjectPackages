using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Alignment
{
    /// <summary>
    /// Computes a local-space offset for one prefab so it sits correctly on its slot.
    /// </summary>
    internal interface IAlignmentStrategy
    {
        Vector3 GetOffset(Bounds prefabBounds);
    }
}