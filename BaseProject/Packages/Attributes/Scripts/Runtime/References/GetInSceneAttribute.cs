using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Fills an empty reference from anywhere in the open scenes, rather than from this object's own
    /// hierarchy. For the manager-shaped objects a component needs a handle on but does not own.
    /// </summary>
    /// <remarks>
    /// The result is cached until the hierarchy changes, and the search only runs while the field is
    /// empty. This is still the most expensive of the auto-getters, and a reference filled this way goes
    /// stale when the object moves to another scene, so prefer a real lookup at runtime where you can.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GetInSceneAttribute : PropertyAttribute
    {
        /// <summary>Whether inactive objects are searched too.</summary>
        public bool IncludeInactive { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="includeInactive">Whether inactive objects are searched too.</param>
        public GetInSceneAttribute(bool includeInactive = true) => IncludeInactive = includeInactive;
    }
}
