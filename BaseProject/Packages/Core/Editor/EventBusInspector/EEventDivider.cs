namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// The draggable lines between the resizable columns of the event table.
    /// </summary>
    internal enum EEventDivider : byte
    {
        /// <summary>The line between the Handler and Target columns.</summary>
        HandlerTarget = 0,

        /// <summary>No line is currently being dragged.</summary>
        None = 1,

        /// <summary>The line between the Event and Handler columns.</summary>
        SubscriberHandler = 2
    }
}