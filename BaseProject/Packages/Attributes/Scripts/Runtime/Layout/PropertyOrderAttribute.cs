using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Moves a field up or down in the inspector without moving it in the file.
    /// </summary>
    /// <remarks>
    /// Unity draws serialized fields in declaration order, which is also the order they are written to
    /// disk in. Reordering a class to make the inspector read better therefore changes serialized data
    /// layout for a purely cosmetic reason. This separates the two: the file keeps the order that suits
    /// the code, the inspector gets the order that suits the reader.
    /// <para>
    /// Fields sort by order and keep their declared order within the same value, so a single attribute
    /// moves one field without disturbing anything else. The default is zero, so an unmarked field sits
    /// between the negatives and the positives.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PropertyOrderAttribute : PropertyAttribute
    {
        /// <summary>Sort position. Lower comes first; the default for an unmarked field is zero.</summary>
        public int Order { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="order">Sort position.</param>
        public PropertyOrderAttribute(int order) => Order = order;
    }
}