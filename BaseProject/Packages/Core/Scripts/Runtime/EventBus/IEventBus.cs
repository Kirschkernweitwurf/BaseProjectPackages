using System;

namespace Base.CorePackage.EventBus
{
    /// <summary>
    /// Strongly-typed, in-process publish/subscribe bus.
    /// Subscribers receive every event of the type they registered for, in subscription order.
    /// </summary>
    /// <remarks>
    /// Not thread-safe. Intended for use on Unity's main thread.
    /// Register a single instance in your ServiceLocator and resolve it where needed.
    /// </remarks>
    public interface IEventBus
    {
        /// <summary>
        /// Registers <paramref name="handler"/> to receive events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The event type to listen for.</typeparam>
        /// <param name="handler">The callback invoked on every <see cref="Publish{TEvent}"/>.</param>
        /// <returns>A disposable token that unsubscribes the handler when disposed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <c>null</c>.</exception>
        IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;

        /// <summary>
        /// Removes a previously registered <paramref name="handler"/>.
        /// Has no effect if the handler was never subscribed.
        /// </summary>
        /// <typeparam name="TEvent">The event type the handler was registered for.</typeparam>
        /// <param name="handler">The exact delegate instance previously passed to <see cref="Subscribe{TEvent}"/>.</param>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;

        /// <summary>
        /// Synchronously invokes every handler registered for <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The event type being dispatched (usually inferred from the argument).</typeparam>
        /// <param name="event">The event payload.</param>
        /// <remarks>
        /// Exceptions thrown by a handler are caught and logged, so a single faulty handler
        /// cannot prevent the remaining handlers from running.
        /// Handlers may (un)subscribe during dispatch without affecting the current invocation.
        /// </remarks>
        void Publish<TEvent>(TEvent @event) where TEvent : IEvent;

        /// <summary>
        /// Removes all handlers for every event type. Useful on scene unload or shutdown.
        /// </summary>
        void Clear();
    }
}