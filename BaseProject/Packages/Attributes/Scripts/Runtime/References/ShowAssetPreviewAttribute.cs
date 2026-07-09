using System;
using UnityEngine;

namespace Base.AttributePackage.References
{
    /// <summary>
    /// Shows a thumbnail preview below an assigned asset reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShowAssetPreviewAttribute : PropertyAttribute
    {
        /// <summary>Preview size in pixels.</summary>
        public int Size { get; }

        /// <summary>Creates the attribute with an optional preview size.</summary>
        public ShowAssetPreviewAttribute(int size = 64) => Size = size;
    }
}