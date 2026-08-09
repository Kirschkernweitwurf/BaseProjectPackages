using System.Collections.Generic;
using Base.UtilityPackage;
using UnityEngine;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Assigns a stable, readable color to each seed name. The hue comes from a stable hash, so the same
    /// name always tints the same way, but the brightness is not free to follow. A node title carries
    /// white text, and at the same brightness value a yellow is roughly ten times as luminous as a blue,
    /// which is why some titles were unreadable. Every color is therefore darkened step by step until it
    /// clears a real contrast ratio against white.
    /// </summary>
    public static class GraphColorPalette
    {
        private const float BlueWeight = 0.0722f;
        private const float ContrastOffset = 0.05f;
        private const float FallbackChannel = 0.24f;
        private const float GammaExponent = 2.4f;
        private const float GammaOffset = 0.055f;
        private const float GammaScale = 1.055f;
        private const float GreenWeight = 0.7152f;
        private const uint HueSteps = 360u;
        private const float LinearDivisor = 12.92f;
        private const float LinearThreshold = 0.03928f;
        private const float MaximumValue = 0.70f;
        private const float MinimumContrast = 5f;
        private const float MinimumValue = 0.18f;
        private const float RedWeight = 0.2126f;
        private const float Saturation = 0.62f;
        private const float ValueStep = 0.02f;
        private const float WhiteLuminance = 1f;

        private static readonly Dictionary<string, Color> Cache = new();

        /// <summary>Returns the title color for a seed name. The same name always yields the same color.</summary>
        /// <param name="seed">Name to derive the color from.</param>
        /// <returns>The tint for the node title bar, dark enough to read white text on.</returns>
        public static Color GetColor(string seed)
        {
            if (string.IsNullOrEmpty(seed))
                return new Color(FallbackChannel, FallbackChannel, FallbackChannel);

            if (Cache.TryGetValue(seed, out Color cached))
                return cached;

            float hue = StringUtility.GetStableHash(seed) % HueSteps / (float)HueSteps;
            Color color = BuildReadableColor(hue);

            Cache[seed] = color;
            return color;
        }

        private static Color BuildReadableColor(float hue)
        {
            for (float value = MaximumValue; value > MinimumValue; value -= ValueStep)
            {
                Color candidate = Color.HSVToRGB(hue, Saturation, value);

                if (GetContrastWithWhite(candidate) >= MinimumContrast)
                    return candidate;
            }

            return Color.HSVToRGB(hue, Saturation, MinimumValue);
        }

        private static float GetContrastWithWhite(Color color)
            => (WhiteLuminance + ContrastOffset) / (GetRelativeLuminance(color) + ContrastOffset);

        private static float GetRelativeLuminance(Color color)
            => RedWeight * Linearize(color.r)
                + GreenWeight * Linearize(color.g)
                + BlueWeight * Linearize(color.b);

        private static float Linearize(float channel)
            => channel <= LinearThreshold
                ? channel / LinearDivisor
                : Mathf.Pow((channel + GammaOffset) / GammaScale, GammaExponent);
    }
}
