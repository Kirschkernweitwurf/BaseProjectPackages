using System;
using System.Collections.Generic;
using Base.ServicePackage;
using Base.UtilityPackage.Logging;

namespace Base.CorePackage.EventBus
{
    /// <summary>
    /// Default <see cref="IEventBus"/> implementation backed by multicast delegates.
    /// </summary>
    /// <remarks>
    /// Registers itself with the service locator through <see cref="GameServiceBehaviour"/> and drops every
    /// handler on destroy, so no subscription survives a scene unload.
    /// </remarks>
    public sealed class EventBus : GameServiceBehaviour, IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();
        private readonly Dictionary<Type, Delegate[]> _invocationListCache = new();

        /// <summary>
        /// The live handler table, keyed by event type. Each value is the multicast delegate every
        /// subscriber of that event was combined into.
        /// </summary>
        /// <remarks>
        /// A view onto the bus's own dictionary rather than a copy, so a tool reading it on a timer
        /// allocates nothing. Internal because it exists for the window in
        /// <c>Base.CorePackage.Editor</c> and nothing else.
        /// </remarks>
        internal IReadOnlyDictionary<Type, Delegate> Handlers => _handlers;

#region Unity Callbacks
        protected override void OnDestroy()
        {
            base.OnDestroy();

            Clear();
        }
#endregion

        /// <inheritdoc/>
        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            Type type = typeof(TEvent);

            if (handler == null)
            {
                CustomLogger.LogError($"Cannot subscribe a null handler for {type.Name}.", this);

                // An empty token keeps using blocks and Dispose calls on the caller side safe.
                return new Subscription<TEvent>(this, null);
            }

            _handlers[type] = _handlers.TryGetValue(type, out Delegate existing)
                ? Delegate.Combine(existing, handler)
                : handler;

            _invocationListCache.Remove(type);

            return new Subscription<TEvent>(this, handler);
        }

        /// <inheritdoc/>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            Type type = typeof(TEvent);

            if (handler == null)
            {
                CustomLogger.LogError($"Cannot unsubscribe a null handler for {type.Name}.", this);
                return;
            }

            if (!_handlers.TryGetValue(type, out Delegate existing))
                return;

            Delegate remaining = Delegate.Remove(existing, handler);
            if (remaining == null)
                _handlers.Remove(type);
            else
                _handlers[type] = remaining;

            _invocationListCache.Remove(type);
        }

        /// <inheritdoc/>
        public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            Type type = typeof(TEvent);
            if (!_handlers.TryGetValue(type, out Delegate combined))
                return;

            // GetInvocationList allocates a new array on every call, so the result is cached here.
            // Subscribe, Unsubscribe and Clear invalidate that cache.
            if (!_invocationListCache.TryGetValue(type, out Delegate[] invocations))
            {
                invocations = combined.GetInvocationList();
                _invocationListCache[type] = invocations;
            }

            // Iterating the local array keeps this dispatch intact while handlers (un)subscribe.
            foreach (Delegate invocation in invocations)
            {
                try
                {
                    ((Action<TEvent>)invocation).Invoke(@event);
                }
                catch (Exception exception)
                {
                    CustomLogger.LogError($"A handler for {type.Name} threw an exception: {exception}", this);
                }
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _handlers.Clear();
            _invocationListCache.Clear();
        }
    }
}