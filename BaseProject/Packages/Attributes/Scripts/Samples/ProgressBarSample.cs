using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A number drawn as a filled bar.</summary>
    [AttributeSample(typeof(ProgressBarAttribute), EAttributeCategory.Widgets,
        Description = "Draws the value as a filled bar instead of a number, for anything read at a glance rather than "
            + "typed exactly.",
        Requirements = "Nothing, unless the maximum reads a member, in which case that member has to exist on the same "
            + "object.",
        Variations = new[]
        {
            "ProgressBar(max) for a constant maximum.",
            "ProgressBar(nameof(member)) reads the maximum from another member.",
            "A color argument tints the bar, and readOnly turns it into a gauge that cannot be dragged."
        })]
    internal sealed class ProgressBarSample : ScriptableObject
    {
        [ProgressBar(100f, EColor.Green)]
        [Tooltip("Drag the bar to set the value.")]
        public float charge = 62f;

        [ProgressBar(100f, EColor.Red, true)]
        [Tooltip("Read-only, so it reports rather than accepts.")]
        public float wear = 18f;
    }
}