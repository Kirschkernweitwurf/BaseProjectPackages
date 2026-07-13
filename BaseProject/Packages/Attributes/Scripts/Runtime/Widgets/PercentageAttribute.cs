using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Displays a normalized float that is stored in the zero to one range as a zero to one hundred
    /// percent value, and lets it be edited the same way. The underlying stored value stays normalized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PercentageAttribute : PropertyAttribute
    {
        /// <summary>Whether a draggable slider is drawn instead of a plain field.</summary>
        public bool Slider { get; }

        /// <summary>Creates the attribute, optionally drawing a slider.</summary>
        public PercentageAttribute(bool slider = false) => Slider = slider;
    }
}
