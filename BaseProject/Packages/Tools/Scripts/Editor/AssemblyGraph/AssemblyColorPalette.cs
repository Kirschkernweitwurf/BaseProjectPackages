using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>Assigns a stable, readable color to each assembly root name.</summary>
    public static class AssemblyColorPalette
    {
        private static readonly Dictionary<string, Color> Cache = new();

        /// <summary>Returns the title color for the given root name. Same name always yields the same color.</summary>
        public static Color GetColor(string rootName)
        {
            if (string.IsNullOrEmpty(rootName))
                return new Color(0.28f, 0.28f, 0.28f);

            if (Cache.TryGetValue(rootName, out Color cached))
                return cached;

            float hue = Hash(rootName) % 360u / 360f;
            Color color = Color.HSVToRGB(hue, 0.52f, 0.58f);

            Cache[rootName] = color;
            return color;
        }

        /// <summary>FNV-1a hash. Stable across sessions, unlike string.GetHashCode.</summary>
        private static uint Hash(string value)
        {
            uint hash = 2166136261u;

            foreach (char character in value)
            {
                hash ^= character;
                hash *= 16777619u;
            }

            return hash;
        }
    }
}