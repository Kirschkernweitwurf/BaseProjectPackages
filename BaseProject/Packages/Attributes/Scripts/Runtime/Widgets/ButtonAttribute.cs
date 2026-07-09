using System;

namespace Base.AttributePackage.Widgets
{
    /// <summary>
    /// Draws an inspector button that invokes the decorated parameterless method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ButtonAttribute : Attribute
    {
        /// <summary>Optional label shown on the button. Falls back to the method name.</summary>
        public string Label { get; }

        /// <summary>Editor state in which the button is enabled. Defaults to <see cref="EButtonMode.Always"/>.</summary>
        public EButtonMode Mode { get; set; } = EButtonMode.Always;

        /// <summary>Optional confirmation message. When set, a dialog must be confirmed before the method runs.</summary>
        public string Confirm { get; set; }

        /// <summary>Creates the attribute with an optional custom label.</summary>
        public ButtonAttribute(string label = null) => Label = label;
    }
}