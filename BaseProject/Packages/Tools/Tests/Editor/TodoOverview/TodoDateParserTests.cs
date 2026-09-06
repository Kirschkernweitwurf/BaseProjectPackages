using System;
using Base.ToolsPackage.Editor.TodoOverview.Scanning;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers how a date in a comment is read. The whole point of configured formats is that 08.09.26
    /// means different days in different notations, so a parser that guesses instead of following the
    /// project's order dates half the list wrong.
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

        /// <summary>Whitespace around a date is the comment's, not the date's.</summary>
        [Test]
        public void SurroundingWhitespaceIsIgnored()
        {
            Assert.That(TodoDateParser.TryParse("  08.09.26  ", DayFirstFormats, out DateTime date), Is.True);
            Assert.That(date, Is.EqualTo(new DateTime(2026, 9, 8)));
        }

        /// <summary>
        /// A project that configured no formats still gets the unambiguous notations, so the tool is
        /// useful before anyone opens its settings page.
        /// </summary>
        [Test]
        public void AnUnconfiguredProjectStillReadsAnUnambiguousDate()
            => Assert.That(TodoDateParser.TryParse("2026-09-08", NoFormats, out DateTime _), Is.True);

        /// <summary>
        /// Words are left to the raw text rather than turned into a date, because a date invented here
        /// would color an item by a day nobody wrote.
        /// </summary>
        [Test]
        public void TextThatIsNotADateIsRefused()
        {
            Assert.That(TodoDateParser.TryParse("soon", DayFirstFormats, out DateTime date), Is.False);
            Assert.That(date, Is.EqualTo(default(DateTime)));
        }

        /// <summary>Nothing at all is not a date either.</summary>
        [Test]
        public void AMissingDateIsRefused()
        {
            Assert.That(TodoDateParser.TryParse(null, DayFirstFormats, out DateTime _), Is.False);
            Assert.That(TodoDateParser.TryParse("   ", DayFirstFormats, out DateTime _), Is.False);
        }
    }
}