using Base.ToolsPackage.Editor.TodoOverview.Model;

namespace Base.ToolsPackage.Editor.TodoOverview
{
    /// <summary>
    /// The words the list uses for a date, which follow what the project's dates mean.
    /// <para>
    /// A column headed Due over dates that record when a note was written is worse than no header at
    /// all, and a filter called Overdue over notes that were never due is a red pill on every item in
    /// the project. Both readings get their own vocabulary, kept here so the window, the column titles
    /// and the pill tooltips cannot drift apart.
    /// </para>
    /// </summary>
    internal static class TodoDateWords
    {
        private const string DueColumn = "Due";
        private const string DueFilter = "Overdue";
        private const string DueFilterTooltip = "Show only the items whose deadline has passed";
        private const string DueInFormat = "Due in {0} days";
        private const string DueToday = "Due today";
        private const string DueTomorrow = "Due tomorrow";
        private const string OverdueFormat = "Overdue by {0} days";
        private const string OverdueYesterday = "Was due yesterday";
        private const string WrittenAgoFormat = "Written {0} days ago";
        private const string WrittenColumn = "Written";
        private const string WrittenFilter = "Stale";
        private const string WrittenFilterTooltip = "Show only the items that have been sitting there longest";
        private const string WrittenToday = "Written today";
        private const string WrittenTomorrow = "Dated tomorrow";
        private const string WrittenYesterday = "Written yesterday";

        /// <summary>The title of the date column.</summary>
        /// <param name="meaning">What the project's dates mean.</param>
        /// <returns>The column title.</returns>
        internal static string Column(ETodoDateMeaning meaning) => meaning == ETodoDateMeaning.Due
            ? DueColumn
            : WrittenColumn;

        /// <summary>The label on the pill that filters the list down to what needs attention.</summary>
        /// <param name="meaning">What the project's dates mean.</param>
        /// <returns>The pill label.</returns>
        internal static string Filter(ETodoDateMeaning meaning) => meaning == ETodoDateMeaning.Due
            ? DueFilter
            : WrittenFilter;

        /// <summary>What that pill says when it is hovered.</summary>
        /// <param name="meaning">What the project's dates mean.</param>
        /// <returns>The pill tooltip.</returns>
        internal static string FilterTooltip(ETodoDateMeaning meaning) => meaning == ETodoDateMeaning.Due
            ? DueFilterTooltip
            : WrittenFilterTooltip;

        /// <summary>Where a date sits relative to today, in words rather than as a number of days.</summary>
        /// <param name="daysPast">Days since the date, negative while it is still ahead.</param>
        /// <param name="meaning">What the date means.</param>
        /// <returns>The phrase for the pill tooltip.</returns>
        internal static string Relative(int daysPast, ETodoDateMeaning meaning) => meaning == ETodoDateMeaning.Due
            ? RelativeDue(daysPast)
            : RelativeWritten(daysPast);

        private static string RelativeDue(int daysPast)
        {
            if (daysPast == 0)
                return DueToday;

            if (daysPast == 1)
                return OverdueYesterday;

            if (daysPast == -1)
                return DueTomorrow;

            return daysPast > 0
                ? string.Format(OverdueFormat, daysPast)
                : string.Format(DueInFormat, -daysPast);
        }

        // A written date in the future is a typo rather than a plan, so it is named as a date rather
        // than described as something that is going to happen.
        private static string RelativeWritten(int daysPast)
        {
            if (daysPast == 0)
                return WrittenToday;

            if (daysPast == 1)
                return WrittenYesterday;

            return daysPast > 0
                ? string.Format(WrittenAgoFormat, daysPast)
                : WrittenTomorrow;
        }
    }
}