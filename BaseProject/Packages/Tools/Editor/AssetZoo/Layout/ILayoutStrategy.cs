using System.Collections.Generic;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Layout
{
    /// <summary>
    /// Strategy that gives every asset of a group a place on the floor. Implement this to add new
    /// arrangements without touching the builder.
    /// </summary>
    internal interface ILayoutStrategy
    {
        /// <summary>Finds a place for every asset.</summary>
        /// <param name="footprints">How much floor each asset takes, x across and y into the zoo.</param>
        /// <param name="settings">The gaps and row length to use.</param>
        /// <returns>The middle of every footprint, and the size of the whole block.</returns>
        LayoutResult Layout(IReadOnlyList<Vector2> footprints, LayoutSettings settings);
    }
}