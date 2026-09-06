using System;
using System.Globalization;

namespace Base.ToolsPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// Turns the date written into a comment into a real date. The configured formats are tried first
    /// and in order, because 08.09.26 means different days in different notations and only the project
    /// can say which one it writes.
    /// </summary>
    /// <remarks>
    /// Reading a date and judging it are two jobs. What a date means and whether it has run out is
    /// decided by <see cref="Model.TodoDateRules"/>, which needs the project's reading of a date and
    /// nothing about notations.
    /// </remarks>
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
    }
}