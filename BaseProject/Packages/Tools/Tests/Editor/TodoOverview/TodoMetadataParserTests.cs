using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using Base.ToolsPackage.Editor.TodoOverview.Scanning;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers how an item is split into message, owner and date. Every pattern is tried in turn and
    /// only counts when it fills something still missing, so a general notation can complete what a
    /// more specific one did not carry. What is cut out also has to leave a readable message behind,
    /// because that message is the whole row.
    /// </summary>
    public sealed class TodoMetadataParserTests
    {
        private const string BracketNotation = @"\((?<owner>[^,()]+),\s*(?<date>[0-9.]+)\)";
        private const string DateOnlyNotation = @"(?<date>[0-9]{2}\.[0-9]{2}\.[0-9]{2})";
        private const string DueNotation = @"due (?<date>\S+)";
        private const string GivenOwner = "Jonny";
        private const string OwnerOnlyNotation = @"@(?<owner>\w+)";

        private static readonly string[] DateFormats =
        {
            "dd.MM.yy"
        };

        private static readonly TodoTag[] NoTags = Array.Empty<TodoTag>();

        /// <summary>One notation carrying both is read in one pass and cut out of the message.</summary>
        [Test]
        public void OneNotationCarryingBothIsReadAtOnce()
        {
            TodoMetadata metadata = Parse("fix this (Jonny, 20.08.26)", BracketNotation);

            Assert.That(metadata.Owner, Is.EqualTo(GivenOwner));
            Assert.That(metadata.RawDate, Is.EqualTo("20.08.26"));
            Assert.That(metadata.Date, Is.EqualTo(new DateTime(2026, 8, 20)));
            Assert.That(metadata.Message, Is.EqualTo("fix this"));
        }

        /// <summary>
        /// Two notations each carrying half still produce a whole item, which is the reason every
        /// pattern is tried instead of only the first one that matches.
        /// </summary>
        [Test]
        public void TwoNotationsCompleteEachOther()
        {
            TodoMetadata metadata = Parse("fix @jonny by 20.08.26", OwnerOnlyNotation, DateOnlyNotation);

            Assert.That(metadata.Owner, Is.EqualTo("jonny"));
            Assert.That(metadata.RawDate, Is.EqualTo("20.08.26"));
        }

        /// <summary>
        /// The gap a cut leaves behind and the punctuation that separated the metadata are not part of
        /// the message, or every row would read with a hole in it.
        /// </summary>
        [Test]
        public void TheMessageIsLeftReadableAfterTheCut()
            => Assert.That(Parse("fix @jonny by 20.08.26", OwnerOnlyNotation, DateOnlyNotation).Message,
                Is.EqualTo("fix by"));

        /// <summary>
        /// A date that matches no configured format is still shown as it was written, because the
        /// alternative is silently dropping something the author put there on purpose.
        /// </summary>
        [Test]
        public void AnUnreadableDateIsKeptAsWritten()
        {
            TodoMetadata metadata = Parse("fix this due someday", DueNotation);

            Assert.That(metadata.RawDate, Is.EqualTo("someday"));
            Assert.That(metadata.Date, Is.Null);
        }

        /// <summary>An item with no notation in it is all message and nothing else.</summary>
        [Test]
        public void TextWithoutAnyNotationStaysTheMessage()
        {
            TodoMetadata metadata = Parse("fix this", BracketNotation);

            Assert.That(metadata.Message, Is.EqualTo("fix this"));
            Assert.That(metadata.Owner, Is.Empty);
            Assert.That(metadata.RawDate, Is.Empty);
            Assert.That(metadata.Date, Is.Null);
        }

        /// <summary>With nothing configured the text is left alone rather than mangled.</summary>
        [Test]
        public void AProjectWithoutNotationsKeepsTheWholeText()
            => Assert.That(Parse("fix this (Jonny, 20.08.26)").Message, Is.EqualTo("fix this (Jonny, 20.08.26)"));

        /// <summary>Reads one message with the given notations configured.</summary>
        private static TodoMetadata Parse(string message, params string[] notations)
            => TodoMetadataParser.Parse(message, Patterns(notations));

        /// <summary>Patterns carrying only the notations and the date format a test needs.</summary>
        private static TodoPatterns Patterns(IReadOnlyList<string> notations)
            => TodoPatterns.Create(new TodoPatternInput(NoTags, notations, DateFormats,
                ETodoContinuation.SingleLine, false));
    }
}