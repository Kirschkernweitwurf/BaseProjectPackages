using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.CanvasRenderOrder
{
    /// <summary>
    /// One canvas that sorts on its own: a root canvas, or a nested one with Override Sorting.
    /// </summary>
    internal sealed class CanvasEntry
    {
        /// <summary>The canvas itself. For a prefab it is the loaded asset's canvas, which is only read.</summary>
        internal Canvas Canvas { get; }

        /// <summary>The thing a prefab instance and its prefab share, so they are not counted as a tie.</summary>
        internal Object Identity { get; }

        /// <summary>Whether the canvas lives in an open scene or in a prefab asset.</summary>
        internal ECanvasSource Source { get; }

        /// <summary>Path of the prefab asset. Empty for scene canvases.</summary>
        internal string AssetPath { get; }

        /// <summary>Sibling index chain from the prefab root down to the canvas. Empty for scene canvases.</summary>
        internal int[] SiblingPath { get; }

        /// <summary>Name of the canvas object.</summary>
        internal string Name { get; }

        /// <summary>Name of the scene or prefab the canvas lives in.</summary>
        internal string Location { get; }

        /// <summary>Short line on how the canvas is drawn.</summary>
        internal string Detail { get; }

        /// <summary>The pass the canvas is drawn in.</summary>
        internal ECanvasPass Pass { get; }

        /// <summary>The display an overlay canvas is drawn on.</summary>
        internal int TargetDisplay { get; }

        /// <summary>Unique id of the sorting layer.</summary>
        internal int SortingLayerId { get; private set; }

        /// <summary>Order inside the sorting layer, or among the overlay canvases of a display.</summary>
        internal int SortingOrder { get; private set; }

        /// <summary>Whether another canvas in the same pass has the same layer and order.</summary>
        internal bool HasTie { get; set; }

        /// <summary>
        /// The key canvases are grouped by inside their pass: the sorting layer for the camera pass, the display
        /// for overlays. Overlay canvases ignore their sorting layer.
        /// </summary>
        internal int GroupKey => Pass == ECanvasPass.Overlay
            ? TargetDisplay
            : SortingLayer.GetLayerValueFromID(SortingLayerId);

        /// <summary>Creates an entry from a canvas and the values its scan resolved.</summary>
        internal CanvasEntry(Canvas canvas, Object identity, ECanvasSource source, string assetPath,
            int[] siblingPath, string location, string detail, ECanvasPass pass, int targetDisplay)
        {
            Canvas = canvas;
            Identity = identity;
            Source = source;
            AssetPath = assetPath ?? string.Empty;
            SiblingPath = siblingPath ?? Array.Empty<int>();
            Name = canvas.name;
            Location = location;
            Detail = detail;
            Pass = pass;
            TargetDisplay = targetDisplay;
            SortingLayerId = canvas.sortingLayerID;
            SortingOrder = canvas.sortingOrder;
        }

        /// <summary>
        /// Sorts entries back to front: pass, then layer or display, then order. Name and location only keep
        /// the list stable, Unity breaks those ties by camera distance or hierarchy order.
        /// </summary>
        internal static int CompareRenderOrder(CanvasEntry a, CanvasEntry b)
        {
            int result = a.Pass.CompareTo(b.Pass);

            if (result == 0)
                result = a.GroupKey.CompareTo(b.GroupKey);

            if (result == 0)
                result = a.SortingOrder.CompareTo(b.SortingOrder);

            if (result == 0)
                result = string.Compare(a.Location, b.Location, StringComparison.OrdinalIgnoreCase);

            return result == 0
                ? string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
                : result;
        }

        /// <summary>Whether both entries land on the same spot of the render order.</summary>
        internal bool IsSameSlot(CanvasEntry other) => other.Pass == Pass
            && other.GroupKey == GroupKey
            && other.SortingOrder == SortingOrder;

        /// <summary>Takes over the values that were just written to the canvas.</summary>
        internal void SetSorting(int sortingLayerId, int sortingOrder)
        {
            SortingLayerId = sortingLayerId;
            SortingOrder = sortingOrder;
        }
    }
}