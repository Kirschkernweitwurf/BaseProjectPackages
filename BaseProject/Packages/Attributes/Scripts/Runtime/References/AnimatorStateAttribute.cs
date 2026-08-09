using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a dropdown of the states of a sibling Animator's controller, for example
    /// <c>[AnimatorState(nameof(animator))]</c>. On a string field the state name is stored, on an int
    /// field the state hash. States of every layer are listed, prefixed by their layer name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AnimatorStateAttribute : PropertyAttribute
    {
        /// <summary>Separator between the layer name and the state name in the dropdown.</summary>
        public const string LayerSeparator = ".";

        /// <summary>Name of the sibling Animator field the states are read from.</summary>
        public string AnimatorField { get; }

        /// <summary>Creates the attribute referencing the given Animator field.</summary>
        /// <param name="animatorField">Name of the sibling Animator field.</param>
        public AnimatorStateAttribute(string animatorField) => AnimatorField = animatorField;
    }
}
