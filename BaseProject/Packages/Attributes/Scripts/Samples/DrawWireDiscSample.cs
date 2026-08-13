using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A radius drawn as a ring, without a handle.</summary>
    [AttributeSample(typeof(DrawWireDiscAttribute), EAttributeCategory.Widgets,
        Description = "Draws a ring in the Scene view for a radius, showing it without offering to change it. The "
            + "read-only half of the radius handle, for a value something else owns.",
        Requirements = "Drawn in the Scene view for the selected object. Use the button below to put this sample into "
            + "your scene, then select it.",
        Variations = new[]
        {
            "A color argument tints the ring.",
            "Axis picks which plane it lies in, and PositionMember names the field it is centered on."
        })]
    internal sealed class DrawWireDiscSample : MonoBehaviour
    {
        [DrawWireDisc(EColor.Orange)]
        [Tooltip("Shown in the Scene view, but only edited here.")]
        public float hearingRange = 6f;
    }
}