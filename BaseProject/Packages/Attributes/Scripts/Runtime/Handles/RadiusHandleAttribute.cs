using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a draggable circle in the scene view for a float field, so a range can be sized by eye.
    /// Attack range, detection radius and explosion size are the usual cases.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RadiusHandleAttribute : PropertyAttribute
    {
        /// <summary>Preset color of the circle.</summary>
        public EColor PresetColor { get; }

        /// <summary>Which axis the circle is oriented around.</summary>
        public ENormalAxis Axis { get; set; } = ENormalAxis.Y;

        /// <summary>
        /// Optional name of a Vector3 member the circle is centered on. Null centers it on the transform.
        /// </summary>
        public string PositionMember { get; set; }

        /// <summary>Whether the center offset is a local offset or a world position.</summary>
        public ESpace Space { get; set; } = ESpace.Local;

        /// <summary>Creates the attribute.</summary>
        /// <param name="color">Preset color of the circle.</param>
        public RadiusHandleAttribute(EColor color = EColor.Default) => PresetColor = color;
    }
}
