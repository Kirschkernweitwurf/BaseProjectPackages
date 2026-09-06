namespace Base.ToolsPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// How loudly an item's date is asking to be looked at, which is what colors its pill.
    /// <para>
    /// The ladder is the same for both readings of a date and only the steps move, so the list keeps
    /// one meaning for red whether the project writes deadlines or writes down when it wrote things.
    /// See <see cref="ETodoDateMeaning"/> for where each step sits.
    /// </para>
    /// </summary>
    internal enum ETodoDateState : byte
    {
        /// <summary>The item carries no date, or one that could not be read.</summary>
        None = 0,

        /// <summary>Due later, or written recently. Nothing to act on.</summary>
        Normal = 1,

        /// <summary>Due today, or old enough to be worth a look.</summary>
        Warning = 2,

        /// <summary>Past its deadline, or old enough to count as stale.</summary>
        Alert = 3
    }
}