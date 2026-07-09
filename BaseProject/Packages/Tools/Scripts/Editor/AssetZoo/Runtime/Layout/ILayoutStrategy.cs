using Base.ToolPackage.Editor.AssetZoo.Runtime.Config;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Layout
{
    /// <summary>
    /// Strategy that places N items into local space. Implement this to add new arrangements
    /// (rings, spirals, hex grids, etc.) without touching the builder.
    /// </summary>
    public interface ILayoutStrategy
    {
        LayoutResult Layout(int itemCount, Vector3 cellSize, LayoutSettings settings);
    }
}