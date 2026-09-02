using System;
using Base.UtilityPackage.Serialization;
using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the reason the duration is stored as ticks: a value authored as one hour has to be
    /// exactly one hour on the way back out, which is what a float of seconds loses.
    /// </summary>
    public sealed class SerializableTimeSpanTests
    {
        private const float SecondsTolerance = 0.0001f;

        /// <summary>A duration comes back as the same duration.</summary>
        [Test]
        public void ADurationSurvivesTheRoundTrip()
        {
            TimeSpan original = TimeSpan.FromHours(1);
            SerializableTimeSpan stored = new(original);

            Assert.That(stored.Value, Is.EqualTo(original));
            Assert.That(stored.Ticks, Is.EqualTo(original.Ticks));
        }

        /// <summary>Storing ticks directly describes the same duration.</summary>
        [Test]
        public void ATickCountDescribesTheSameDuration()
        {
            SerializableTimeSpan stored = new(TimeSpan.TicksPerMinute);

            Assert.That(stored.Value, Is.EqualTo(TimeSpan.FromMinutes(1)));
        }

        /// <summary>The seconds shortcut matches the duration it is taken from.</summary>
        [Test]
        public void TheSecondsShortcutMatchesTheDuration()
        {
            SerializableTimeSpan stored = new(TimeSpan.FromSeconds(90));

            Assert.That(stored.Seconds, Is.EqualTo(90f).Within(SecondsTolerance));
        }

        /// <summary>A negative duration is legal and is kept as authored.</summary>
        [Test]
        public void ANegativeDurationIsKept()
        {
            SerializableTimeSpan stored = new(TimeSpan.FromSeconds(-5));

            Assert.That(stored.Ticks, Is.LessThan(0));
            Assert.That(stored.Value, Is.EqualTo(TimeSpan.FromSeconds(-5)));
        }

        /// <summary>The conversions in both directions have to line up.</summary>
        [Test]
        public void TheImplicitConversionsMatch()
        {
            SerializableTimeSpan stored = TimeSpan.FromMinutes(5);
            TimeSpan unwrapped = stored;

            Assert.That(unwrapped, Is.EqualTo(TimeSpan.FromMinutes(5)));
        }

        /// <summary>Two equal durations compare equal through every path.</summary>
        [Test]
        public void EqualDurationsCompareEqual()
        {
            SerializableTimeSpan first = new(TimeSpan.FromMinutes(5));
            SerializableTimeSpan second = new(TimeSpan.FromMinutes(5));

            Assert.That(second, Is.EqualTo(first));
            Assert.That(first == second, Is.True);
            Assert.That(first != second, Is.False);
            Assert.That(second.GetHashCode(), Is.EqualTo(first.GetHashCode()));
            Assert.That(first.CompareTo(second), Is.EqualTo(0));
        }

        /// <summary>Ordering follows the length of the durations.</summary>
        [Test]
        public void ShorterDurationsOrderFirst()
        {
            SerializableTimeSpan shorter = new(TimeSpan.FromMinutes(1));
            SerializableTimeSpan longer = new(TimeSpan.FromMinutes(5));

            Assert.That(shorter.CompareTo(longer), Is.LessThan(0));
            Assert.That(longer.CompareTo(shorter), Is.GreaterThan(0));
        }
    }
}