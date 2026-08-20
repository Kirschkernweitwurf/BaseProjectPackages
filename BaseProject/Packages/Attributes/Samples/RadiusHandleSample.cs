using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A radius edited with a ring handle.</summary>
    [AttributeSample(typeof(RadiusHandleAttribute), EAttributeCategory.Widgets,
        Description = "Draws a ring in the Scene view whose radius is the field, for a range or a trigger size that is "
            + "easier to judge against the level than as a number.",
        Requirements = "Drawn in the Scene view for the selected object. Use the button below to put this sample into "
            + "your scene, then select it.",
        Variations = new[]
        {
            "A color argument tints the ring.",
            "Axis picks which plane it lies in, and PositionMember names the field it is centered on."
        })]
    internal sealed class RadiusHandleSample : MonoBehaviour
    {
        [RadiusHandle(EColor.Cyan)]
        [Tooltip("Dragged in the Scene view rather than typed here.")]
        public float attackRange = 3f;
    }
}