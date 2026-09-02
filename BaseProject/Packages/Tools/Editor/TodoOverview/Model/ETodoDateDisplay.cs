namespace Base.ToolsPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// Which notation a date is shown in, whatever notation it was written in. A project that lets
    /// everyone write dates their own way still reads as one list this way, and a list sorted by date
    /// no longer looks unsorted because the rows above and below are counting different things.
    /// </summary>
    internal enum ETodoDateDisplay : byte
    {
        /// <summary>The first of the project's own date formats, which every machine agrees on.</summary>
        Project = 0,

        /// <summary>The short date of whatever region this machine is set to.</summary>
        Regional = 1
    }
}