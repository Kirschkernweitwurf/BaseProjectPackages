using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A separator line above a field.</summary>
    [AttributeSample(typeof(HorizontalLineAttribute), EAttributeCategory.Layout,
        Description = "Draws a line above the field. Use it to split two groups of fields when neither of them is "
            + "worth a heading of its own.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "HorizontalLine() for the default line.",
            "An EColor or a hex string sets the color.",
            "Thickness and Padding set how heavy the line is and how much room it gets."
        })]
    internal sealed class HorizontalLineSample : ScriptableObject
    {
        [Tooltip("Above the line.")]
        public int before = 1;

        [HorizontalLine(EColor.Red)]
        [Tooltip("Below the line. The line belongs to this field, not to the one above it.")]
        public int after = 2;
    }
}