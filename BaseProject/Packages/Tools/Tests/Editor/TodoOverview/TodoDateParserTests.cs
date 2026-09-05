using System;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using Base.ToolsPackage.Editor.TodoOverview.Scanning;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers how a date in a comment is read and where it lands relative to today. The whole point of
    /// configured formats is that 08.09.26 means different days in different notations, so a parser
    /// that guesses instead of following the project's order colors the wrong items overdue.
    /// </summary>
    public sealed class TodoDateParserTests
    {
        private const string Ambiguous = "08.09.26";
        private const string DayFirst = "dd.MM.yy";
        private const string MonthFirst = "MM.dd.yy";

        private static readonly string[] DayFirstFormats =
        {
            DayFirst
        };

        private static readonly string[] MonthFirstFormats =
        {
            MonthFirst
        };

        private static readonly string[] NoFormats = Array.Empty<string>();

        /// <summary>The first configured format wins, which is the only reason to configure any.</summary>
        [Test]
        public void TheConfiguredFormatDecidesWhatAnAmbiguousDateMeans()
        {
            TodoDateParser.TryParse(Ambiguous, DayFirstFormats, out DateTime dayFirst);
            TodoDateParser.TryParse(Ambiguous, MonthFirstFormats, out DateTime monthFirst);

            Assert.That(dayFirst, Is.EqualTo(new DateTime(2026, 9, 8)));
            Assert.That(monthFirst, Is.EqualTo(new DateTime(2026, 8, 9)));
        }

        /// <summary>Surrounding space is punctuation, not part of the date.</summary>
        [Test]
        public void SurroundingSpaceIsIgnored()
        {
            Assert.That(TodoDateParser.TryParse("  08.09.26  ", DayFirstFormats, out DateTime date), Is.True);
            Assert.That(date, Is.EqualTo(new DateTime(2026, 9, 8)));
        }

        /// <summary>
        /// With no formats configured the parser still reads what it can, so a project that never set
        /// any is not left with every date unread.
        /// </summary>
        [Test]
        public void AnUnconfiguredProjectStillReadsAnUnambiguousDate()
            => Assert.That(TodoDateParser.TryParse("2026-09-08", NoFormats, out DateTime _), Is.True);

        /// <summary>
        /// Text that is not a date has to be refused rather than turned into one, because the raw text
        /// is kept and shown and a wrong date would silently replace it.
        /// </summary>
        [Test]
        public void TextThatIsNotADateIsRefused()
        {
            Assert.That(TodoDateParser.TryParse("soon", DayFirstFormats, out DateTime date), Is.False);
            Assert.That(date, Is.EqualTo(default(DateTime)));
        }

        /// <summary>Nothing in means nothing out, not a crash.</summary>
        [Test]
        public void AMissingDateIsRefused()
        {
            Assert.That(TodoDateParser.TryParse(null, DayFirstFormats, out DateTime _), Is.False);
            Assert.That(TodoDateParser.TryParse("   ", DayFirstFormats, out DateTime _), Is.False);
        }

        /// <summary>An item with no date is not overdue, it is simply undated.</summary>
        [Test]
        public void AnItemWithoutADateHasNoState()
            => Assert.That(TodoDateParser.Resolve(null), Is.EqualTo(ETodoDateState.None));

        /// <summary>Yesterday is past due, which is the state the whole date column exists for.</summary>
        [Test]
        public void ADateInThePastIsOverdue()
            => Assert.That(TodoDateParser.Resolve(DateTime.Today.AddDays(-1)), Is.EqualTo(ETodoDateState.Overdue));

        /// <summary>
        /// Today is its own state rather than overdue, so an item due today does not read as already
        /// missed.
        /// </summary>
        [Test]
        public void ADateOfTodayIsNotYetOverdue()
            => Assert.That(TodoDateParser.Resolve(DateTime.Today), Is.EqualTo(ETodoDateState.Today));

        /// <summary>Tomorrow is still ahead, whatever time of day it is compared at.</summary>
        [Test]
        public void ADateInTheFutureIsFuture()
            => Assert.That(TodoDateParser.Resolve(DateTime.Today.AddDays(1)), Is.EqualTo(ETodoDateState.Future));

        /// <summary>
        /// The comparison is on the day, not the moment, so an item due later today is still due today
        /// rather than in the future.
        /// </summary>
        [Test]
        public void TheTimeOfDayDoesNotChangeTheState()
            => Assert.That(TodoDateParser.Resolve(DateTime.Today.AddHours(23)), Is.EqualTo(ETodoDateState.Today));
    }
}