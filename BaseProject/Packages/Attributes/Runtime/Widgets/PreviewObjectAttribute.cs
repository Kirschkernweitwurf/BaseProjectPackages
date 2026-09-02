using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Draws a large preview of a referenced asset below its field, big enough to actually judge, and
    /// interactive where the asset supports it.
    /// </summary>
    /// <remarks>
    /// The thumbnail from <see cref="ShowAssetPreviewAttribute"/> answers "is something assigned". This
    /// answers "is it the right one", which for a mesh or a prefab usually means turning it round.
    /// <para>
    /// Interactive previews come from the asset's own preview, so a mesh or a prefab can be rotated and
    /// a texture cannot. That is Unity's behavior, not a limit of the attribute.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PreviewObjectAttribute : PropertyAttribute
    {
        private const float DefaultHeight = 128f;

        /// <summary>Height of the preview in pixels.</summary>
        public float Height { get; }

        /// <summary>Width of the preview, or zero to fill the inspector.</summary>
        public float Width { get; set; }

        /// <summary>Whether the preview sits behind a foldout rather than always being drawn.</summary>
        public bool Foldout { get; set; }

        /// <summary>Whether that foldout starts open. Ignored while <see cref="Foldout"/> is false.</summary>
        public bool DefaultExpanded { get; set; } = true;

        /// <summary>
        /// Whether the preview can be dragged to rotate. Falls back to a still image for assets that
        /// have no interactive preview.
        /// </summary>
        public bool Interactive { get; set; } = true;

        /// <summary>Creates the attribute.</summary>
        /// <param name="height">Height of the preview in pixels.</param>
        public PreviewObjectAttribute(float height = DefaultHeight) => Height = height;
    }
}