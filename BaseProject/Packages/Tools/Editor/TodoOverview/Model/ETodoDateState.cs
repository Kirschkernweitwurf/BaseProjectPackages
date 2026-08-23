namespace Base.ToolPackage.Editor.TodoOverview.Model
{
    /// <summary>Where the date on an item sits relative to today, which is what colors its pill.</summary>
    internal enum ETodoDateState : byte
    {
        /// <summary>The item carries no date, or one that could not be read.</summary>
        None = 0,

        /// <summary>The date is still ahead.</summary>
        Future = 1,

        /// <summary>The date is today.</summary>
        Today = 2,

        /// <summary>The date has passed.</summary>
        Overdue = 3
    }
}