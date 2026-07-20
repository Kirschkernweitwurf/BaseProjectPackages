using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a Vector2 field as a min-max range slider. X holds the low value, Y the high value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxSliderAttribute : PropertyAttribute
    {
        /// <summary>Lower limit of the slider.</summary>
        public float Min { get; }

        /// <summary>Upper limit of the slider.</summary>
        public float Max { get; }

        /// <summary>Creates the attribute with the slider limits.</summary>
        public MinMaxSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}