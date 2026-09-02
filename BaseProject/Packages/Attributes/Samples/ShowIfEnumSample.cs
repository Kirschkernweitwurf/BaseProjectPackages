using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field visible only for certain enum values.</summary>
    [AttributeSample(typeof(ShowIfEnumAttribute), EAttributeCategory.Conditions,
        Description = "Shows the field only while an enum member equals one of the given values, which is how most "
            + "conditions read once a component has more than two modes.",
        Requirements = "The member it names has to be an enum field or property on the same object.",
        Variations = new[]
        {
            "One value, or several, which are treated as a list of accepted values.",
            "Works with flag enums too, where a value matches when its bits are set."
        })]
    internal sealed class ShowIfEnumSample : ScriptableObject
    {
        /// <summary>The modes the fields above react to.</summary>
        public enum EMode : byte
        {
            /// <summary>Nothing special.</summary>
            Simple = 0,

            /// <summary>The advanced settings apply.</summary>
            Advanced = 1,

            /// <summary>Neither of the conditional fields applies.</summary>
            Disabled = 2
        }

        [Tooltip("Drives the fields below. Switch it and watch them swap.")]
        public EMode mode = EMode.Simple;

        [ShowIfEnum(nameof(mode), EMode.Advanced)]
        [Tooltip("Visible only in Advanced.")]
        public float tolerance = 0.01f;

        [ShowIfEnum(nameof(mode), EMode.Simple, EMode.Advanced)]
        [Tooltip("Visible in either of the two named modes, but not in Disabled.")]
        public string shownInBoth = "Simple or Advanced";
    }
}