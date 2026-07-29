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
        private const uint HueSteps = 360u;

        /// <summary>
        /// Generates a consistent color for a given name.
        /// </summary>
        /// <param name="name">The name to generate a color for (e.g. a class name or log category).</param>
        /// <returns>A color that stays the same for the same name across sessions.</returns>
        public static Color GetColorValue(string name)
        {
            float hue = StringUtility.GetStableHash(name) % HueSteps / (float)HueSteps;
            return Color.HSVToRGB(hue, ColorSaturation, ColorValue);
        }

        /// <summary>
        /// Generates a consistent color string for a given name.
        /// </summary>
        /// <param name="name">The name to generate a color for (e.g. a class name or log category).</param>
        /// <returns>A hex color string (e.g. "#FFAA00") that can be used in Unity rich text.</returns>
        public static string GetColor(string name) => $"#{ColorUtility.ToHtmlStringRGB(GetColorValue(name))}";

        /// <summary>
        /// Builds the styled "[ClassName]" tag that every log message is prefixed with.
        /// </summary>
        /// <param name="className">Name of the class the message originates from.</param>
        /// <returns>The colored and bolded class tag.</returns>
        public static string BuildClassTag(string className)
            => LogTextFormatter.Colorize(LogTextFormatter.Bold($"[{className}]"), GetColor(className));

        /// <summary>
        /// Returns the edit mode marker, or an empty string outside edit mode.
        /// </summary>
        /// <returns>The marker to put in front of a log message.</returns>
        public static string GetEditorMarker() => Platform.IsEditorMode()
            ? LogTextFormatter.EditorMarker
            : string.Empty;
    }
}