using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>Clamps an int or float field to a maximum.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MaxAttribute : PropertyAttribute
    {
        /// <summary>Highest allowed value.</summary>
        public float Max { get; }

        /// <summary>Creates the attribute with a maximum.</summary>
        public MaxAttribute(float max) => Max = max;
    }
}
