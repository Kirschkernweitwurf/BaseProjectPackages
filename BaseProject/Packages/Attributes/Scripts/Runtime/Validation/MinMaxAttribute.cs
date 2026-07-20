using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Clamps a field to the inclusive range without drawing a slider. Works on int and float, and
    /// component wise on Vector2, Vector3, Vector2Int and Vector3Int, mirroring how Unity's own
    /// <c>[Min]</c> handles vectors. Out-of-range values are reset to the nearest bound.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxAttribute : PropertyAttribute
    {
        /// <summary>Lower bound, applied to each component of a vector.</summary>
        public float Min { get; }

        /// <summary>Upper bound, applied to each component of a vector.</summary>
        public float Max { get; }

        /// <summary>Creates the attribute with an inclusive minimum and maximum.</summary>
        public MinMaxAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}