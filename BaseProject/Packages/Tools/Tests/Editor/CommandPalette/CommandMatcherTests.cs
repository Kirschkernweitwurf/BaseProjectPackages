using System.Collections.Generic;
using Base.ToolsPackage.Editor.CommandPalette;
using Base.ToolsPackage.Editor.Shared;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the ranking that decides what the first row of the palette is. The exact numbers are
    /// tuning and are deliberately not asserted; what has to hold is the order between them, because
    /// the whole tool is worth using only when typing the name of a command puts it on top.
    /// </summary>
    public sealed class CommandMatcherTests
    {
        private const string ExactLeafPath = "Tools/Base/Copy";
        private const string LeafSubstringPath = "Tools/Base/Copy Component";
        private const string NonLeafSubstringPath = "Tools/Copy/Component";
        private const string ScatteredPath = "Create Object Package Yield";
        private const string Term = "copy";

        private readonly List<int> _matches = new();

        /// <summary>The list is reused across calls, so it starts every test in a known state.</summary>
        [SetUp]
        public void Prepare() => _matches.Clear();

        /// <summary>An empty term is not a filter, so everything survives it unranked.</summary>
        [Test]
        public void AnEmptyTermMatchesEverything()
        {
            Assert.That(CommandMatcher.TryMatch(Entry(ExactLeafPath), string.Empty, _matches, out int score),
                Is.True);

            Assert.That(score, Is.EqualTo(0));
            Assert.That(_matches, Is.Empty);
        }

        /// <summary>
        /// Typing the name of a command outright has to beat every looser way of arriving at it. This
        /// is the ordering the palette lives or dies on.
        /// </summary>
        [Test]
        public void AnExactLeafOutranksEveryLooserMatch()
        {
            int exact = ScoreOf(ExactLeafPath);
            int leafSubstring = ScoreOf(LeafSubstringPath);
            int nonLeafSubstring = ScoreOf(NonLeafSubstringPath);
            int scattered = ScoreOf(ScatteredPath);

            Assert.That(exact, Is.GreaterThan(leafSubstring));
            Assert.That(leafSubstring, Is.GreaterThan(nonLeafSubstring));
            Assert.That(nonLeafSubstring, Is.GreaterThan(scattered));
        }

        /// <summary>
        /// A run inside the last segment beats the same run earlier in the path, because the last
        /// segment is the command's own name and the rest is only where it was filed.
        /// </summary>
        [Test]
        public void ARunInsideTheLeafOutranksTheSameRunEarlierInThePath()
            => Assert.That(ScoreOf(LeafSubstringPath), Is.GreaterThan(ScoreOf(NonLeafSubstringPath)));

        /// <summary>
        /// The characters only have to appear in order, which is what lets a few letters reach a long
        /// path nobody wants to type out.
        /// </summary>
        [Test]
        public void ScatteredCharactersStillMatchWhenTheyAreInOrder()
            => Assert.That(CommandMatcher.TryMatch(Entry(ScatteredPath), Term, _matches, out int _), Is.True);

        /// <summary>A letter that is not there at all is a miss, not a weak match.</summary>
        [Test]
        public void ATermThatIsNotContainedDoesNotMatch()
        {
            Assert.That(CommandMatcher.TryMatch(Entry(ExactLeafPath), "xq", _matches, out int score), Is.False);

            Assert.That(score, Is.EqualTo(0));
            Assert.That(_matches, Is.Empty);
        }

        /// <summary>
        /// Order is the whole rule, so the same letters backwards must not match a path the forward
        /// spelling does.
        /// </summary>
        [Test]
        public void CharactersOutOfOrderDoNotMatch()
            => Assert.That(CommandMatcher.TryMatch(Entry(ExactLeafPath), "ypoc", _matches, out int _), Is.False);

        /// <summary>
        /// Every character reports where it landed, in ascending order, because the row highlights
        /// exactly those positions.
        /// </summary>
        [Test]
        public void EveryTermCharacterReportsItsPosition()
        {
            CommandMatcher.TryMatch(Entry(ScatteredPath), Term, _matches, out int _);

            Assert.That(_matches, Has.Count.EqualTo(Term.Length));
            Assert.That(_matches, Is.Ordered);
        }

        /// <summary>Builds an entry that does nothing when run, since only its path is scored.</summary>
        private static CommandEntry Entry(string path)
            => new(path, path, null, ECommandKind.MenuItem, EAssetOrigin.Project, () => { });

        /// <summary>Scores the given path against the shared term.</summary>
        private int ScoreOf(string path)
        {
            CommandMatcher.TryMatch(Entry(path), Term, _matches, out int score);

            return score;
        }
    }
}