using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A color restricted to a set of swatches.</summary>
    [AttributeSample(typeof(ColorPaletteAttribute), EAttributeCategory.Widgets,
        Description = "Restricts a color to a set of swatches provided by a member, so a tint cannot quietly drift off "
            + "the palette the rest of the project uses.",
        Requirements = "The member it names has to be an instance member on the same object returning colors. A static "
            + "one is not found.",
        Variations = new[]
        {
            "AllowCustom lets the field fall back to the normal color picker as well."
        })]
    internal sealed class ColorPaletteSample : ScriptableObject
    {
        [ColorPalette(nameof(Brand))]
        [Tooltip("Only the three swatches below are on offer.")]
        public Color brandColor = Color.white;

        // The palette reads from here. Instance rather than static, because the member resolver only looks at
        // instance members and a static source would silently find nothing.
        private Color[] Brand => new[]
        {
            new Color(0.20f, 0.60f, 0.86f),
            new Color(0.18f, 0.80f, 0.44f),
            new Color(0.95f, 0.61f, 0.07f)
        };
    }
}