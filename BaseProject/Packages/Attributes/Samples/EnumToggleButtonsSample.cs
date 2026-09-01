using System;
using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>An enum shown as a row of buttons.</summary>
    [AttributeSample(typeof(EnumToggleButtonsAttribute), EAttributeCategory.Widgets,
        Description = "Draws an enum as a row of toolbar buttons instead of a dropdown, which saves a click and shows "
            + "every option at once. A flags enum becomes a row of multi-select toggles.",
        Requirements = "The field has to be an enum. Keep the option count low, since the row wraps once it runs out "
            + "of width.",
        Variations = new[]
        {
            "Works on a plain enum as a single-select row.",
            "Works on a flags enum as a multi-select row, where several buttons can be down at once."
        })]
    internal sealed class EnumToggleButtonsSample : ScriptableObject
    {
        /// <summary>Damage types, shown as a multi-select row.</summary>
        [Flags]
        public enum EElement : byte
        {
            /// <summary>Nothing selected.</summary>
            None = 0,

            /// <summary>Fire damage.</summary>
            Fire = 1,

            /// <summary>Ice damage.</summary>
            Ice = 2,

            /// <summary>Shock damage.</summary>
            Shock = 4
        }

        /// <summary>Modes shown as a single-select row.</summary>
        public enum EMode : byte
        {
            /// <summary>Nothing special.</summary>
            Simple = 0,

            /// <summary>The advanced settings apply.</summary>
            Advanced = 1
        }

        [EnumToggleButtons]
        [Tooltip("One option at a time.")]
        public EMode mode = EMode.Simple;

        [EnumToggleButtons]
        [Tooltip("A flags enum, so several can be down at once.")]
        public EElement elements = EElement.Fire;
    }
}