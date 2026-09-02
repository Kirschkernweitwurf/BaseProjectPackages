using NUnit.Framework;

namespace Base.EditorUIPackage.Editor.Tests
{
    /// <summary>
    /// Covers the numbers every Base window lays out by. They are user editable, so the guards on the
    /// way out matter more than the values themselves: a row height of zero or a font size of zero
    /// would leave a window drawing nothing at all.
    /// </summary>
    public sealed class EditorThemeMetricsTests
    {
        private const float Tolerance = 0.0001f;

        /// <summary>The values a project falls back to are all usable.</summary>
        [Test]
        public void TheFallbackMetricsAreUsable()
        {
            EditorThemeMetrics metrics = EditorThemeDefaults.CreateMetrics();

            Assert.That(metrics.RowHeight, Is.GreaterThan(0f));
            Assert.That(metrics.HeaderHeight, Is.GreaterThan(0f));
            Assert.That(metrics.TitleFontSize, Is.GreaterThan(0));
            Assert.That(metrics.DescriptionFontSize, Is.GreaterThan(0));
            Assert.That(metrics.DividerThickness, Is.GreaterThan(0f));
            Assert.That(metrics.SeparatorThickness, Is.GreaterThan(0f));
        }

        /// <summary>A title reads larger than the sentence under it.</summary>
        [Test]
        public void TheTitleIsLargerThanItsDescription()
        {
            EditorThemeMetrics metrics = EditorThemeDefaults.CreateMetrics();

            Assert.That(metrics.TitleFontSize, Is.GreaterThan(metrics.DescriptionFontSize));
        }

        /// <summary>A divider is easier to grab than it is to see, so dragging one is not fiddly.</summary>
        [Test]
        public void ADividerIsWiderToGrabThanToSee()
        {
            EditorThemeMetrics metrics = EditorThemeDefaults.CreateMetrics();

            Assert.That(metrics.DividerHitWidth, Is.GreaterThanOrEqualTo(metrics.DividerThickness));
        }

        /// <summary>The values handed in are the values that come back.</summary>
        [Test]
        public void TheMetricsKeepWhatTheyWereBuiltWith()
        {
            EditorThemeMetrics metrics = Build(24f, 30, 3);

            Assert.That(metrics.RowHeight, Is.EqualTo(24f).Within(Tolerance));
            Assert.That(metrics.TitleFontSize, Is.EqualTo(30));
            Assert.That(metrics.CardCornerRadius, Is.EqualTo(3));
        }

        /// <summary>
        /// A row of no height would leave a list drawing nothing, so the value is lifted to something
        /// that still shows.
        /// </summary>
        [Test]
        public void ARowAlwaysHasSomeHeight()
        {
            Assert.That(Build(0f, 12, 4).RowHeight, Is.GreaterThan(0f));
            Assert.That(Build(-10f, 12, 4).RowHeight, Is.GreaterThan(0f));
        }

        /// <summary>Text of no size would be invisible, so both font sizes are lifted too.</summary>
        [Test]
        public void TextAlwaysHasSomeSize()
        {
            EditorThemeMetrics metrics = new(16f, 14f, 6, 0, 8f, 1f, 20f, 0.06f, 14f, 8f, 8, 18f, 0.08f, 22f,
                6f, 12f, 1f, 8f, 4f, 0);

            Assert.That(metrics.TitleFontSize, Is.GreaterThan(0));
            Assert.That(metrics.DescriptionFontSize, Is.GreaterThan(0));
        }

        /// <summary>A corner radius cannot go below square, on a card or on a pill.</summary>
        [Test]
        public void ACornerRadiusCannotGoBelowSquare()
        {
            EditorThemeMetrics metrics = Build(20f, 12, -5);

            Assert.That(metrics.CardCornerRadius, Is.EqualTo(0));
            Assert.That(metrics.PillCornerRadius, Is.EqualTo(0));
        }

        /// <summary>A line thickness cannot fall below a single pixel.</summary>
        [Test]
        public void ALineIsNeverThinnerThanAPixel()
        {
            EditorThemeMetrics metrics = new(16f, 14f, 6, 11, 0f, 0f, 20f, 0.06f, 14f, 8f, 8, 18f, 0.08f, 22f,
                6f, 12f, 0f, 8f, 4f, 15);

            Assert.That(metrics.DividerThickness, Is.GreaterThanOrEqualTo(1f));
            Assert.That(metrics.DividerHitWidth, Is.GreaterThanOrEqualTo(1f));
            Assert.That(metrics.SeparatorThickness, Is.GreaterThanOrEqualTo(1f));
        }

        /// <summary>An untouched set still answers with usable numbers.</summary>
        [Test]
        public void AnUntouchedSetIsStillUsable()
        {
            EditorThemeMetrics metrics = new();

            Assert.That(metrics.RowHeight, Is.GreaterThan(0f));
            Assert.That(metrics.TitleFontSize, Is.GreaterThan(0));
            Assert.That(metrics.DividerThickness, Is.GreaterThanOrEqualTo(1f));
        }

        // Only the three values a test varies are spelled out; the rest are the defaults so the
        // constructor call does not drown the assertion it belongs to. The corner radius drives both
        // the card and the pill, since a test that varies one wants to see the other clamp too.
        private static EditorThemeMetrics Build(float rowHeight, int titleFontSize, int cornerRadius)
            => new(16f, 14f, cornerRadius, 11, 8f, 1f, 20f, 0.06f, 14f, 8f, cornerRadius, 18f, 0.08f, rowHeight,
                6f, 12f, 1f, 8f, 4f, titleFontSize);
    }
}