namespace Base.AttributePackage
{
    /// <summary>
    /// Which halves of a point in time <see cref="DateAttribute"/> puts on screen. The half that is
    /// hidden keeps whatever it held, so narrowing the display never throws part of the value away.
    /// </summary>
    public enum EDateDisplay : byte
    {
        /// <summary>The date row and the time of day row underneath it.</summary>
        DateAndTime = 0,

        /// <summary>Only the year, month and day row.</summary>
        DateOnly = 1,

        /// <summary>Only the time of day row.</summary>
        TimeOnly = 2
    }
}