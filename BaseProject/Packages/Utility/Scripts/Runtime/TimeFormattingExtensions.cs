using System;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Provides extension methods for formatting time durations.
    /// </summary>
    public static class TimeFormattingExtensions
    {
        /// <summary>
        /// Converts a time duration in seconds to a formatted string,
        /// e.g. "2 hours, 5 minutes and 30 seconds", "5 minutes and 30 seconds" or "30 seconds".
        /// </summary>
        public static string ToMinutesSecondsText(this float seconds)
        {
            if (seconds < 0f)
                seconds = 0f;

            int totalSeconds = (int)Math.Round(seconds);
            TimeSpan ts = TimeSpan.FromSeconds(totalSeconds);

            string text;
            if (ts.TotalHours >= 1)
                text = $"{(int)ts.TotalHours} hours, {ts.Minutes} minutes and {ts.Seconds} seconds";
            else if (ts.TotalMinutes >= 1)
                text = $"{ts.Minutes} minutes and {ts.Seconds} seconds";
            else
                text = $"{ts.Seconds} seconds";

            return text;
        }
    }
}