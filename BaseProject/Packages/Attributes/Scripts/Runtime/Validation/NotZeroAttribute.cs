using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Keeps a field away from zero. Negative and positive values are both allowed, only zero is not,
    /// so -5 and 5 pass while 0 is pushed away. Works on int and float, and component wise on Vector2,
    /// Vector3, Vector2Int and Vector3Int, the same way <c>[Min]</c> handles vectors.
    /// A value that reaches zero is pushed back to the side it came from, so a field holding -5 becomes
    /// the negative step and a field holding 5 becomes the positive step.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NotZeroAttribute : PropertyAttribute
    {
        /// <summary>Distance from zero used when a value has to be pushed away from it.</summary>
        public float Step { get; }

        /// <summary>Creates the attribute with the distance kept from zero.</summary>
        public NotZeroAttribute(float step = 1f) => Step = step;
    }
}
