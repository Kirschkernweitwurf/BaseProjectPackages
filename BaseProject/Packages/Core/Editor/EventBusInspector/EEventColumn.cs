namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// The columns of the event table that can be sorted by.
    /// </summary>
    /// <remarks>
    /// The table holds two kinds of row, so a column can mean something slightly different to each:
    /// sorting by <see cref="Event"/> orders the events by name and the subscribers under each one
    /// by the type that subscribed, while <see cref="State"/> orders the events by how many handlers
    /// they carry and the subscribers by condition. A column that only a subscriber row has says
    /// nothing about an event, so the events keep their name order in that case.
    /// </remarks>
    internal enum EEventColumn : byte
    {
        /// <summary>The event type, and the subscribing type on the rows below it.</summary>
        Event = 0,

        /// <summary>The subscribed method.</summary>
        Handler = 1,

        /// <summary>The handler count on an event, and the condition on a subscriber.</summary>
        State = 2,

        /// <summary>The object a handler runs on.</summary>
        Target = 3
    }
}