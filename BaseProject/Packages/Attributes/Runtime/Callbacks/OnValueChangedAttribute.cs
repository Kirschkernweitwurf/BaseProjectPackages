using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Calls a method whenever the field value changes in the inspector. The method is referenced by
    /// name, for example <c>[OnValueChanged(nameof(OnHealthChanged))]</c>, and may be parameterless or
    /// take a single parameter of the field type, which receives the new value. The edited value is
    /// applied to the target before the method runs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OnValueChangedAttribute : PropertyAttribute
    {
        /// <summary>Name of the callback method to invoke.</summary>
        public string Method { get; }

        /// <summary>Creates the attribute referencing the given callback method.</summary>
        public OnValueChangedAttribute(string method) => Method = method;
    }
}