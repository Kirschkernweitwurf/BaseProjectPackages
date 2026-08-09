using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a dropdown of the project layers for a single layer value. On an int field the layer index
    /// is stored, on a string field the layer name. Use a <c>LayerMask</c> field instead when several
    /// layers have to be selected at once; this attribute exists for the single-layer case Unity has no
    /// picker for, such as assigning <c>GameObject.layer</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LayerAttribute : PropertyAttribute { }
}
