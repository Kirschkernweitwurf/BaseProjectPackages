using Base.ToolsPackage.Editor.CommandPalette;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers how the search box is read. Every keystroke goes through here, so a marker that is not
    /// recognized either narrows nothing or drops the letters after it out of the term, and in both
    /// cases the palette answers a question the user did not ask.
    /// </summary>
    public sealed class CommandFilterTests
    {
        private const string GivenTag = "audio";
        private const string GivenTerm = "copy";

        /// <summary>Plain words are the term, lowercased so the matcher can compare directly.</summary>
        [Test]
        public void PlainWordsBecomeTheTerm()
        {
            CommandFilter filter = CommandFilter.Parse("COPY");

            Assert.That(filter.Term, Is.EqualTo(GivenTerm));
            Assert.That(filter.Kind, Is.Null);
            Assert.That(filter.Tags, Is.Empty);
        }

        /// <summary>
        /// Spaces are dropped rather than kept, because the matcher scores a subsequence and a typed
        /// "copy component" has to reach a path written "Copy Component".
        /// </summary>
        [Test]
        public void SeveralWordsAreJoinedWithoutSpaces()
            => Assert.That(CommandFilter.Parse("copy component").Term, Is.EqualTo("copycomponent"));

        /// <summary>The angle bracket narrows to menu items and is not part of what is searched.</summary>
        [Test]
        public void TheMenuItemMarkerNarrowsTheKind()
        {
            CommandFilter filter = CommandFilter.Parse($"{CommandFilter.MenuItemMarker}{GivenTerm}");

            Assert.That(filter.Kind, Is.EqualTo(ECommandKind.MenuItem));
            Assert.That(filter.Term, Is.EqualTo(GivenTerm));
        }

        /// <summary>The plus narrows to asset creation.</summary>
        [Test]
        public void TheCreateAssetMarkerNarrowsTheKind()
            => Assert.That(CommandFilter.Parse($"{CommandFilter.CreateAssetMarker}{GivenTerm}").Kind,
                Is.EqualTo(ECommandKind.CreateAsset));

        /// <summary>The at sign narrows to settings pages.</summary>
        [Test]
        public void TheSettingsMarkerNarrowsTheKind()
            => Assert.That(CommandFilter.Parse($"{CommandFilter.SettingsMarker}{GivenTerm}").Kind,
                Is.EqualTo(ECommandKind.Settings));

        /// <summary>A tag narrows the result without being searched for in any path.</summary>
        [Test]
        public void ATagIsCollectedAndKeptOutOfTheTerm()
        {
            CommandFilter filter = CommandFilter.Parse($"{CommandFilter.TagMarker}{GivenTag}");

            Assert.That(filter.Tags, Contains.Item(GivenTag));
            Assert.That(filter.Term, Is.Empty);
        }

        /// <summary>
        /// A marker on its own is somebody midway through typing, so it narrows the kind and adds
        /// nothing to the term rather than being treated as a word.
        /// </summary>
        [Test]
        public void ABareTagMarkerAddsNoTag()
        {
            CommandFilter filter = CommandFilter.Parse(CommandFilter.TagMarker.ToString());

            Assert.That(filter.Tags, Is.Empty);
            Assert.That(filter.Term, Is.Empty);
        }

        /// <summary>Markers and words combine, which is the point of parsing tokens rather than a line.</summary>
        [Test]
        public void MarkersAndWordsCombine()
        {
            CommandFilter filter =
                CommandFilter.Parse($"{CommandFilter.MenuItemMarker} {GivenTerm} {CommandFilter.TagMarker}{GivenTag}");

            Assert.That(filter.Kind, Is.EqualTo(ECommandKind.MenuItem));
            Assert.That(filter.Term, Is.EqualTo(GivenTerm));
            Assert.That(filter.Tags, Contains.Item(GivenTag));
        }

        /// <summary>An empty box narrows nothing, so the palette shows everything.</summary>
        [Test]
        public void AnEmptyBoxNarrowsNothing()
        {
            CommandFilter filter = CommandFilter.Parse("   ");

            Assert.That(filter.Term, Is.Empty);
            Assert.That(filter.Tags, Is.Empty);
            Assert.That(filter.Kind, Is.Null);
        }

        /// <summary>Nothing in means nothing out, not a crash.</summary>
        [Test]
        public void AMissingBoxIsHandled()
            => Assert.That(CommandFilter.Parse(null).Term, Is.Empty);
    }
}