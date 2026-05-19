using System;

namespace Base.SystemsCorePackage.EventBus
{
    /// <summary>
    /// Internal helper class for managing event subscriptions. Not intended for external use.
    /// It encapsulates the logic for unsubscribing handlers when the returned token is disposed.
    /// </summary>
    /// <remarks>
    /// This class is a part of <see cref="EventBus"/> moved here for organizational clarity.
    /// It is defined as a nested class to encapsulate its functionality and prevent external access.
    /// </remarks>
    public sealed partial class EventBus
    {
        /// <summary>
        /// RAII-style subscription token returned by <see cref="EventBus.Subscribe{TEvent}"/>.
        /// Disposing it unsubscribes the associated handler from the owning bus.
        /// </summary>
        /// <typeparam name="TEvent">The event type the wrapped handler listens to.</typeparam>
        /// <remarks>
        /// Safe to dispose multiple times, subsequent calls are no-ops.
        /// </remarks>
        private sealed class Subscription<TEvent> : IDisposable where TEvent : IEvent
        {
            private EventBus _bus;
            private Action<TEvent> _handler;

            public Subscription(EventBus bus, Action<TEvent> handler)
            {
                _bus = bus;
                _handler = handler;
            }

            /// <summary>
            /// Unsubscribes the wrapped handler from the owning bus.
            /// </summary>
            /// <remarks>
            /// This method is idempotent, calling this more than once has no additional effect.
            /// After disposal the internal references are cleared to GC.
            /// </remarks>
            public void Dispose()
            {
                if (_bus == null)
                    return;

                _bus.Unsubscribe(_handler);
                _bus = null;
                _handler = null;
            }
        }
    }
}