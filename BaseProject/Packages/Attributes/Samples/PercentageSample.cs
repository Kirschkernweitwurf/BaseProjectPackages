using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A normalized value shown as a percentage.</summary>
    [AttributeSample(typeof(PercentageAttribute), EAttributeCategory.Widgets,
        Description = "Shows a value between zero and one as a percentage, so the inspector says 75 while the field "
            + "still stores 0.75 for the code that reads it.",
        Requirements = "The field is expected to hold a normalized value.",
        Variations = new[]
        {
            "Percentage() for a plain field.",
            "Percentage(true) adds a slider."
        })]
    internal sealed class PercentageSample : ScriptableObject
    {
        [Percentage]
        [Tooltip("Stored as 0.75, shown as 75.")]
        public float opacity = 0.75f;

        [Percentage(true)]
        [Tooltip("The same with a slider.")]
        public float volume = 0.5f;
    }
}