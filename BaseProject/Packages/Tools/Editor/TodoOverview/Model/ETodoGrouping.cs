namespace Base.ToolPackage.Editor.TodoOverview.Model
{
    /// <summary>What the list is split into sections by.</summary>
    internal enum ETodoGrouping : byte
    {
        /// <summary>One flat list without section headers.</summary>
        None = 0,

        /// <summary>One section per source file.</summary>
        File = 1,

        /// <summary>One section per keyword.</summary>
        Keyword = 2,

        /// <summary>One section per responsible person.</summary>
        Owner = 3
    }
}