namespace Base.ToolsPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// The named groups a metadata pattern reports its findings in. The group name is the whole
    /// contract between the project's patterns and the scan: the notation around it is the pattern
    /// author's business, and which group a date lands in is how a pattern says what that date means.
    /// </summary>
    internal static class TodoGroupNames
    {
        /// <summary>A date whose meaning the project decides.</summary>
        internal const string Date = "date";

        /// <summary>A date that is a deadline, whatever the project's default reading is.</summary>
        internal const string Due = "due";

        /// <summary>The responsible person.</summary>
        internal const string Owner = "owner";

        /// <summary>A date that is when the note was written, whatever the project's default is.</summary>
        internal const string Written = "written";
    }
}