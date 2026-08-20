using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A position edited with a move handle.</summary>
    [AttributeSample(typeof(PositionHandleAttribute), EAttributeCategory.Widgets,
        Description = "Draws a move handle in the Scene view for a Vector3 field, so a position stored on a component "
            + "is dragged rather than typed.",
        Requirements = "Drawn in the Scene view for the selected object, which an embedded inspector cannot show. Use "
            + "the button below to put this sample into your scene, then select it.",
        Variations = new[]
        {
            "PositionHandle(ESpace.World) treats the value as a world position instead of a local one.",
            "Label names the handle in the Scene view."
        })]
    internal sealed class PositionHandleSample : MonoBehaviour
    {
        [PositionHandle]
        [Tooltip("Dragged in the Scene view rather than typed here.")]
        public Vector3 spawnPoint = Vector3.forward;
    }
}