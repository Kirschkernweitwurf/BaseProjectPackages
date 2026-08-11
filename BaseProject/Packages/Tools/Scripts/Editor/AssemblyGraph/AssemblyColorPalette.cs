using System.Collections.Generic;
using Base.UtilityPackage;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>Assigns a stable, readable color to each assembly root name.</summary>
    internal static class AssemblyColorPalette
    {
        private const float FallbackChannel = 0.28f;
        private const uint HueSteps = 360u;
        private const float TitleSaturation = 0.52f;
        private const float TitleValue = 0.58f;

        private static readonly Dictionary<string, Color> Cache = new();

        /// <summary>Returns the title color for the given root name. Same name always yields the same color.</summary>
        public static Color GetColor(string rootName)
        {
            if (string.IsNullOrEmpty(rootName))
                return new Color(FallbackChannel, FallbackChannel, FallbackChannel);

            if (Cache.TryGetValue(rootName, out Color cached))
                return cached;

            // Darker than the console color for the same name, since this tints a node title bar.
            float hue = StringUtility.GetStableHash(rootName) % HueSteps / (float)HueSteps;
            Color color = Color.HSVToRGB(hue, TitleSaturation, TitleValue);

            Cache[rootName] = color;
            return color;
        }
    }
}