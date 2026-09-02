using System;
using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A nested type drawn on one row instead of behind a foldout.</summary>
    [AttributeSample(typeof(InlinePropertyAttribute), EAttributeCategory.Layout,
        Description = "Draws a nested serializable type on the field own row instead of behind a foldout, for the "
            + "small pairs where the foldout costs more room than it saves.",
        Requirements = "The field type has to be marked Serializable and be small enough to fit on a row.",
        Variations = new[]
        {
            "Works at any depth, so an inline type inside an inline type stays on one row."
        })]
    internal sealed class InlinePropertySample : ScriptableObject
    {
        [InlineProperty]
        [Tooltip("Both numbers sit on this row rather than behind a foldout.")]
        public Range range = new();

        [Tooltip("The same type without the attribute, for comparison.")]
        public Range foldedOut = new();

        /// <summary>Two numbers, small enough that a foldout costs more than it shows.</summary>
        [Serializable]
        public sealed class Range
        {
            /// <summary>Low end.</summary>
            public float min = 1f;

            /// <summary>High end.</summary>
            public float max = 5f;
        }
    }
}