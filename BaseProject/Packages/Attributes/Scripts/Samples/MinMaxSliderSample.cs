using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A range picked with one slider and two handles.</summary>
    [AttributeSample(typeof(MinMaxSliderAttribute), EAttributeCategory.Widgets,
        Description = "Turns a Vector2 into a single slider with two handles, for a range whose two ends belong "
            + "together. X is the low end and Y the high one.",
        Requirements = "The field has to be a Vector2.",
        Variations = new[]
        {
            "MinMaxSlider(min, max) for two constants.",
            "Either bound can be a member name instead.",
            "MinMaxSlider(rangeMember) reads both bounds from one Vector2 member."
        })]
    internal sealed class MinMaxSliderSample : ScriptableObject
    {
        [MinMaxSlider(0f, 100f)]
        [Tooltip("Drag either handle. X is the low end, Y the high one.")]
        public Vector2 spawnRange = new(20f, 80f);
    }
}