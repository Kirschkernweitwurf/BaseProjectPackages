using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a line in the scene view from the transform, or from another Vector3 member, to the value of
    /// this Vector3 field. Read-only: it visualizes a relationship rather than editing it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DrawLineAttribute : PropertyAttribute
    {
        /// <summary>Preset color of the line.</summary>
        public EColor PresetColor { get; }

        /// <summary>
        /// Optional name of a Vector3 member the line starts at. Null starts it at the transform.
        /// </summary>
        public string FromMember { get; set; }

        /// <summary>Whether the endpoints are local offsets or world positions.</summary>
        public ESpace Space { get; set; } = ESpace.Local;

        /// <summary>Whether the line is dotted rather than solid.</summary>
        public bool Dotted { get; set; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="color">Preset color of the line.</param>
        public DrawLineAttribute(EColor color = EColor.Default) => PresetColor = color;
    }
}
