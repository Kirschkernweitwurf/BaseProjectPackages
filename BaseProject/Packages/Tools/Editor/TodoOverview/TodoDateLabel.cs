using System.Globalization;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using Base.ToolsPackage.Editor.TodoOverview.Settings;

namespace Base.ToolsPackage.Editor.TodoOverview
{
    /// <summary>
    /// Turns the date on an item into the text the list shows for it, in the one notation the project
    /// chose, never in the notation the comment happened to be written in.
    /// </summary>
    /// <remarks>
    /// The scan reads several notations on purpose, because a codebase collects them and refusing to
    /// understand one only loses the item. Showing them back the way they were written is a separate
    /// decision and the wrong one: a column that reads 20.08.26 next to 2026-09-15 makes the reader
    /// compare notations before dates, and looks unsorted even while it is sorted correctly.
    /// <para>
    /// Formatted rather than precomputed on the item, so changing the setting redraws the list
    /// instead of needing the project scanned again.
    /// </para>
    /// </remarks>
    internal static class TodoDateLabel
    {
        private const string TooltipFormat = "{0}  ({1})";

        /// <summary>The text shown on an item's date pill.</summary>
        /// <param name="entry">The item whose date is being drawn.</param>
        /// <returns>The date in the chosen notation, or the raw text when it could not be read.</returns>
        internal static string Of(TodoEntry entry)
        {
            // A date no format matched is left exactly as it was written. Rewriting it is impossible
            // and dropping it would hide the typo that is the reason it did not parse.
            if (!entry.Date.HasValue)
                return entry.RawDate;

            TodoSettings settings = TodoSettings.instance;

            if (settings.DateDisplay == ETodoDateDisplay.Regional)
                return entry.Date.Value.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
                    CultureInfo.CurrentCulture);

            return entry.Date.Value.ToString(ProjectFormat(settings), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// What the pill says when it is hovered: where the date sits relative to today, and the text
        /// it was written as, so a date that looks wrong can be checked against the comment itself.
        /// </summary>
        /// <param name="entry">The item whose date is being drawn.</param>
        /// <param name="rules">How this project reads a date.</param>
        /// <returns>The tooltip text.</returns>
        internal static string TooltipOf(TodoEntry entry, TodoDateRules rules)
        {
            if (!entry.Date.HasValue)
                return entry.RawDate;

            string relative = TodoDateWords.Relative(TodoDateRules.DaysPast(entry.Date.Value),
                rules.MeaningOf(entry));

            return string.Format(TooltipFormat, relative, entry.RawDate);
        }

        // The same culture the formats are parsed with, so a project format round trips instead of
        // having its separators swapped out by whatever region the machine is set to.
        private static string ProjectFormat(TodoSettings settings)
        {
            if (settings.DateFormats.Count > 0)
                return settings.DateFormats[0];

            return CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
        }
    }
}