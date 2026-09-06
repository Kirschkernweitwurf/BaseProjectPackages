namespace Base.ToolsPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// What the date on an item is saying. Both readings are ordinary and neither can be told from
    /// the date itself: 20.08.26 is a deadline in one project and the day the note was written in the
    /// next, and which one it is decides whether a date in the past is a problem or just history.
    /// </summary>
    internal enum ETodoDateMeaning : byte
    {
        /// <summary>A deadline. Anything past it is overdue.</summary>
        Due = 0,

        /// <summary>The day the note was written. Old notes go stale rather than overdue.</summary>
        Written = 1
    }
}