using System;

namespace Base.ToolsPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// How this project reads the date on an item, and where the line sits between calm, worth a look
    /// and needs attention.
    /// <para>
    /// A deadline and a note of when something was written are both written as a bare date and cannot
    /// be told apart by looking at one. A project says which it means by default, an item overrides
    /// that for itself, and the two live in the same codebase without either being read as the other.
    /// </para>
    /// <para>
    /// Plain values rather than a settings reference, so the judging can be reasoned about and tested
    /// without a project around it.
    /// </para>
    /// </summary>
    internal readonly struct TodoDateRules
    {
        /// <summary>What a date that does not say for itself means in this project.</summary>
        internal ETodoDateMeaning DefaultMeaning { get; }

        /// <summary>Days a written date may age before the item is worth a look.</summary>
        internal int AgingAfterDays { get; }

        /// <summary>Days a written date may age before the item counts as stale.</summary>
        internal int StaleAfterDays { get; }

        /// <summary>Creates the rules a list of items is judged by.</summary>
        /// <param name="defaultMeaning">What a date without a marker means.</param>
        /// <param name="agingAfterDays">Days before a written date is worth a look.</param>
        /// <param name="staleAfterDays">Days before a written date counts as stale.</param>
        internal TodoDateRules(ETodoDateMeaning defaultMeaning, int agingAfterDays, int staleAfterDays)
        {
            int aging = Math.Max(0, agingAfterDays);

            DefaultMeaning = defaultMeaning;

            // Clamped rather than trusted, because the two are typed independently in the settings
            // and a stale line below the aging line would leave the middle step unreachable.
            AgingAfterDays = aging;
            StaleAfterDays = Math.Max(aging, staleAfterDays);
        }

        /// <summary>Whole days between a date and today, negative while the date is still ahead.</summary>
        /// <param name="date">The date to measure.</param>
        /// <returns>The number of days that have passed since it.</returns>
        internal static int DaysPast(DateTime date) => (DateTime.Today - date.Date).Days;

        /// <summary>What the date on one item means, which the item may have said for itself.</summary>
        /// <param name="entry">The item to read.</param>
        /// <returns>The reading its date is judged by.</returns>
        internal ETodoDateMeaning MeaningOf(TodoEntry entry) => entry.DateMeaning ?? DefaultMeaning;

        /// <summary>How loudly an item's date is asking to be looked at.</summary>
        /// <param name="entry">The item to judge.</param>
        /// <returns>The state its pill is colored by.</returns>
        internal ETodoDateState Resolve(TodoEntry entry)
        {
            if (!entry.Date.HasValue)
                return ETodoDateState.None;

            return Resolve(entry.Date.Value, MeaningOf(entry));
        }

        /// <summary>How loudly a date read a given way is asking to be looked at.</summary>
        /// <param name="date">The date to judge.</param>
        /// <param name="meaning">The reading to judge it by.</param>
        /// <returns>The state its pill is colored by.</returns>
        internal ETodoDateState Resolve(DateTime date, ETodoDateMeaning meaning) => meaning == ETodoDateMeaning.Due
            ? ResolveDue(date)
            : ResolveWritten(date);

        private static ETodoDateState ResolveDue(DateTime date)
        {
            int past = DaysPast(date);

            if (past > 0)
                return ETodoDateState.Alert;

            // Today is its own step rather than overdue, so an item due today does not already read
            // as missed while there is still a day left to do it in.
            return past == 0
                ? ETodoDateState.Warning
                : ETodoDateState.Normal;
        }

        // A note dated in the future is a typo rather than a stale note, so it stays calm and the
        // date is left on the pill for whoever wrote it to notice.
        private ETodoDateState ResolveWritten(DateTime date)
        {
            int age = DaysPast(date);

            if (age >= StaleAfterDays)
                return ETodoDateState.Alert;

            return age >= AgingAfterDays
                ? ETodoDateState.Warning
                : ETodoDateState.Normal;
        }
    }
}