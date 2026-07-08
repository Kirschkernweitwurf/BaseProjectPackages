using System;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws an inspector button that invokes the decorated parameterless method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ButtonAttribute : Attribute
    {
        /// <summary>Optional label shown on the button. Falls back to the method name.</summary>
        public string Label { get; }

        /// <summary>Creates the attribute with an optional custom label.</summary>
        public ButtonAttribute(string label = null) => Label = label;
    }
}