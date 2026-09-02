using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers what a duration has to read like: only the units that carry a value are named, the
    /// singular form is used for exactly one, and a negative duration reads as none instead of a
    /// count that runs backwards.
    /// </summary>
    public sealed class TimeFormattingExtensionsTests
    {
        private const float FullDuration = 7530f;
        private const float MinuteDuration = 90f;
        private const float SecondDuration = 30f;
        private const float SingularDuration = 61f;

        /// <summary>A short duration names seconds only.</summary>
        [Test]
        public void SecondsAloneAreNamedAlone()
            => Assert.That(SecondDuration.ToMinutesSecondsText(), Is.EqualTo("30 seconds"));

        /// <summary>Once there is a minute, both units are named.</summary>
        [Test]
        public void MinutesAreNamedNextToSeconds()
            => Assert.That(MinuteDuration.ToMinutesSecondsText(), Is.EqualTo("1 minute and 30 seconds"));

        /// <summary>Once there is an hour, all three units are named.</summary>
        [Test]
        public void HoursAreNamedNextToMinutesAndSeconds()
            => Assert.That(FullDuration.ToMinutesSecondsText(), Is.EqualTo("2 hours, 5 minutes and 30 seconds"));

        /// <summary>A value of one drops the plural, so "1 seconds" cannot happen.</summary>
        [Test]
        public void ASingleUnitIsNamedInTheSingular()
            => Assert.That(SingularDuration.ToMinutesSecondsText(), Is.EqualTo("1 minute and 1 second"));

        /// <summary>A negative duration is treated as none, not as a countdown into the past.</summary>
        [Test]
        public void ANegativeDurationReadsAsNone()
            => Assert.That((-5f).ToMinutesSecondsText(), Is.EqualTo("0 seconds"));

        /// <summary>A fractional duration is rounded rather than truncated.</summary>
        [Test]
        public void AFractionalDurationIsRounded()
            => Assert.That(29.6f.ToMinutesSecondsText(), Is.EqualTo("30 seconds"));
    }
}