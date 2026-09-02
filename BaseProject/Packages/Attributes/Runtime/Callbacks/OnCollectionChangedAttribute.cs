using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Calls a method before and after the element count of an array or list changes in the inspector,
    /// for example <c>[OnCollectionChanged(nameof(Before), nameof(After))]</c>.
    /// </summary>
    /// <remarks>
    /// The difference from <see cref="OnArraySizeChangedAttribute"/> is the before half. A collection
    /// that owns something, a pool, a set of spawned objects, a registration, has to release what is
    /// leaving before the list forgets it exists, and after the change the old contents are gone. Each
    /// method may be parameterless or take a single int, which receives the size at that moment.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OnCollectionChangedAttribute : PropertyAttribute
    {
        /// <summary>Name of the method called before the change, or null.</summary>
        public string Before { get; }

        /// <summary>Name of the method called after the change, or null.</summary>
        public string After { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="before">Name of the method called before the change.</param>
        /// <param name="after">Name of the method called after the change.</param>
        public OnCollectionChangedAttribute(string before, string after = null)
        {
            Before = before;
            After = after;
        }
    }
}