using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Shifts a field one or more steps to the right, or to the left with a negative amount.
    /// </summary>
    /// <remarks>
    /// Pulling left is what gets a field back out of a block it is drawn inside but does not belong to,
    /// which happens whenever a nested type ends with a summary line that reads as belonging to the
    /// parent. The result is clamped at zero, since there is nothing to the left of the first column.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IndentAttribute : PropertyAttribute
    {
        /// <summary>Number of steps to shift by. Negative pulls left.</summary>
        public int Amount { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="amount">Number of steps to shift by. Negative pulls left.</param>
        public IndentAttribute(int amount = 1) => Amount = amount;
    }
}