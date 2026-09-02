using Base.CorePackage.EventBus;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// The event the bus tests publish. A readonly struct, which is the shape the bus documents as
    /// the intended one.
    /// </summary>
    internal readonly struct ProbeEvent : IEvent
    {
        /// <summary>Identifies which publish a handler saw.</summary>
        internal int Value { get; }

        /// <summary>Creates an event carrying a value.</summary>
        /// <param name="value">The value handlers read back.</param>
        internal ProbeEvent(int value) => Value = value;
    }
}