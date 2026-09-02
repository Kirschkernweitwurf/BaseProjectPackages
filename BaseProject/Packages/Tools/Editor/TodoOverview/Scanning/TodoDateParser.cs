using System;
using System.Globalization;
using Base.ToolsPackage.Editor.TodoOverview.Model;

namespace Base.ToolsPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// Turns the date written into a comment into a real date, and says whether it has passed.
    /// The configured formats are tried first and in order, because 08.09.26 means different days in
    /// different notations and only the project can say which one it writes.
    /// </summary>
    internal static class TodoDateParser
    {
        /// <summary>Tries to read a date the way the project writes them.</summary>
        /// <param name="raw">The date exactly as it was written.</param>
        /// <param name="formats">The formats to try, in order.</param>
        /// <param name="date">The parsed date, or the default value when none of the formats fit.</param>
        /// <returns><c>true</c> when the date could be read.</returns>
        internal static bool TryParse(string raw, string[] formats, out DateTime date)
        {
            date = default(DateTime);

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            string trimmed = raw.Trim();

            if (formats.Length > 0
                && DateTime.TryParseExact(trimmed, formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out date))
                return true;

            return DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        /// <summary>Where a date sits relative to today.</summary>
        /// <param name="date">The date to judge, or null when the item carries none.</param>
        /// <returns>The state its pill is colored by.</returns>
        internal static ETodoDateState Resolve(DateTime? date)
        {
            if (!date.HasValue)
                return ETodoDateState.None;

            int difference = date.Value.Date.CompareTo(DateTime.Today);

            if (difference < 0)
                return ETodoDateState.Overdue;

            return difference == 0
                ? ETodoDateState.Today
                : ETodoDateState.Future;
        }
    }
}