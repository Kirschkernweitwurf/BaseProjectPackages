using System;
using UnityEngine;

namespace Base.AttributePackage.References
{
    /// <summary>
    /// Draws a dropdown of the parameters of a sibling Animator field. On a string field it stores the
    /// parameter name, on an int field it stores the parameter hash.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AnimatorParamAttribute : PropertyAttribute
    {
        /// <summary>Name of the Animator field on the same object.</summary>
        public string AnimatorField { get; }

        /// <summary>Optional parameter type filter.</summary>
        public AnimatorControllerParameterType Type { get; }

        /// <summary>Whether the type filter is active.</summary>
        public bool HasFilter { get; }

        /// <summary>Creates the attribute referencing the Animator field, without a type filter.</summary>
        public AnimatorParamAttribute(string animatorField)
        {
            AnimatorField = animatorField;
            HasFilter = false;
        }

        /// <summary>Creates the attribute referencing the Animator field, filtered by parameter type.</summary>
        public AnimatorParamAttribute(string animatorField, AnimatorControllerParameterType type)
        {
            AnimatorField = animatorField;
            Type = type;
            HasFilter = true;
        }
    }
}