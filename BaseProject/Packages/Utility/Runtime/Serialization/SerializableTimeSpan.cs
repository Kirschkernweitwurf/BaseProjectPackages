using System;
using UnityEngine;

namespace Base.UtilityPackage.Serialization
{
    /// <summary>
    /// A <see cref="TimeSpan"/> Unity can serialize. Unity refuses every type declared in the core
    /// library, so the duration is kept as its tick count and rebuilt on access. The inspector draws it
    /// as a row of day, hour, minute, second and millisecond fields.
    /// </summary>
    /// <remarks>
    /// A tick count rather than a float of seconds, so a duration authored as one hour is exactly one
    /// hour on the way back out. A float loses that as soon as the value gets large enough, which is
    /// what makes long timers drift.
    /// </remarks>
    [Serializable]
    public struct SerializableTimeSpan : IComparable<SerializableTimeSpan>, IEquatable<SerializableTimeSpan>
    {
        /// <summary>Name of the serialized tick field. Used by the inspector drawer.</summary>
        public const string TicksField = nameof(ticks);

        [SerializeField] private long ticks;

        /// <summary>The tick count of the duration. Negative for a duration that runs backwards.</summary>
        public long Ticks => ticks;

        /// <summary>The duration as a <see cref="TimeSpan"/>.</summary>
        public TimeSpan Value => new(ticks);

        /// <summary>The duration in seconds, for the many Unity APIs that take one.</summary>
        public float Seconds => (float)Value.TotalSeconds;

        /// <summary>Creates a value from a duration.</summary>
        /// <param name="value">The duration to store.</param>
        public SerializableTimeSpan(TimeSpan value) => ticks = value.Ticks;

        /// <summary>Creates a value from a tick count.</summary>
        /// <param name="ticks">The tick count to store.</param>
        public SerializableTimeSpan(long ticks) => this.ticks = ticks;

        /// <summary>Unwraps the stored duration.</summary>
        /// <param name="value">The value to unwrap.</param>
        /// <returns>The duration it holds.</returns>
        public static implicit operator TimeSpan(SerializableTimeSpan value) => value.Value;

        /// <summary>Wraps a duration so it can be serialized.</summary>
        /// <param name="value">The duration to wrap.</param>
        /// <returns>The wrapped value.</returns>
        public static implicit operator SerializableTimeSpan(TimeSpan value) => new(value);

        /// <summary>Compares two durations for equality.</summary>
        /// <param name="left">The first duration.</param>
        /// <param name="right">The second duration.</param>
        /// <returns>True when both are the same length.</returns>
        public static bool operator ==(SerializableTimeSpan left, SerializableTimeSpan right) => left.Equals(right);

        /// <summary>Compares two durations for inequality.</summary>
        /// <param name="left">The first duration.</param>
        /// <param name="right">The second duration.</param>
        /// <returns>True when they are different lengths.</returns>
        public static bool operator !=(SerializableTimeSpan left, SerializableTimeSpan right) => !left.Equals(right);

        /// <summary>Orders two durations by length.</summary>
        /// <param name="other">The duration to compare against.</param>
        /// <returns>A negative number, zero or a positive number.</returns>
        public int CompareTo(SerializableTimeSpan other) => ticks.CompareTo(other.ticks);

        /// <summary>Determines whether the given duration is the same length.</summary>
        /// <param name="other">The duration to compare against.</param>
        /// <returns>True when both hold the same tick count.</returns>
        public bool Equals(SerializableTimeSpan other) => ticks == other.ticks;

        /// <summary>Determines whether the given object is an equal duration.</summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>True when it is a duration of the same length.</returns>
        public override bool Equals(object obj) => obj is SerializableTimeSpan other && Equals(other);

        /// <summary>Returns a hash code for the stored duration.</summary>
        /// <returns>The hash code of the tick count.</returns>
        public override int GetHashCode() => ticks.GetHashCode();

        /// <summary>Returns the duration in the constant format, for logs and inspectors.</summary>
        /// <returns>The formatted duration.</returns>
        public override string ToString() => Value.ToString();
    }
}