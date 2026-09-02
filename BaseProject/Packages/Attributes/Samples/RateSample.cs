using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A small count shown as stars.</summary>
    [AttributeSample(typeof(RateAttribute), EAttributeCategory.Widgets,
        Description = "Shows a small integer as a row of stars, for a value picked by feel rather than measured.",
        Requirements = "The field has to be an int.",
        Variations = new[]
        {
            "Rate() for zero to five.",
            "Rate(min, max) for any other span, though the row stops being readable much past ten."
        })]
    internal sealed class RateSample : ScriptableObject
    {
        [Rate(1)]
        [Tooltip("Click a star to set the value.")]
        public int difficulty = 3;
    }
}