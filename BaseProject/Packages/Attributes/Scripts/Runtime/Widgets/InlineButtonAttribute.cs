using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a button next to the field that invokes a parameterless method on the same object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class InlineButtonAttribute : PropertyAttribute
    {
        /// <summary>Name of the method to invoke.</summary>
        public string Method { get; }

        /// <summary>Optional button label. Falls back to the method name.</summary>
        public string Label { get; }

        /// <summary>Creates the attribute with a method name and an optional label.</summary>
        public InlineButtonAttribute(string method, string label = null)
        {
            Method = method;
            Label = label;
        }
    }
}