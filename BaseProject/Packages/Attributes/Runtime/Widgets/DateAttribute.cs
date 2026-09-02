using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Draws a point in time as a year, month and day row with a calendar picker, optionally followed
    /// by a time of day row. For dates that are authored rather than measured: an event start, a build
    /// stamp, a seasonal window.
    /// </summary>
    /// <remarks>
    /// The field is a tick count. On a <c>long</c> the attribute is what says the number is a date at
    /// all, which is why it names the meaning rather than the layout. On a
    /// <c>SerializableDateTime</c> the type already says it, and the attribute only narrows what the
    /// two rows show.
    /// <para>
    /// Ticks and not seconds, so a date survives the round trip to disk unchanged. A float of seconds
    /// stops being able to name a single second once the value gets far enough from zero, which is
    /// exactly where calendar dates live.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DateAttribute : PropertyAttribute
    {
        /// <summary>Which halves of the value are drawn.</summary>
        public EDateDisplay Display { get; }

        /// <summary>True to add a millisecond field to the time row.</summary>
        public bool ShowMilliseconds { get; set; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="display">Which halves of the value are drawn.</param>
        public DateAttribute(EDateDisplay display = EDateDisplay.DateOnly) => Display = display;
    }
}