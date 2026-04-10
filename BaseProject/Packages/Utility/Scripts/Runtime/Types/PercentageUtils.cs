namespace Utility.Types
{
    /// <summary>
    /// Utility methods for converting between normalized values and percentages.
    /// </summary>
    public static class PercentageUtils
    {
        /// <summary>
        /// Converts a value (0–1) to a percentage (0–100).
        /// Example: 0.56 → 56
        /// </summary>
        public static float ToPercent(float value) => value * 100f;

        /// <summary>
        /// Converts a percentage (0–100) to a normalized value (0–1).
        /// Example: 56 → 0.56
        /// </summary>
        public static float FromPercent(float percent) => percent / 100f;

        /// <summary>
        /// Returns a formatted percentage string with a % symbol.
        /// Example: 0.56 → "56%"
        /// </summary>
        public static string ToPercentString(float value, int decimals = 0)
        {
            return ToPercent(value).ToString($"F{decimals}") + "%";
        }
    }
}