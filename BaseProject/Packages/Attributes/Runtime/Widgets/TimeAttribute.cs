using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a duration as a signed row of day, hour, minute, second and millisecond fields. For the
    /// lengths of time a designer types rather than measures: a cooldown, a round timer, a respawn
    /// delay.
    /// </summary>
    /// <remarks>
    /// A duration, not a point in time. Use <see cref="DateAttribute"/> for the latter.
    /// <para>
    /// The field is a tick count. On a <c>long</c> the attribute is what says the number is a duration
    /// at all; on a <c>SerializableTimeSpan</c> the type already says it and the attribute only picks
    /// which units are shown. A unit that is switched off keeps what it held rather than being
    /// dropped, so a two day cooldown drawn without the day field reads as forty eight hours.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TimeAttribute : PropertyAttribute
    {
        /// <summary>True to put a day field in front of the hours.</summary>
        public bool ShowDays { get; set; }

        /// <summary>True to add a millisecond field after the seconds.</summary>
        public bool ShowMilliseconds { get; set; }
    }
}