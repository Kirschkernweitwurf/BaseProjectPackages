using UnityEngine;

namespace Base.UtilityPackage.Logging
{
    /// <summary>
    /// Shared helpers for the logging classes: the stable per-class color, the styled class tag
    /// and the edit mode marker.
    /// </summary>
    public static class CustomLoggingUtils
    {
        private const float ColorSaturation = 0.5f;
        private const float ColorValue = 0.9f;
        private const int HueSteps = 360;

        /// <summary>
        /// Generates a consistent color string for a given name, based on its hash code.
        /// </summary>
        /// <param name="name">The name to generate a color for (e.g. a class name or log category).</param>
        /// <returns>A hex color string (e.g. "#FFAA00") that can be used in Unity rich text.</returns>
        public static string GetColor(string name)
        {
            float hue = (name.GetHashCode() & int.MaxValue) % HueSteps / (float)HueSteps;
            Color color = Color.HSVToRGB(hue, ColorSaturation, ColorValue);
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        /// <summary>
        /// Builds the styled "[ClassName]" tag that every log message is prefixed with.
        /// </summary>
        /// <param name="className">Name of the class the message originates from.</param>
        /// <returns>The colored and bolded class tag.</returns>
        public static string BuildClassTag(string className)
            => $"<color={GetColor(className)}>{LogTextFormatter.Bold($"[{className}]")}</color>";

        /// <summary>
        /// Returns the edit mode marker, or an empty string outside edit mode.
        /// </summary>
        /// <returns>The marker to put in front of a log message.</returns>
        public static string GetEditorMarker() => Platform.IsEditorMode()
            ? LogTextFormatter.EditorMarker
            : string.Empty;
    }
}