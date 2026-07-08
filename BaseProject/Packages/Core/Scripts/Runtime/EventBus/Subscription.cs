using System;

namespace Base.CorePackage.EventBus
{
    /// <summary>
    /// RAII-style subscription token returned by <see cref="EventBus.Subscribe{TEvent}"/>
    /// (Resource Acquisition Is Initialization). Disposing it unsubscribes the associated handler from the owning bus.
    /// </summary>
    /// <typeparam name="TEvent">The event type the wrapped handler listens to.</typeparam>
    /// <remarks>
    /// Safe to dispose multiple times, subsequent calls are no-ops.
    /// </remarks>
    public sealed class Subscription<TEvent> : IDisposable where TEvent : IEvent
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