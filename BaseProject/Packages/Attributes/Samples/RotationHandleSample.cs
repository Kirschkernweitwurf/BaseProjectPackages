using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A rotation edited with a rotate handle.</summary>
    [AttributeSample(typeof(RotationHandleAttribute), EAttributeCategory.Widgets,
        Description = "Draws a rotation handle in the Scene view for a Quaternion or Vector3 field.",
        Requirements = "Drawn in the Scene view for the selected object. Use the button below to put this sample into "
            + "your scene, then select it.",
        Variations = new[]
        {
            "The field can be a Quaternion or a Vector3 of euler angles.",
            "RotationHandle(ESpace.World) treats the value as a world rotation.",
            "PositionMember names the field the handle sits on."
        })]
    internal sealed class RotationHandleSample : MonoBehaviour
    {
        [RotationHandle(PositionMember = nameof(anchor))]
        [Tooltip("Euler angles rather than a raw quaternion, so the four numbers do not read as a bug.")]
        public Vector3 facing;

        [Tooltip("Where the handle above is drawn.")]
        public Vector3 anchor = Vector3.zero;
    }
}