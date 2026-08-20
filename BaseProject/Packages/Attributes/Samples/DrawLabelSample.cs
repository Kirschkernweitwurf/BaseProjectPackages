using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A label drawn in the Scene view.</summary>
    [AttributeSample(typeof(DrawLabelAttribute), EAttributeCategory.Widgets,
        Description = "Draws a text label in the Scene view at the position the field holds, for naming what a point "
            + "in space is while looking at the level.",
        Requirements = "Drawn in the Scene view for the selected object. Use the button below to put this sample into "
            + "your scene, then select it.",
        Variations = new[]
        {
            "DrawLabel(text) writes a fixed label.",
            "TextMember reads the label from a member instead.",
            "PresetColor tints it and Space picks between local and world."
        })]
    internal sealed class DrawLabelSample : MonoBehaviour
    {
        [DrawLabel("Spawn")]
        [Tooltip("Labelled in the Scene view at the position it holds.")]
        public Vector3 spawnPoint = Vector3.right;
    }
}