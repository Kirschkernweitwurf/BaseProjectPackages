using Base.EditorUIPackage.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Base.EditorUIPackage.Tests
{
    /// <summary>
    /// The marker on the column a list is sorted by.
    /// <para>
    /// It was stacked one pixel rectangles, which only stay one pixel while the editor draws on whole
    /// pixels. At 125 or 150 percent display scaling each row lands between two of them and comes out
    /// at half brightness, so the arrow read as a smudge rather than as a shape. It is a glyph now,
    /// because text is the one thing here already resolved against the real pixel grid.
    /// </para>
    /// </summary>
    public sealed class EditorRowsSortArrowTests
    {
        private const string Ascending = "\u25B2";
        private const string Descending = "\u25BC";
        private const int MinimumWidth = 3;

        /// <summary>Ascending points up, which is the way that order reads.</summary>
        [Test]
        public void AscendingPointsUp()
            => Assert.That(EditorRows.SortArrowContent(ESortOrder.Ascending).text, Is.EqualTo(Ascending));

        /// <summary>Descending points down.</summary>
        [Test]
        public void DescendingPointsDown()
            => Assert.That(EditorRows.SortArrowContent(ESortOrder.Descending).text, Is.EqualTo(Descending));

        /// <summary>The two orders never draw the same glyph, or the marker says nothing.</summary>
        [Test]
        public void TheTwoOrdersDrawDifferentGlyphs()
            => Assert.That(EditorRows.SortArrowContent(ESortOrder.Ascending).text,
                Is.Not.EqualTo(EditorRows.SortArrowContent(ESortOrder.Descending).text));

        /// <summary>An unsorted column draws nothing at all.</summary>
        [Test]
        public void AnUnsortedColumnDrawsNothing()
            => Assert.That(EditorRows.SortArrowContent(ESortOrder.Default).text, Is.Empty);

        /// <summary>The glyph keeps the column and the row height it was given.</summary>
        [Test]
        public void TheGlyphKeepsTheColumnItWasGiven()
        {
            Rect given = new(40f, 12f, 7f, 20f);
            Rect area = EditorRows.SortArrowArea(given);

            Assert.That(area.x, Is.EqualTo(given.x));
            Assert.That(area.height, Is.EqualTo(given.height));
        }

        /// <summary>
        /// It sits a little below where the box would centre it. A font box reserves room under the
        /// baseline for descenders a triangle does not have, so centering the box leaves the ink high.
        /// </summary>
        [Test]
        public void TheGlyphSitsBelowTheBoxCentre()
        {
            Rect given = new(40f, 12f, 7f, 20f);

            Assert.That(EditorRows.SortArrowArea(given).y, Is.GreaterThan(given.y));
        }

        /// <summary>
        /// A theme that shrinks the arrow to nothing still leaves something to draw, rather than a
        /// zero width rectangle the glyph is clipped out of.
        /// </summary>
        [Test]
        public void AnArrowIsNeverNarrowedAway()
            => Assert.That(EditorRows.SortArrowArea(new Rect(0f, 0f, 0f, 20f)).width,
                Is.GreaterThanOrEqualTo(MinimumWidth));
    }
}