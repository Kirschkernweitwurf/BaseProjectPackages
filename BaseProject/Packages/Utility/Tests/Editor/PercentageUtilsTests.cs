using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the conversion between the normalized values the code works in and the percentages a
    /// player reads, in both directions and in the formatted form.
    /// </summary>
    public sealed class PercentageUtilsTests
    {
        private const float Normalized = 0.56f;
        private const float Percent = 56f;
        private const string PercentSymbol = "%";
        private const string PercentText = "56";
        private const float Tolerance = 0.0001f;

        /// <summary>A normalized value scales up to the percentage a player sees.</summary>
        [Test]
        public void ANormalizedValueBecomesAPercentage() => Assert.That(PercentageUtils.ToPercent(Normalized),
            Is.EqualTo(Percent).Within(Tolerance));

        /// <summary>A percentage scales back down to the value the code works in.</summary>
        [Test]
        public void APercentageBecomesANormalizedValue() => Assert.That(PercentageUtils.FromPercent(Percent),
            Is.EqualTo(Normalized).Within(Tolerance));

        /// <summary>Going out and back has to land on the value it started from.</summary>
        [Test]
        public void TheConversionsUndoEachOther() => Assert.That(
            PercentageUtils.FromPercent(PercentageUtils.ToPercent(Normalized)),
            Is.EqualTo(Normalized).Within(Tolerance));

        /// <summary>
        /// The formatted value carries the symbol and the whole number. The decimal separator is left
        /// out of the assertion on purpose, since it depends on the culture the tests run under.
        /// </summary>
        [Test]
        public void TheFormattedValueCarriesTheNumberAndTheSymbol()
        {
            string formatted = PercentageUtils.ToPercentString(Normalized);

            Assert.That(formatted, Does.StartWith(PercentText));
            Assert.That(formatted, Does.EndWith(PercentSymbol));
        }

        /// <summary>Asking for decimals has to make the text longer, not round them away.</summary>
        [Test]
        public void DecimalsShowUpInTheFormattedValue()
        {
            string plain = PercentageUtils.ToPercentString(Normalized);
            string detailed = PercentageUtils.ToPercentString(Normalized, 2);

            Assert.That(detailed.Length, Is.GreaterThan(plain.Length));
            Assert.That(detailed, Does.EndWith(PercentSymbol));
        }
    }
}