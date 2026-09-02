using System.Collections.Generic;
using Base.UtilityPackage;
using UnityEngine;

namespace Base.ToolsPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Assigns a stable, readable color to each seed name. The palette is pastel, which changes what the
    /// contrast has to be measured against: a light title bar cannot carry white text, so the title is
    /// written in near black and every color is brightened until it clears a real ratio against that.
    /// <br/><br/>
    /// Pastels also give the whole hue wheel back. A saturated red reads as a warning and had to be kept
    /// out, but nothing about a pale coral says anything is wrong, so reds and oranges are back in the
    /// rotation and the variety roughly doubles.
    /// </summary>
    internal static class GraphColorPalette
    {
        private const float BlueWeight = 0.0722f;
        private const float ContrastOffset = 0.05f;
        private const float FallbackChannel = 0.88f;
        private const float GammaExponent = 2.4f;
        private const float GammaOffset = 0.055f;
        private const float GammaScale = 1.055f;
        private const float GreenWeight = 0.7152f;
        private const uint HueSteps = 360u;
        private const float LinearDivisor = 12.92f;
        private const float LinearThreshold = 0.03928f;
        private const float MaximumValue = 1f;
        private const float MinimumContrast = 5f;
        private const float RedWeight = 0.2126f;
        private const float StartingValue = 0.86f;
        private const uint TierSpread = 7u;
        private const float ValueStep = 0.02f;

        /// <summary>Color the node title is written in, dark because every background here is light.</summary>
        internal static Color TitleTextColor { get; } = new(0.10f, 0.10f, 0.11f);

        /// <summary>Saturations, all gentle, so two nearby hues still read as different colors.</summary>
        private static readonly float[] Saturations =
        {
            0.16f,
            0.24f,
            0.32f,
            0.40f
        };

        private static readonly Dictionary<string, Color> Cache = new();

        /// <summary>Returns the title color for a seed name. The same name always yields the same color.</summary>
        /// <param name="seed">Name to derive the color from.</param>
        /// <returns>The tint for the node title bar, light enough to read dark text on.</returns>
        internal static Color GetColor(string seed)
        {
            if (string.IsNullOrEmpty(seed))
                return new Color(FallbackChannel, FallbackChannel, FallbackChannel);

            if (Cache.TryGetValue(seed, out Color cached))
                return cached;

            uint hash = StringUtility.GetStableHash(seed);
            float hue = hash % HueSteps / (float)HueSteps;

            // A second, coarser slice of the same hash picks the saturation, so hue and depth vary apart.
            float saturation = Saturations[hash / TierSpread % (uint)Saturations.Length];
            Color color = BuildReadableColor(hue, saturation);

            Cache[seed] = color;
            return color;
        }

        private static Color BuildReadableColor(float hue, float saturation)
        {
            for (float value = StartingValue; value < MaximumValue; value += ValueStep)
            {
                Color candidate = Color.HSVToRGB(hue, saturation, value);

                if (GetContrast(candidate) >= MinimumContrast)
                    return candidate;
            }

            return Color.HSVToRGB(hue, saturation, MaximumValue);
        }

        private static float GetContrast(Color background) => (GetRelativeLuminance(background) + ContrastOffset)
            / (GetRelativeLuminance(TitleTextColor) + ContrastOffset);

        private static float GetRelativeLuminance(Color color) => RedWeight * Linearize(color.r)
            + GreenWeight * Linearize(color.g)
            + BlueWeight * Linearize(color.b);

        private static float Linearize(float channel) => channel <= LinearThreshold
            ? channel / LinearDivisor
            : Mathf.Pow((channel + GammaOffset) / GammaScale, GammaExponent);
    }
}