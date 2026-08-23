namespace Base.ToolPackage.Editor.TodoOverview.Model
{
    /// <summary>The order the items inside a section are listed in.</summary>
    internal enum ETodoSort : byte
    {
        /// <summary>By file path, then by line number.</summary>
        Location = 0,

        /// <summary>By keyword in the order the tags are configured in.</summary>
        Keyword = 1,

        /// <summary>By responsible person, unassigned items last.</summary>
        Owner = 2,

        /// <summary>By date, oldest first, undated items last.</summary>
        Date = 3,

        /// <summary>By the text of the item itself.</summary>
        Message = 4
    }
}