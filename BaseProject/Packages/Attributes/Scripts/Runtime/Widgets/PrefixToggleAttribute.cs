using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a bool member as a checkbox in front of this field and greys the field out while it is off.
    /// The bool loses its own row, which collapses the two-field "toggle plus value" pattern into one
    /// line.
    /// </summary>
    /// <remarks>
    /// The named member has to be a serialized bool on the same object, because the checkbox writes to
    /// it. A bool property or method can be read but not assigned, so those are rejected.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PrefixToggleAttribute : PropertyAttribute
    {
        /// <summary>Name of the bool field the checkbox drives.</summary>
        public string Member { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="member">Name of the bool field the checkbox drives.</param>
        public PrefixToggleAttribute(string member) => Member = member;
    }
}
