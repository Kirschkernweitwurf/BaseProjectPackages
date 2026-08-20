namespace Base.UtilityPackage
{
    /// <summary>
    /// Utility methods for converting between normalized values and percentages.
    /// </summary>
    public static class PercentageUtils
    {
        private const float PercentFactor = 100f;
        private const string PercentSymbol = "%";

        /// <summary>
        /// Converts a normalized value (0 to 1) to a percentage (0 to 100). Example: 0.56 becomes 56.
        /// </summary>
        /// <param name="value">The normalized value.</param>
        /// <returns>The value as a percentage.</returns>
        public static float ToPercent(float value) => value * PercentFactor;

        /// <summary>
        /// Converts a percentage (0 to 100) to a normalized value (0 to 1). Example: 56 becomes 0.56.
        /// </summary>
        /// <param name="percent">The percentage.</param>
        /// <returns>The normalized value.</returns>
        public static float FromPercent(float percent) => percent / PercentFactor;

        /// <summary>
        /// Returns a formatted percentage string with a percent symbol. Example: 0.56 becomes "56%".
        /// </summary>
        /// <param name="value">The normalized value.</param>
        /// <param name="decimals">Number of decimal places to show.</param>
        /// <returns>The formatted percentage.</returns>
        public static string ToPercentString(float value, int decimals = 0)
            => ToPercent(value).ToString($"F{decimals}") + PercentSymbol;
    }
}