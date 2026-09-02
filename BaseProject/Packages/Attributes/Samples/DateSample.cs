using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A point in time picked from a calendar.</summary>
    [AttributeSample(typeof(DateAttribute), EAttributeCategory.Widgets,
        Description = "Draws a tick count as a year, month and day row with a calendar picker, and "
            + "optionally a time of day row under it.",
        Requirements = "The field has to be a long holding DateTime ticks, or a SerializableDateTime.",
        Info = "The day clamps against the month that is selected, so moving from March to February "
            + "pulls the 31st back to the 28th instead of leaving a date that does not exist.",
        Variations = new[]
        {
            "Date() for the date row on its own.",
            "Date(EDateDisplay.DateAndTime) to add the time of day underneath.",
            "Date(EDateDisplay.TimeOnly) for the time of day on its own.",
            "ShowMilliseconds = true to add a millisecond field to the time row."
        })]
    internal sealed class DateSample : ScriptableObject
    {
        [Date]
        [Tooltip("Pick a day from the calendar button at the right.")]
        public long eventStart;

        [Date(EDateDisplay.DateAndTime)]
        [Tooltip("The Now button stamps the current date and time.")]
        public long lastBuilt;
    }
}