using System;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeGallery
{
    /// <summary>Demo effect that fades over a duration. One of the picker's candidates.</summary>
    [Serializable]
    public sealed class GalleryFadeEffect : IGalleryEffect
    {
        [Title("Fade")]
        [SerializeField] [MinMax(0f, 10f)] [Suffix("s")] private float duration = 1f;

        /// <inheritdoc/>
        public string Description => $"Fade over {duration} seconds.";
    }
}