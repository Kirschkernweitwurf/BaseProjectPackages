namespace Base.UtilityPackage.Logging
{
    /// <summary>
    /// Utility methods for applying Unity rich text styling to strings.
    /// Supports colors, bold, italic and other inline formatting.
    /// </summary>
    public static class LogTextFormatter
    {
        /// <summary>
        /// A marker to indicate that a log message was generated in editor mode.
        /// Styled in gray and italic for visibility.
        /// </summary>
        public const string EditorMarker = "<color=#9E9E9E><i>editor</i></color> ";

        /// <summary>
        /// Makes text bold in Unity Console logs.
        /// </summary>
        public static string Bold(string text) => $"<b>{text}</b>";

        /// <summary>
        /// Makes text italic in Unity Console logs.
        /// </summary>
        public static string Italic(string text) => $"<i>{text}</i>";

        /// <summary>
        /// Underlines text in Unity Console logs.
        /// </summary>
        public static string Underline(string text) => $"<u>{text}</u>";

        /// <summary>
        /// Colors text using a value from <see cref="EDebugLogColors"/>.
        /// </summary>
        public static string Colorize(string text, EDebugLogColors color)
            => $"<color={ColorToString(color)}>{text}</color>";

        /// <summary>
        /// Changes text size in Unity Console logs.
        /// </summary>
        /// <param name="text">Text to change size of.</param>
        /// <param name="size">Font size (e.g. 10, 14, 20).</param>
        public static string Size(string text, int size) => $"<size={size}>{text}</size>";

        /// <summary>
        /// Converts enum value into a Unity-supported color string.
        /// </summary>
        private static string ColorToString(EDebugLogColors color) => color switch
        {
            EDebugLogColors.Black => "black",
            EDebugLogColors.White => "white",
            EDebugLogColors.Red => "red",
            EDebugLogColors.Green => "green",
            EDebugLogColors.Blue => "blue",
            EDebugLogColors.Yellow => "yellow",
            EDebugLogColors.Cyan => "cyan",
            EDebugLogColors.Magenta => "magenta",
            EDebugLogColors.Gray => "gray",
            EDebugLogColors.Orange => "orange",
            _ => "white"
        };
    }
}