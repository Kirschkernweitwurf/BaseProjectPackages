using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A small label in front of the value.</summary>
    [AttributeSample(typeof(PrefixAttribute), EAttributeCategory.Layout,
        Description = "Puts a small label in front of the value, for a qualifier that reads better "
            + "before the number than after it: an axis, a currency, a comparison.",
        Requirements = "Nothing.",
        Info = "Keep it to a word or two. The prefix takes room from the value field, so a long one "
            + "leaves nothing to type in.",
        Variations = new[]
        {
            "Any string works, though short ones read best.",
            "A shared constant keeps the same prefix spelled the same way across a project."
        })]
    internal sealed class PrefixSample : ScriptableObject
    {
        private const string Currency = "EUR";

        [Prefix("x")]
        [Tooltip("A multiplier, which reads as x2 rather than as 2x.")]
        public float multiplier = 2f;

        [Prefix(">=")]
        [Tooltip("A threshold, where the comparison belongs in front of the number.")]
        public int minimumScore = 100;

        [Prefix(Currency)]
        [Tooltip("A constant, so every price field in the project is prefixed identically.")]
        public float price = 9.99f;

        [Prefix("Speed")]
        [Tooltip("A plain word, for a value whose field name alone does not say what it measures.")]
        public float tuning = 3.5f;
    }
}