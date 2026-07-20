using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Trims a string field to a maximum number of characters. Text pasted or typed beyond the limit
    /// is cut back to the limit after the field is edited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MaxLengthAttribute : PropertyAttribute
    {
        /// <summary>Highest allowed character count.</summary>
        public int Length { get; }

        /// <summary>Creates the attribute with a maximum character count.</summary>
        public MaxLengthAttribute(int length) => Length = length;
    }
}