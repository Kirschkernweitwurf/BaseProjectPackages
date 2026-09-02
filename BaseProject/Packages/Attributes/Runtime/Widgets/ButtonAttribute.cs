using System;
using JetBrains.Annotations;

namespace Base.AttributesPackage
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

        /// <summary>How tall the button is drawn. Defaults to <see cref="EButtonSize.Normal"/>.</summary>
        public EButtonSize Size { get; set; } = EButtonSize.Normal;

        /// <summary>
        /// Optional name of a row. Consecutive buttons sharing one are drawn side by side, which is what
        /// a pair of opposites wants: apply and revert, start and stop.
        /// </summary>
        public string Row { get; set; }

        /// <summary>
        /// Optional name of a collapsible block. Consecutive buttons sharing one fold away together,
        /// which is where the debug actions belong once there are more than two of them.
        /// </summary>
        public string Foldout { get; set; }

        /// <summary>
        /// Whether that block starts open. Ignored while <see cref="Foldout"/> is null.
        /// </summary>
        public bool DefaultExpanded { get; set; }

        /// <summary>
        /// Preset color of that block's heading, so a group of buttons can be given the same weight as a
        /// section of fields. Read from whichever button of the block is found first.
        /// </summary>
        public EColor FoldoutColor { get; set; } = EColor.Default;

        /// <summary>
        /// HTML color of that block's heading, for example "#FFB2F0". Takes precedence over
        /// <see cref="FoldoutColor"/>, and exists so a block can sit in a palette the presets are not
        /// part of.
        /// </summary>
        public string FoldoutColorHex { get; set; }

        /// <summary>Creates the attribute with an optional custom label.</summary>
        /// <param name="label">Label shown on the button.</param>
        public ButtonAttribute(string label = null) => Label = label;
    }
}