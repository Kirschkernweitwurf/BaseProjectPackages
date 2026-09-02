using System;
using Base.UtilityPackage.Serialization;
using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the guarantee that keeps an inspector edit from throwing: whatever tick count ends up in
    /// the field, reading it back yields a date instead of an out of range exception.
    /// </summary>
    public sealed class SerializableDateTimeTests
    {
        /// <summary>A date comes back as the same date.</summary>
        [Test]
        public void ADateSurvivesTheRoundTrip()
        {
            DateTime original = new(2024, 5, 17, 13, 45, 30);
            SerializableDateTime stored = new(original);

            Assert.That(stored.Value, Is.EqualTo(original));
            Assert.That(stored.Ticks, Is.EqualTo(original.Ticks));
        }

        /// <summary>The kind is deliberately not carried, so the value reads as unspecified.</summary>
        [Test]
        public void TheValueCarriesNoTimeZone()
        {
            SerializableDateTime stored = new(DateTime.UtcNow);

            Assert.That(stored.Value.Kind, Is.EqualTo(DateTimeKind.Unspecified));
        }

        /// <summary>A tick count above the range clamps instead of throwing.</summary>
        [Test]
        public void ATickCountAboveTheRangeClamps()
        {
            SerializableDateTime stored = new(long.MaxValue);

            Assert.That(stored.Ticks, Is.EqualTo(DateTime.MaxValue.Ticks));
        }

        /// <summary>A tick count below the range clamps instead of throwing.</summary>
        [Test]
        public void ATickCountBelowTheRangeClamps()
        {
            SerializableDateTime stored = new(long.MinValue);

            Assert.That(stored.Ticks, Is.EqualTo(DateTime.MinValue.Ticks));
        }

        /// <summary>The shared clamp is reachable for code that holds the ticks itself.</summary>
        [Test]
        public void TheSharedClampAcceptsAnyTickCount()
        {
            Assert.That(SerializableDateTime.ToDateTime(long.MinValue), Is.EqualTo(DateTime.MinValue));
            Assert.That(SerializableDateTime.ToDateTime(long.MaxValue), Is.EqualTo(DateTime.MaxValue));
        }

        /// <summary>An untouched field is the same default a plain date has.</summary>
        [Test]
        public void AnUntouchedValueIsTheMinimumDate()
            => Assert.That(default(SerializableDateTime).Value, Is.EqualTo(DateTime.MinValue));

        /// <summary>The conversions in both directions have to line up.</summary>
        [Test]
        public void TheImplicitConversionsMatch()
        {
            DateTime original = new(2024, 1, 1);
            SerializableDateTime stored = original;
            DateTime unwrapped = stored;

            Assert.That(unwrapped, Is.EqualTo(original));
        }

        /// <summary>Two equal dates compare equal through every path.</summary>
        [Test]
        public void EqualDatesCompareEqual()
        {
            SerializableDateTime first = new(new DateTime(2024, 1, 1));
            SerializableDateTime second = new(new DateTime(2024, 1, 1));

            Assert.That(second, Is.EqualTo(first));
            Assert.That(first == second, Is.True);
            Assert.That(first != second, Is.False);
            Assert.That(second.GetHashCode(), Is.EqualTo(first.GetHashCode()));
            Assert.That(first.CompareTo(second), Is.EqualTo(0));
        }

        /// <summary>Ordering follows the point in time the values describe.</summary>
        [Test]
        public void EarlierDatesOrderFirst()
        {
            SerializableDateTime earlier = new(new DateTime(2023, 1, 1));
            SerializableDateTime later = new(new DateTime(2024, 1, 1));

            Assert.That(earlier.CompareTo(later), Is.LessThan(0));
            Assert.That(later.CompareTo(earlier), Is.GreaterThan(0));
        }
    }
}