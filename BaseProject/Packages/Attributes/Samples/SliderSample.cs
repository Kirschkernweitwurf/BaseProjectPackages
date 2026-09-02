using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A number dragged rather than typed.</summary>
    [AttributeSample(typeof(SliderAttribute), EAttributeCategory.Widgets,
        Description = "A slider whose bounds can be read from other members. Unity's own Range takes only "
            + "constants, so a limit that depends on the setup has to be duplicated as a magic number or "
            + "given up on. This takes a member name for either end, and can clamp the stored value to it.",
        Requirements = "A bound reading a member needs that member on the same object, returning a number. "
            + "With two constant bounds there is no reason to reach for this over Range.",
        Variations = new[]
        {
            "Either bound can be a member name, so one end can be fixed and the other computed.",
            "A member can be a field, a property or a parameterless method.",
            "AutoClamp keeps the stored value inside the bounds even when it was set from code."
        })]
    internal sealed class SliderSample : ScriptableObject
    {
        [Tooltip("The upper bound the slider below reads.")]
        public float maxSpeed = 20f;

        [Slider(0f, nameof(maxSpeed), AutoClamp = true)]
        [Tooltip("The upper bound comes from the field above. Lower it and watch this value follow.")]
        public float speed = 8f;
    }
}