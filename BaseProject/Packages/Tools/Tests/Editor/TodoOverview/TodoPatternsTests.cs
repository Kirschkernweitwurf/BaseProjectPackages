using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using Base.ToolsPackage.Editor.TodoOverview.Scanning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers what a scan is compiled into. A keyword that matches inside a longer word turns every
    /// mention of a word into a task, a pattern the user typed can be invalid, and the cheap text
    /// check in front of the real match decides whether a file is lexed at all.
    /// </summary>
    public sealed class TodoPatternsTests
    {
        private const string InvalidPattern = "(unclosed";
        private const string InvalidPatternWarning = "ignoring the invalid pattern";
        private const string Keyword = "TODO";
        private const string ValidPattern = @"\((?<owner>[^,()]+)\)";

        private static readonly string[] NoPatterns = Array.Empty<string>();
        private static readonly TodoTag[] NoTags = Array.Empty<TodoTag>();

        /// <summary>An enabled keyword is what the cheap pre-check looks for.</summary>
        [Test]
        public void AnEnabledKeywordIsFoundInASource()
            => Assert.That(PatternsFor(Tag(Keyword, true)).ContainsKeyword("// TODO fix this"), Is.True);

        /// <summary>A keyword that was switched off is not looked for at all.</summary>
        [Test]
        public void ADisabledKeywordIsNotLookedFor()
        {
            TodoPatterns patterns = PatternsFor(Tag(Keyword, false));

            Assert.That(patterns.HasKeywords, Is.False);
            Assert.That(patterns.ContainsKeyword("// TODO fix this"), Is.False);
        }

        /// <summary>A keyword of only whitespace is not a keyword, so it is dropped rather than compiled.</summary>
        [Test]
        public void ABlankKeywordIsDropped() => Assert.That(PatternsFor(Tag("   ", true)).HasKeywords, Is.False);

        /// <summary>
        /// The keyword has to stand on its own. Without that, a comment mentioning "todos" or a class
        /// called "TodoEntry" would every one of them become a task.
        /// </summary>
        [Test]
        public void AKeywordOnlyMatchesAsAWholeWord()
        {
            Regex keywords = PatternsFor(Tag(Keyword, true)).Keywords;

            Assert.That(keywords.IsMatch("// TODO fix"), Is.True);
            Assert.That(keywords.IsMatch("// TODOS everywhere"), Is.False);
            Assert.That(keywords.IsMatch("// MYTODO"), Is.False);
        }

        /// <summary>Casing is a project decision, so an insensitive scan finds a lowercase spelling.</summary>
        [Test]
        public void AnInsensitiveScanFindsAnyCasing()
            => Assert.That(PatternsFor(Tag(Keyword, true)).Keywords.IsMatch("// todo fix"), Is.True);

        /// <summary>A sensitive scan finds only the spelling that was configured.</summary>
        [Test]
        public void ASensitiveScanFindsOnlyTheConfiguredCasing()
        {
            Regex keywords = Patterns(new[]
                {
                    Tag(Keyword, true)
                }, NoPatterns, true)
                .Keywords;

            Assert.That(keywords.IsMatch("// TODO fix"), Is.True);
            Assert.That(keywords.IsMatch("// todo fix"), Is.False);
        }

        /// <summary>
        /// The pre-check ignores casing even for a sensitive scan. It only decides whether a file is
        /// worth lexing, so being too generous costs a file read and being too strict loses items.
        /// </summary>
        [Test]
        public void ThePreCheckIgnoresCasingEvenForASensitiveScan()
        {
            TodoPatterns patterns = Patterns(new[]
            {
                Tag(Keyword, true)
            }, NoPatterns, true);

            Assert.That(patterns.ContainsKeyword("// todo fix this"), Is.True);
        }

        /// <summary>A shouted keyword and a quiet one end up in the same section.</summary>
        [Test]
        public void AMatchIsReportedInTheConfiguredCasing()
            => Assert.That(PatternsFor(Tag(Keyword, true)).Resolve("todo"), Is.EqualTo(Keyword));

        /// <summary>A keyword whose tag is gone keeps the spelling it was found with.</summary>
        [Test]
        public void AnUnknownMatchKeepsItsOwnSpelling()
            => Assert.That(PatternsFor(Tag(Keyword, true)).Resolve("HACK"), Is.EqualTo("HACK"));

        /// <summary>A pattern the user typed can be invalid, and one bad one must not lose the rest.</summary>
        [Test]
        public void AnInvalidPatternIsReportedAndDropped()
        {
            LogAssert.Expect(LogType.Warning, new Regex(InvalidPatternWarning));

            TodoPatterns patterns = Patterns(NoTags, new[]
                {
                    InvalidPattern,
                    ValidPattern
                },
                false);

            Assert.That(patterns.Metadata, Has.Count.EqualTo(1));
        }

        /// <summary>Blank formats are not formats, and a stray space around one is not part of it.</summary>
        [Test]
        public void BlankDateFormatsAreDroppedAndTheRestAreTrimmed()
        {
            TodoPatterns patterns = TodoPatterns.Create(new TodoPatternInput(NoTags, NoPatterns,
                new[]
                {
                    "  dd.MM.yy  ",
                    "   "
                }, ETodoContinuation.SingleLine, false));

            Assert.That(patterns.DateFormats, Has.Length.EqualTo(1));
            Assert.That(patterns.DateFormats[0], Is.EqualTo("dd.MM.yy"));
        }

        /// <summary>The compiler reads plain values, so a missing list is a bug rather than an empty scan.</summary>
        [Test]
        public void AMissingListIsRefused() => Assert.Throws<ArgumentNullException>(() => new TodoPatternInput(null,
            NoPatterns, NoPatterns,
            ETodoContinuation.SingleLine, false));

        /// <summary>One keyword definition in the color the color does not matter in.</summary>
        private static TodoTag Tag(string keyword, bool enabled) => new(keyword, Color.white, enabled);

        /// <summary>Patterns compiled for one keyword, with nothing else configured.</summary>
        private static TodoPatterns PatternsFor(TodoTag tag) => Patterns(new[]
        {
            tag
        }, NoPatterns, false);

        /// <summary>Patterns compiled from the given keywords and notations.</summary>
        private static TodoPatterns Patterns(IReadOnlyList<TodoTag> tags, IReadOnlyList<string> metadata,
            bool caseSensitive) => TodoPatterns.Create(new TodoPatternInput(tags, metadata, NoPatterns,
            ETodoContinuation.SingleLine, caseSensitive));
    }
}