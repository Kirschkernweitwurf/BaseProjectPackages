namespace Utility.Logging
{
    /// <summary>
    /// Supported colors for Unity rich text logging.
    /// Used with <see cref="LogTextFormatter.Colorize"/> to style log text.
    /// </summary>
    public enum EDebugLogColors : byte
    {
        Black = 0,
        White = 1,
        Red = 2,
        Green = 3,
        Blue = 4,
        Yellow = 5,
        Cyan = 6,
        Magenta = 7,
        Gray = 8,
        Orange = 9
    }
}