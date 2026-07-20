using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Clamps a field to a maximum. Works on int and float, and component wise on Vector2, Vector3,
    /// Vector2Int and Vector3Int, mirroring how Unity's own <c>[Min]</c> handles vectors.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MaxAttribute : PropertyAttribute
    {
        /// <summary>Highest allowed value, applied to each component of a vector.</summary>
        public float Max { get; }

        /// <summary>Creates the attribute with a maximum.</summary>
        public MaxAttribute(float max) => Max = max;
    }
}