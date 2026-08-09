using System;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a button in the component header instead of the inspector body, so it costs no vertical
    /// space and stays reachable while the component is collapsed. The decorated method has to be
    /// parameterless. Buttons are laid out right to left in declaration order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HeaderButtonAttribute : Attribute
    {
        /// <summary>Width used when none is set explicitly. Fits a short label.</summary>
        public const float DefaultWidth = 60f;

        /// <summary>Optional label shown on the button. Falls back to the method name.</summary>
        public string Label { get; }

        /// <summary>
        /// Editor state in which the button is enabled. Defaults to <see cref="EButtonMode.Always"/>.
        /// </summary>
        public EButtonMode Mode { get; set; } = EButtonMode.Always;

        /// <summary>
        /// Optional confirmation message. When set, a dialog must be confirmed before the method runs.
        /// </summary>
        public string Confirm { get; set; }

        /// <summary>Width of the button in pixels.</summary>
        public float Width { get; set; } = DefaultWidth;

        /// <summary>Creates the attribute with an optional custom label.</summary>
        /// <param name="label">Label shown on the button.</param>
        public HeaderButtonAttribute(string label = null) => Label = label;
    }
}
