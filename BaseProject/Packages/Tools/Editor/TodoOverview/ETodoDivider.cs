namespace Base.ToolPackage.Editor.TodoOverview
{
    /// <summary>The draggable dividers between the resizable columns of the list.</summary>
    internal enum ETodoDivider : byte
    {
        /// <summary>No divider is being dragged.</summary>
        None = 0,

        /// <summary>The divider between the keyword and the message column.</summary>
        KeywordMessage = 1,

        /// <summary>The divider between the message and the owner column.</summary>
        MessageOwner = 2,

        /// <summary>The divider between the owner and the date column.</summary>
        OwnerDate = 3,

        /// <summary>The divider between the date and the location column.</summary>
        DateLocation = 4
    }
}