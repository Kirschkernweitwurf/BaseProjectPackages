using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Alignment
{
    /// <summary>
    /// Decides which point of an asset stands on its place in the zoo.
    /// </summary>
    internal interface IAlignmentStrategy
    {
        /// <summary>Where the asset's pivot goes, relative to its place.</summary>
        /// <param name="prefabBounds">The size of what you see, with the pivot at zero.</param>
        /// <returns>The offset to give the instance.</returns>
        Vector3 GetOffset(Bounds prefabBounds);

        /// <summary>How much floor the asset takes when placed this way, so it clears its neighbors.</summary>
        /// <param name="prefabBounds">The size of what you see, with the pivot at zero.</param>
        /// <returns>The footprint, x across and y into the zoo.</returns>
        Vector2 GetFootprint(Bounds prefabBounds);
    }
}