using System;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Provides extension methods for formatting time durations.
    /// </summary>
    public static class TimeFormattingExtensions
    {
        private const string HourUnit = "hour";
        private const string MinuteUnit = "minute";
        private const string SecondUnit = "second";

        /// <summary>
        /// Converts a time duration in seconds to a formatted string,
        /// e.g. "2 hours, 5 minutes and 30 seconds", "5 minutes and 30 seconds" or "30 seconds".
        /// </summary>
        /// <param name="seconds">The duration in seconds. Negative values are treated as zero.</param>
        /// <returns>The formatted duration.</returns>
        public static string ToDurationText(this float seconds)
        {
            TimeSpan duration = TimeSpan.FromSeconds(Math.Round(Math.Max(seconds, 0f)));
            int hours = (int)duration.TotalHours;

            if (hours >= 1)
                return $"{Format(hours, HourUnit)}, {Format(duration.Minutes, MinuteUnit)} "
                    + $"and {Format(duration.Seconds, SecondUnit)}";

            if (duration.Minutes >= 1)
                return $"{Format(duration.Minutes, MinuteUnit)} and {Format(duration.Seconds, SecondUnit)}";

            return Format(duration.Seconds, SecondUnit);
        }

        // Keeps the singular form for a value of one, so "1 seconds" cannot happen.
        private static string Format(int value, string unit) => value == 1
            ? $"{value} {unit}"
            : $"{value} {unit}s";
    }
}