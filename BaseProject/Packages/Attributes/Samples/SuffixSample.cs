using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A small label after the value, almost always a unit.</summary>
    [AttributeSample(typeof(SuffixAttribute), EAttributeCategory.Layout,
        Description = "Puts a small label after the value, almost always a unit. The constants cover the common units, "
            + "so the same unit is spelled the same way on every field in the project that uses it.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Suffix(SuffixAttribute.Second) and the other constants for the standard units.",
            "Suffix(text) for anything the constants do not cover."
        })]
    internal sealed class SuffixSample : ScriptableObject
    {
        [Suffix(SuffixAttribute.MetersPerSecond)]
        [Tooltip("A unit taken from the shared list.")]
        public float speed = 7f;

        [Suffix(SuffixAttribute.Second)]
        [Tooltip("Another one, spelled the same way everywhere it is used.")]
        public float duration = 1.5f;

        [Suffix("rounds")]
        [Tooltip("A free label, for the units the constants do not cover.")]
        public int capacity = 6;
    }
}