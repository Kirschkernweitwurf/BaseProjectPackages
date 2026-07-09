using System;
using UnityEngine;

namespace Base.AttributePackage.Validation
{
    /// <summary>
    /// Clamps an int or float field to the inclusive range without drawing a slider.
    /// Out-of-range values entered in the inspector are reset to the nearest bound.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxAttribute : PropertyAttribute
    {
        /// <summary>Lower bound.</summary>
        public float Min { get; }

        /// <summary>Upper bound.</summary>
        public float Max { get; }

        /// <summary>Creates the attribute with an inclusive minimum and maximum.</summary>
        public MinMaxAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}