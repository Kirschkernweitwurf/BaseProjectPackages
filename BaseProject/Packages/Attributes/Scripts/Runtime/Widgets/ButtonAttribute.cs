using System;
using JetBrains.Annotations;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws an inspector button that invokes the decorated parameterless method.
    /// </summary>
    /// <remarks>
    /// <see cref="MeansImplicitUseAttribute"/> tells Rider that a decorated method is called even though
    /// nothing in the codebase references it. Without it, every inspector button reads as dead code.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public sealed class ButtonAttribute : Attribute
    {
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

        /// <summary>Creates the attribute with an optional custom label.</summary>
        /// <param name="label">Label shown on the button.</param>
        public ButtonAttribute(string label = null) => Label = label;
    }
}
