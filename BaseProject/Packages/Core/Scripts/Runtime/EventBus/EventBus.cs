using System;
using System.Collections.Generic;
using Base.CorePackage.Services;
using UnityEngine;

namespace Base.CorePackage.EventBus
{
    /// <summary>
    /// Default <see cref="IEventBus"/> implementation backed by multicast delegates.
    /// </summary>
    public sealed class EventBus : GameServiceBehaviour, IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();
        private readonly Dictionary<Type, Delegate[]> _invocationListCache = new();

        /// <inheritdoc/>
        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type type = typeof(TEvent);
            _handlers[type] = _handlers.TryGetValue(type, out Delegate existing)
                ? Delegate.Combine(existing, handler)
                : handler;

            _invocationListCache.Remove(type);

            return new Subscription<TEvent>(this, handler);
        }

        /// <inheritdoc/>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            if (handler == null)
                return;

            Type type = typeof(TEvent);
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
            if (!_handlers.TryGetValue(type, out Delegate del))
                return;

            if (del is not Action<TEvent> typed)
                return;

            // Invocation lists are cached to avoid the per-publish array allocation
            // of GetInvocationList. The cache is invalidated on (un)subscribe.
            if (!_invocationListCache.TryGetValue(type, out Delegate[] invocations))
            {
                invocations = typed.GetInvocationList();
                _invocationListCache[type] = invocations;
            }

            foreach (Delegate invocation in invocations)
            {
                try
                {
                    ((Action<TEvent>)invocation).Invoke(@event);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
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