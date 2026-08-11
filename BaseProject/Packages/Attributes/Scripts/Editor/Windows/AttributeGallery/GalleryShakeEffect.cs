using System;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeGallery
{
    /// <summary>Demo effect that shakes with an amplitude. One of the picker's candidates.</summary>
    [Serializable]
    public sealed class GalleryShakeEffect : IGalleryEffect
    {
        [Title("Shake")]
        [SerializeField] [MinMax(0f, 5f)] private float amplitude = 0.5f;

        [SerializeField] [Percentage(slider: true)] private float falloff = 0.5f;

        /// <inheritdoc/>
        public string Description => $"Shake at {amplitude} units, falling off to {falloff:P0}.";
    }
}