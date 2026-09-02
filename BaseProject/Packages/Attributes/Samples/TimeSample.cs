using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A duration typed as hours, minutes and seconds.</summary>
    [AttributeSample(typeof(TimeAttribute), EAttributeCategory.Widgets,
        Description = "Draws a tick count as a signed row of day, hour, minute, second and millisecond "
            + "fields, so a duration is read and typed in the units it is thought about in.",
        Requirements = "The field has to be a long holding TimeSpan ticks, or a SerializableTimeSpan.",
        Info = "A unit that is switched off keeps what it held rather than being dropped, so a two day "
            + "cooldown drawn without the day field reads as forty eight hours.",
        Variations = new[]
        {
            "Time() for hours, minutes and seconds.",
            "Time(ShowDays = true) to put a day field in front.",
            "Time(ShowMilliseconds = true) to add a millisecond field after the seconds."
        })]
    internal sealed class TimeSample : ScriptableObject
    {
        [Time]
        [Tooltip("Type the parts separately; the sign button flips the whole duration.")]
        public long cooldown;

        [Time(ShowDays = true, ShowMilliseconds = true)]
        [Tooltip("Every unit from days down to milliseconds.")]
        public long recordingLength;
    }
}