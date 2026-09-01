using System.Collections.Generic;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// A named set of colors for the things a window colors by category rather than by meaning: a
    /// keyword in a todo list, the kind of a command in the palette, a label on a group. One row of
    /// the spectrum plus a gray, given twice, once for a dark editor theme and once for a light one.
    /// </summary>
    /// <remarks>
    /// The palette names colors by what they are for, and there is no name for "the third of five
    /// categories". These are for exactly that: a set that reads as one family, so two windows using
    /// them look like they were built by the same hand.
    /// <para>
    /// The two sets differ in lightness rather than in hue. A chip sits on the window behind it, so
    /// on a dark theme the colors have to carry enough light to separate from it, and on a light one
    /// enough depth not to wash into it. Each is tuned to stay readable once
    /// <see cref="EditorPalette.TextOn"/> has chosen which of the theme's two text colors to put on
    /// it, which is what lets a yellow and a navy both work as a chip.
    /// </para>
    /// </remarks>
    public static class EditorSwatches
    {
        private const string AmberName = "Amber";
        private const string BlueName = "Blue";
        private const string CyanName = "Cyan";
        private const string GrayName = "Gray";
        private const string GreenName = "Green";
        private const string IndigoName = "Indigo";
        private const string LimeName = "Lime";
        private const string OrangeName = "Orange";
        private const string PinkName = "Pink";
        private const string RedName = "Red";
        private const string TealName = "Teal";
        private const string VioletName = "Violet";
        private const string YellowName = "Yellow";

        /// <summary>Red, for something broken or destructive.</summary>
        public static Color Red => Named(RedName);

        /// <summary>Orange, for something to be corrected.</summary>
        public static Color Orange => Named(OrangeName);

        /// <summary>Amber, for a shortcut taken on purpose.</summary>
        public static Color Amber => Named(AmberName);

        /// <summary>Green, for something created or passing.</summary>
        public static Color Green => Named(GreenName);

        /// <summary>Teal, for something waiting on another person.</summary>
        public static Color Teal => Named(TealName);

        /// <summary>Blue, for the ordinary case of whatever is being categorized.</summary>
        public static Color Blue => Named(BlueName);

        /// <summary>Violet, for configuration and for anything that changes how a tool behaves.</summary>
        public static Color Violet => Named(VioletName);

        /// <summary>Every swatch, in spectrum order, resolved for the current editor theme.</summary>
        /// <returns>The name and color of each swatch.</returns>
        public static IReadOnlyList<KeyValuePair<string, Color>> All() => new[]
        {
            Swatch(RedName, new Color(0.898f, 0.325f, 0.294f), new Color(0.765f, 0.231f, 0.196f)),
            Swatch(OrangeName, new Color(0.925f, 0.557f, 0.173f), new Color(0.722f, 0.384f, 0.106f)),
            Swatch(AmberName, new Color(0.851f, 0.643f, 0.255f), new Color(0.620f, 0.455f, 0.078f)),
            Swatch(YellowName, new Color(0.831f, 0.761f, 0.290f), new Color(0.541f, 0.478f, 0.071f)),
            Swatch(LimeName, new Color(0.635f, 0.765f, 0.290f), new Color(0.373f, 0.494f, 0.110f)),
            Swatch(GreenName, new Color(0.341f, 0.671f, 0.353f), new Color(0.184f, 0.490f, 0.204f)),
            Swatch(TealName, new Color(0.247f, 0.690f, 0.651f), new Color(0.110f, 0.490f, 0.459f)),
            Swatch(CyanName, new Color(0.298f, 0.706f, 0.839f), new Color(0.106f, 0.494f, 0.608f)),
            Swatch(BlueName, new Color(0.325f, 0.608f, 0.961f), new Color(0.106f, 0.384f, 0.769f)),
            Swatch(IndigoName, new Color(0.478f, 0.498f, 0.878f), new Color(0.290f, 0.310f, 0.749f)),
            Swatch(VioletName, new Color(0.663f, 0.439f, 0.878f), new Color(0.482f, 0.247f, 0.749f)),
            Swatch(PinkName, new Color(0.878f, 0.420f, 0.659f), new Color(0.702f, 0.227f, 0.494f)),
            Swatch(GrayName, new Color(0.545f, 0.580f, 0.620f), new Color(0.357f, 0.392f, 0.427f))
        };

        private static KeyValuePair<string, Color> Swatch(string name, Color dark, Color light)
            => new(name, EditorPalette.Pick(dark, light));

        // Looked up by name rather than by position, so the spectrum above can be reordered or added
        // to without silently repainting everything that already uses one of the named colors.
        private static Color Named(string name)
        {
            foreach (KeyValuePair<string, Color> swatch in All())
            {
                if (swatch.Key == name)
                    return swatch.Value;
            }

            return Color.gray;
        }
    }
}