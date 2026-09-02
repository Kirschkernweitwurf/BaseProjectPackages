using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A line drawn to a position.</summary>
    [AttributeSample(typeof(DrawLineAttribute), EAttributeCategory.Widgets,
        Description = "Draws a line in the Scene view from the object to the position the field holds, so a target or "
            + "an offset is visible rather than inferred from three numbers.",
        Requirements = "Drawn in the Scene view for the selected object. Use the button below to put this sample into "
            + "your scene, then select it.",
        Variations = new[]
        {
            "A color argument tints the line, and Dotted draws it dotted.",
            "FromMember names the field the line starts at, instead of the object itself."
        })]
    internal sealed class DrawLineSample : MonoBehaviour
    {
        [DrawLine(EColor.Lime, Dotted = true)]
        [Tooltip("A line runs from the object to this position in the Scene view.")]
        public Vector3 patrolTarget = new(0f, 0f, 5f);
    }
}