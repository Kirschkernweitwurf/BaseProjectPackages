using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Draws a dropdown of the project sorting layers. On a string field the layer name is stored, on
    /// an int field the layer id as used by <c>Renderer.sortingLayerID</c>. Unity offers no picker for
    /// these outside its own renderer inspectors, which makes the names easy to mistype.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SortingLayerAttribute : PropertyAttribute { }
}