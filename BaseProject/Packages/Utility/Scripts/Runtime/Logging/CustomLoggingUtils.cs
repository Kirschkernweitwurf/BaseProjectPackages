using UnityEngine;

namespace Base.UtilityPackage.Logging
{
    /// <summary>
    /// Utility class for logging-related helper methods, such as generating consistent colors for log categories.
    /// </summary>
    public static class CustomLoggingUtils
    {
        /// <summary>
        /// Generates a consistent color string for a given name, based on its hash code.
        /// </summary>
        /// <param name="name">The name to generate a color for (e.g. a class name or log category).</param>
        /// <returns>A hex color string (e.g. "#FFAA00") that can be used in Unity rich text.</returns>
        public static string GetColor(string name)
        {
            float hue = (name.GetHashCode() & int.MaxValue) % 360 / 360f;
            Color color = Color.HSVToRGB(hue, 0.5f, 0.9f);
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
    }
}