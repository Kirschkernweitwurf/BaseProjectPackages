using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A number clamped to a range, without a slider.</summary>
    [AttributeSample(typeof(MinMaxAttribute), EAttributeCategory.Validation,
        Description = "Clamps the value into a range without drawing a slider, for a number that is typed rather than "
            + "dragged.",
        Requirements = "Type a number outside the range and watch it snap back.",
        Variations = new[]
        {
            "Works on int, float and the vector types, where every component is clamped.",
            "Use Slider or MinMaxSlider instead when the value is better picked by dragging."
        })]
    internal sealed class MinMaxSample : ScriptableObject
    {
        [MinMax(0, 100)]
        [Tooltip("Type 250 and it settles on 100.")]
        public int health = 50;

        [MinMax(0f, 1f)]
        [Tooltip("The same on a vector, where each component is clamped on its own.")]
        public Vector2 normalized = new(0.5f, 0.5f);
    }
}