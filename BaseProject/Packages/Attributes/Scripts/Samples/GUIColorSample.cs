using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A tinted field.</summary>
    [AttributeSample(typeof(GUIColorAttribute), EAttributeCategory.Layout,
        Description = "Tints the whole field row, for the one value on a component that has to stand out from the "
            + "rest.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "An EColor for the standard palette, or a hex string for anything else.",
            "The tint applies to one field only, unless the next fields carry it too."
        })]
    internal sealed class GUIColorSample : ScriptableObject
    {
        [GUIColor(EColor.Lime)]
        [Tooltip("Tinted with a color from the shared palette.")]
        public string tinted = "Lime tinted";

        [GUIColor("#FF8080")]
        [Tooltip("Tinted with a hex color, for anything the palette does not cover.")]
        public string custom = "Hex tinted";

        [Tooltip("Untinted, to show the tint does not leak into the fields below it.")]
        public string plain = "Back to normal";
    }
}