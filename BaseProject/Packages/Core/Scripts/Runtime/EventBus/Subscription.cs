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
        // Stored as the concrete type on purpose: Unity's overloaded == only applies to UnityEngine.Object
        // references, so a destroyed bus is recognized as null here.
        private EventBus _bus;
        private Action<TEvent> _handler;

        /// <summary>
        /// Creates a token that removes <paramref name="handler"/> from <paramref name="bus"/> on dispose.
        /// </summary>
        /// <param name="bus">The bus the handler is registered with.</param>
        /// <param name="handler">The handler to remove, or <c>null</c> for an empty token that does nothing.</param>
        internal Subscription(EventBus bus, Action<TEvent> handler)
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
            if (_handler == null)
                return;

            // A destroyed bus already dropped all its handlers, so only the local references are cleared.
            if (_bus != null)
                _bus.Unsubscribe(_handler);

            _bus = null;
            _handler = null;
        }
    }
}