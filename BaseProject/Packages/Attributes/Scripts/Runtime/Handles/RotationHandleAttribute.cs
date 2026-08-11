using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a rotation gizmo in the scene view for a Quaternion field or a Vector3 field holding euler
    /// angles. The gizmo sits at the transform unless a position member is named.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RotationHandleAttribute : PropertyAttribute
    {
        /// <summary>Whether the stored rotation is relative to the transform or absolute.</summary>
        public ESpace Space { get; }

        /// <summary>
        /// Optional name of a Vector3 member the gizmo is drawn at. Null draws it at the transform.
        /// </summary>
        public string PositionMember { get; set; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="space">Whether the stored rotation is relative or absolute.</param>
        public RotationHandleAttribute(ESpace space = ESpace.Local) => Space = space;
    }
}