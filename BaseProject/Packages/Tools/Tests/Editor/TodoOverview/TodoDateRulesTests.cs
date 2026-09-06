using System;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers what a date on an item is taken to mean and how loudly it then asks to be looked at.
    /// <para>
    /// The same date reads two ways. A project that writes deadlines wants yesterday in red; a project
    /// that writes down when it wrote something wants yesterday left alone, or every item it has ever
    /// written turns red on the day after it was written and the column stops saying anything.
    /// </para>
    /// <para>
    /// The expected state is fixed per test rather than passed in, because the states are internal to
    /// the tool and a public test method cannot name one in its signature.
    /// </para>
    /// </summary>
    public sealed class TodoDateRulesTests
    {
        private const int AgingAfterDays = 30;
        private const int LongPast = 400;
        private const int StaleAfterDays = 90;

        /// <summary>A deadline that is still ahead is nothing to act on.</summary>
        /// <param name="daysUntil">Days between today and the deadline.</param>
        [TestCase(1)]
        [TestCase(LongPast)]
        public void ADeadlineStillAheadIsCalm(int daysUntil)
            => AssertState(ETodoDateMeaning.Due, -daysUntil, ETodoDateState.Normal);

        /// <summary>
        /// Today is its own step rather than overdue, so an item due today does not already read as
        /// missed while there is still a day left to do it in.
        /// </summary>
        [Test]
        public void ADeadlineOfTodayIsAWarning() => AssertState(ETodoDateMeaning.Due, 0, ETodoDateState.Warning);

        /// <summary>Yesterday is past due, which is the state the whole date column exists for.</summary>
        /// <param name="daysPast">Days since the deadline.</param>
        [TestCase(1)]
        [TestCase(LongPast)]
        public void ADeadlineInThePastIsAnAlert(int daysPast)
            => AssertState(ETodoDateMeaning.Due, daysPast, ETodoDateState.Alert);

        /// <summary>
        /// A note written recently is left alone. Under the other reading it would already be overdue,
        /// which is the whole reason the two readings cannot share one judgement.
        /// </summary>
        /// <param name="daysPast">Days since the note was written.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(AgingAfterDays - 1)]
        public void ARecentlyWrittenNoteIsCalm(int daysPast)
            => AssertState(ETodoDateMeaning.Written, daysPast, ETodoDateState.Normal);

        /// <summary>Past the aging line a note is worth a look, without being shouted about yet.</summary>
        /// <param name="daysPast">Days since the note was written.</param>
        [TestCase(AgingAfterDays)]
        [TestCase(StaleAfterDays - 1)]
        public void AnAgingNoteIsAWarning(int daysPast)
            => AssertState(ETodoDateMeaning.Written, daysPast, ETodoDateState.Warning);

        /// <summary>Past the stale line a note is what the list is there to surface.</summary>
        /// <param name="daysPast">Days since the note was written.</param>
        [TestCase(StaleAfterDays)]
        [TestCase(LongPast)]
        public void AnOldNoteIsStale(int daysPast)
            => AssertState(ETodoDateMeaning.Written, daysPast, ETodoDateState.Alert);

        /// <summary>A note dated in the future is a typo, not something to shout about.</summary>
        [Test]
        public void AWrittenDateInTheFutureStaysCalm()
            => AssertState(ETodoDateMeaning.Written, -5, ETodoDateState.Normal);

        /// <summary>
        /// The comparison is on the day rather than the moment, so an item due later today is still
        /// due today rather than already in the future.
        /// </summary>
        [Test]
        public void TheTimeOfDayDoesNotChangeTheState()
        {
            ETodoDateState state = Rules(ETodoDateMeaning.Due)
                .Resolve(DateTime.Today.AddHours(23), ETodoDateMeaning.Due);

            Assert.That(state, Is.EqualTo(ETodoDateState.Warning));
        }

        /// <summary>An item with no date has no state, whichever way the project reads its dates.</summary>
        [Test]
        public void AnItemWithoutADateHasNoState()
        {
            Assert.That(Rules(ETodoDateMeaning.Due).Resolve(Entry(null, null)),
                Is.EqualTo(ETodoDateState.None));

            Assert.That(Rules(ETodoDateMeaning.Written).Resolve(Entry(null, null)),
                Is.EqualTo(ETodoDateState.None));
        }

        /// <summary>
        /// An item that said what its own date means is judged that way, which is what lets one
        /// codebase carry deadlines and written dates at once.
        /// </summary>
        [Test]
        public void AnItemOverridesTheProjectsReading()
        {
            TodoDateRules rules = Rules(ETodoDateMeaning.Written);
            TodoEntry entry = Entry(DateTime.Today.AddDays(-1), ETodoDateMeaning.Due);

            Assert.That(rules.MeaningOf(entry), Is.EqualTo(ETodoDateMeaning.Due));
            Assert.That(rules.Resolve(entry), Is.EqualTo(ETodoDateState.Alert));
        }

        /// <summary>
        /// A stale line typed below the aging line would leave the middle step unreachable, so the two
        /// are pulled back into order rather than trusted as they were typed.
        /// </summary>
        [Test]
        public void AStaleLineBelowTheAgingLineIsPulledUp()
        {
            TodoDateRules rules = new(ETodoDateMeaning.Written, 60, 10);

            Assert.That(rules.StaleAfterDays, Is.EqualTo(60));
        }

        private static TodoDateRules Rules(ETodoDateMeaning meaning) => new(meaning, AgingAfterDays, StaleAfterDays);

        private static void AssertState(ETodoDateMeaning meaning, int daysPast, ETodoDateState expected)
        {
            TodoEntry entry = Entry(DateTime.Today.AddDays(-daysPast), null);

            Assert.That(Rules(meaning).Resolve(entry), Is.EqualTo(expected));
        }

        /// <summary>An item carrying nothing but the date being judged.</summary>
        private static TodoEntry Entry(DateTime? date, ETodoDateMeaning? meaning) => new(string.Empty, string.Empty,
            string.Empty,
            new TodoMetadata(string.Empty, string.Empty, string.Empty, date, meaning), string.Empty,
            string.Empty, 1, 0, 1);
    }
}