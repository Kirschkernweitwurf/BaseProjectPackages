using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws an int as a row of clickable stars. For the small bounded ratings that read better as a
    /// picture than as a number: difficulty, tier, quality.
    /// </summary>
    /// <remarks>
    /// Clicking the star that is already the current value clears back to the minimum, so a rating can
    /// be unset without typing.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RateAttribute : PropertyAttribute
    {
        /// <summary>Lowest value, which is also what clicking the current star falls back to.</summary>
        public int Min { get; }

        /// <summary>Highest value, and therefore the number of stars drawn.</summary>
        public int Max { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="min">Lowest value.</param>
        /// <param name="max">Highest value.</param>
        public RateAttribute(int min = 0, int max = 5)
        {
            Min = min;
            Max = Mathf.Max(min, max);
        }
    }
}
