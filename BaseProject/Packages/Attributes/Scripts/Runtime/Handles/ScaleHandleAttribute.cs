using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a scale gizmo in the scene view for a Vector3 field. The gizmo sits at the transform unless
    /// a position member is named.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ScaleHandleAttribute : PropertyAttribute
    {
        /// <summary>Size the gizmo is drawn at, independent of the value it edits.</summary>
        public float Size { get; set; } = DefaultSize;

        /// <summary>
        /// Optional name of a Vector3 member the gizmo is drawn at. Null draws it at the transform.
        /// </summary>
        public string PositionMember { get; set; }

        private const float DefaultSize = 1f;
    }
}