using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Resolves an explicit color from an HTML hex string or an <see cref="EColor"/> preset.
    /// Hex takes priority over the preset.
    /// </summary>
    internal static class ColorAttributeUtility
    {
        /// <summary>
        /// Tries to resolve an explicit color. Returns false when neither a hex nor a preset is set,
        /// leaving the drawer to apply its own default.
        /// </summary>
        public static bool TryResolve(string colorHex, EColor preset, out Color color)
        {
            if (!string.IsNullOrEmpty(colorHex))
            {
                string normalized = colorHex.StartsWith("#")
                    ? colorHex
                    : "#" + colorHex;

                if (ColorUtility.TryParseHtmlString(normalized, out color))
                    return true;
            }

            if (preset != EColor.Default)
            {
                color = preset.ToColor();
                return true;
            }

            color = default(Color);
            return false;
        }
    }
}