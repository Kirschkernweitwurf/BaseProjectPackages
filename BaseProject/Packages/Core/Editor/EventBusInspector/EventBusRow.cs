namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// One visible line of the table, either an event header or one subscriber under it.
    /// </summary>
    /// <remarks>
    /// The table is a tree, but every part of the window that is not drawing wants a flat list: the
    /// selection is one index, the arrow keys move by one line, and the row a click landed on is
    /// found by counting. Flattening once per rebuild is what lets all of that stay simple.
    /// </remarks>
    internal readonly struct EventBusRow
    {
        /// <summary>The event this line belongs to. Never null.</summary>
        internal EventTypeEntry Event { get; }

        /// <summary>The subscriber this line shows, or null when the line is the event header.</summary>
        internal HandlerEntry Handler { get; }

        /// <summary>True when this line is the expandable header of an event.</summary>
        internal bool IsHeader => Handler == null;

        /// <summary>Creates a line of the table.</summary>
        /// <param name="eventEntry">The event the line belongs to.</param>
        /// <param name="handler">The subscriber, or null for the event header itself.</param>
        internal EventBusRow(EventTypeEntry eventEntry, HandlerEntry handler)
        {
            Event = eventEntry;
            Handler = handler;
        }
    }
}