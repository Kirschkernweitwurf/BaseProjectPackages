using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Calls a method whenever the element count of an array or list changes in the inspector, for
    /// example <c>[OnArraySizeChanged(nameof(OnSlotsResized))]</c>. The method may be parameterless or
    /// take a single int parameter, which receives the new size. The edited size is applied to the
    /// target before the method runs. Element edits that keep the size do not trigger the callback;
    /// use <see cref="OnValueChangedAttribute"/> for those.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OnArraySizeChangedAttribute : PropertyAttribute
    {
        /// <summary>Name of the callback method to invoke.</summary>
        public string Method { get; }

        /// <summary>Creates the attribute referencing the given callback method.</summary>
        /// <param name="method">Name of the callback method to invoke.</param>
        public OnArraySizeChangedAttribute(string method) => Method = method;
    }
}
