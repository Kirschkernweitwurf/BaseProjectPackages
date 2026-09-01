using System;
using UnityEngine;

namespace Base.UtilityPackage.Serialization
{
    /// <summary>
    /// A <see cref="DateTime"/> Unity can serialize. Unity refuses every type declared in the core
    /// library, so the value is kept as its tick count and rebuilt on access. The inspector draws it as
    /// a year, month and day row with a calendar picker plus a time of day row.
    /// </summary>
    /// <remarks>
    /// The kind is deliberately not stored. A date typed into an inspector has no time zone behind it,
    /// and carrying a <see cref="DateTimeKind"/> next to it would make <see cref="Value"/> look
    /// authoritative about something nobody was ever asked. Convert where it actually matters.
    /// <para>
    /// A field that was never touched holds tick zero, which is <see cref="DateTime.MinValue"/> and
    /// reads as year one in the inspector. That is the same default a plain <see cref="DateTime"/> has;
    /// use the Now button or the calendar to set a real one.
    /// </para>
    /// </remarks>
    [Serializable]
    public struct SerializableDateTime : IComparable<SerializableDateTime>, IEquatable<SerializableDateTime>
    {
        private const string RoundTripFormat = "O";

        /// <summary>Name of the serialized tick field. Used by the inspector drawer.</summary>
        public const string TicksField = nameof(ticks);

        [SerializeField] private long ticks;

        /// <summary>The tick count, clamped to the range <see cref="DateTime"/> accepts.</summary>
        public long Ticks => Clamp(ticks);

        /// <summary>The value as a <see cref="DateTime"/> of kind <see cref="DateTimeKind.Unspecified"/>.</summary>
        public DateTime Value => new(Clamp(ticks));

        /// <summary>Creates a value from a date and time.</summary>
        /// <param name="value">The date and time to store.</param>
        public SerializableDateTime(DateTime value) => ticks = value.Ticks;

        /// <summary>Creates a value from a tick count.</summary>
        /// <param name="ticks">The tick count to store.</param>
        public SerializableDateTime(long ticks) => this.ticks = Clamp(ticks);

        /// <summary>Unwraps the stored value.</summary>
        /// <param name="value">The value to unwrap.</param>
        /// <returns>The date and time it holds.</returns>
        public static implicit operator DateTime(SerializableDateTime value) => value.Value;

        /// <summary>Wraps a date and time so it can be serialized.</summary>
        /// <param name="value">The date and time to wrap.</param>
        /// <returns>The wrapped value.</returns>
        public static implicit operator SerializableDateTime(DateTime value) => new(value);

        /// <summary>Compares two values for equality.</summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>True when both describe the same point in time.</returns>
        public static bool operator ==(SerializableDateTime left, SerializableDateTime right) => left.Equals(right);

        /// <summary>Compares two values for inequality.</summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>True when they describe different points in time.</returns>
        public static bool operator !=(SerializableDateTime left, SerializableDateTime right) => !left.Equals(right);

        /// <summary>
        /// Rebuilds a <see cref="DateTime"/> from a raw tick count, clamped to the range it accepts.
        /// </summary>
        /// <remarks>
        /// Exposed because the inspector drawers and any code storing the ticks as a plain
        /// <see cref="long"/> need the same clamp, and an out of range tick count throws otherwise.
        /// </remarks>
        /// <param name="ticks">The tick count to rebuild from.</param>
        /// <returns>The date and time the ticks describe.</returns>
        public static DateTime ToDateTime(long ticks) => new(Clamp(ticks));

        /// <summary>Orders two values by the point in time they describe.</summary>
        /// <param name="other">The value to compare against.</param>
        /// <returns>A negative number, zero or a positive number.</returns>
        public int CompareTo(SerializableDateTime other) => Ticks.CompareTo(other.Ticks);

        /// <summary>Determines whether the given value describes the same point in time.</summary>
        /// <param name="other">The value to compare against.</param>
        /// <returns>True when both hold the same tick count.</returns>
        public bool Equals(SerializableDateTime other) => Ticks == other.Ticks;

        /// <summary>Determines whether the given object is an equal value.</summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>True when it is a value describing the same point in time.</returns>
        public override bool Equals(object obj) => obj is SerializableDateTime other && Equals(other);

        /// <summary>Returns a hash code for the stored value.</summary>
        /// <returns>The hash code of the tick count.</returns>
        public override int GetHashCode() => Ticks.GetHashCode();

        /// <summary>Returns the value in the round trip format, for logs and inspectors.</summary>
        /// <returns>The formatted date and time.</returns>
        public override string ToString() => Value.ToString(RoundTripFormat);

        private static long Clamp(long ticks) => Math.Clamp(ticks, DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);
    }
}