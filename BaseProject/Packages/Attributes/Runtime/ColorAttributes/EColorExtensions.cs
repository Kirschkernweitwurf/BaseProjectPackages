using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Maps <see cref="EColor"/> values to concrete <see cref="Color"/> values.
    /// </summary>
    /// <remarks>
    /// The set is tuned as one family rather than picked hue by hue. Every entry sits at a similar
    /// perceived lightness, bright enough to read against the dark editor skin and dark enough to stay
    /// legible on the light one, and saturated enough to tell apart without any of them shouting over
    /// its neighbors. That last part matters most: these colors end up next to each other in a single
    /// inspector, so a palette of individually pretty colors that fight each other is worse than a
    /// slightly duller one that does not.
    /// </remarks>
    public static class EColorExtensions
    {
        private static readonly Color32 Black = new(0, 0, 0, 255);
        private static readonly Color32 Blue = new(96, 165, 250, 255);
        private static readonly Color32 Brown = new(176, 137, 104, 255);
        private static readonly Color32 Cyan = new(34, 211, 238, 255);
        private static readonly Color32 Gray = new(148, 163, 184, 255);
        private static readonly Color32 Green = new(52, 211, 153, 255);
        private static readonly Color32 Lime = new(163, 230, 53, 255);
        private static readonly Color32 Magenta = new(232, 121, 249, 255);
        private static readonly Color32 Orange = new(251, 146, 60, 255);
        private static readonly Color32 Pink = new(244, 114, 182, 255);
        private static readonly Color32 Purple = new(167, 139, 250, 255);
        private static readonly Color32 Red = new(248, 113, 113, 255);
        private static readonly Color32 Teal = new(45, 212, 191, 255);
        private static readonly Color32 White = new(241, 245, 249, 255);
        private static readonly Color32 Yellow = new(251, 191, 36, 255);

        /// <summary>Returns the concrete color for the given preset.</summary>
        /// <param name="color">The preset to resolve.</param>
        /// <returns>The color to draw with.</returns>
        public static Color ToColor(this EColor color)
        {
            return color switch
            {
                EColor.White => White,
                EColor.Black => Black,
                EColor.Gray => Gray,
                EColor.Red => Red,
                EColor.Orange => Orange,
                EColor.Yellow => Yellow,
                EColor.Green => Green,
                EColor.Teal => Teal,
                EColor.Cyan => Cyan,
                EColor.Blue => Blue,
                EColor.Purple => Purple,
                EColor.Pink => Pink,
                EColor.Magenta => Magenta,
                EColor.Brown => Brown,
                EColor.Lime => Lime,
                _ => White
            };
        }
    }
}