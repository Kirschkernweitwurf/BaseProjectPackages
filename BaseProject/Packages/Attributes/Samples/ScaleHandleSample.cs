using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A scale edited with a scale handle.</summary>
    [AttributeSample(typeof(ScaleHandleAttribute), EAttributeCategory.Widgets,
        Description = "Draws a scale handle in the Scene view for a Vector3 field.",
        Requirements = "Drawn in the Scene view for the selected object. Use the button below to put this sample into "
            + "your scene, then select it.",
        Variations = new[]
        {
            "Size sets how large the handle is drawn.",
            "PositionMember names the field the handle sits on."
        })]
    internal sealed class ScaleHandleSample : MonoBehaviour
    {
        [ScaleHandle]
        [Tooltip("Dragged in the Scene view rather than typed here.")]
        public Vector3 boxSize = Vector3.one;
    }
}