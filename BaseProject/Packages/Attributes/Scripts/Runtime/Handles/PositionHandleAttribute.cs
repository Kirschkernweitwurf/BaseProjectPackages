using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a movable position gizmo in the scene view for a Vector3 field, so a point can be dragged
    /// instead of typed. Defaults to local space, which is what an offset stored on a component almost
    /// always means.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PositionHandleAttribute : PropertyAttribute
    {
        /// <summary>Whether the stored value is a local offset or a world position.</summary>
        public ESpace Space { get; }

        /// <summary>Optional label drawn next to the gizmo. Null draws no label.</summary>
        public string Label { get; set; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="space">Whether the stored value is a local offset or a world position.</param>
        public PositionHandleAttribute(ESpace space = ESpace.Local) => Space = space;
    }
}
