using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Draws a wire circle in the scene view sized by this float field. Unlike
    /// <see cref="RadiusHandleAttribute"/> the circle cannot be dragged, so use it for a value that is
    /// computed rather than authored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DrawWireDiscAttribute : PropertyAttribute
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
        public DrawWireDiscAttribute(EColor color = EColor.Default) => PresetColor = color;
    }
}