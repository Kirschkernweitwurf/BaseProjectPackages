using UnityEngine;

namespace Base.EditorUIPackage.Editor.Tests
{
    /// <summary>
    /// Measures the contrast between two colors the way the accessibility standard defines it, so the
    /// legibility the presets promise can be checked rather than eyeballed.
    /// </summary>
    /// <remarks>
    /// This is the WCAG 2 ratio: each channel is taken back out of the sRGB curve, weighted into a
    /// relative luminance, and the lighter of the two is compared against the darker with a small
    /// offset that keeps black against black from dividing by nothing. The perceptual model behind the
    /// draft of the next standard is deliberately not reimplemented here, since a second-hand copy of
    /// it would measure the tests rather than the palette.
    /// </remarks>
    internal static class ContrastRatio
    {
        private const float ChannelBreak = 0.03928f;
        private const float ChannelOffset = 0.055f;
        private const float ChannelScale = 1.055f;
        private const float ChannelSlope = 12.92f;
        private const float Exponent = 2.4f;
        private const float LuminanceOffset = 0.05f;

        private const float BlueWeight = 0.0722f;
        private const float GreenWeight = 0.7152f;
        private const float RedWeight = 0.2126f;

        /// <summary>The contrast between two colors, from 1 for identical to 21 for black on white.</summary>
        /// <param name="first">One of the two colors. Order does not matter.</param>
        /// <param name="second">The other color.</param>
        /// <returns>The ratio.</returns>
        internal static float Between(Color first, Color second)
        {
            float firstLuminance = Luminance(first);
            float secondLuminance = Luminance(second);

            float lighter = Mathf.Max(firstLuminance, secondLuminance);
            float darker = Mathf.Min(firstLuminance, secondLuminance);

            return (lighter + LuminanceOffset) / (darker + LuminanceOffset);
        }

        private static float Luminance(Color color) => RedWeight * Linear(color.r)
            + GreenWeight * Linear(color.g)
            + BlueWeight * Linear(color.b);

        // Undoes the sRGB transfer curve, so the weights below are applied to light rather than to
        // the encoded values a display is handed.
        private static float Linear(float channel) => channel <= ChannelBreak
            ? channel / ChannelSlope
            : Mathf.Pow((channel + ChannelOffset) / ChannelScale, Exponent);
    }
}