using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Locks the element count of an array or list. Use a fixed size for the arrays whose indices carry
    /// meaning, such as one entry per enum value, where a removed row silently breaks every lookup that
    /// indexes into it.
    /// </summary>
    /// <remarks>
    /// The add and remove controls of <see cref="ListDrawerSettingsAttribute"/> and
    /// <see cref="TableAttribute"/> switch themselves off on a fixed-size field, since a button that
    /// cannot change anything is worse than no button.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ArraySizeAttribute : PropertyAttribute
    {
        /// <summary>Value meaning "no limit on this end".</summary>
        public const int Unbounded = -1;

        /// <summary>Exact element count, or <see cref="Unbounded"/> when a range is used instead.</summary>
        public int Size { get; }

        /// <summary>Smallest allowed element count.</summary>
        public int Min { get; set; } = Unbounded;

        /// <summary>Largest allowed element count.</summary>
        public int Max { get; set; } = Unbounded;

        /// <summary>True when the count cannot be changed at all.</summary>
        public bool IsFixed => Size >= 0 || (Min >= 0 && Min == Max);

        /// <summary>Creates the attribute.</summary>
        /// <param name="size">Exact element count, or nothing to use <see cref="Min"/> and <see cref="Max"/>.</param>
        public ArraySizeAttribute(int size = Unbounded) => Size = size;
    }
}
