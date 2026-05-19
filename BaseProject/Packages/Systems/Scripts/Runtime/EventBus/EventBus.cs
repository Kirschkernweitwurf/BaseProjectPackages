using System;
using System.Collections.Generic;
using Base.SystemsCorePackage.Services;
using UnityEngine;

namespace Base.SystemsCorePackage.EventBus
{
    /// <summary>
    /// Default <see cref="IEventBus"/> implementation backed by multicast delegates.
    /// </summary>
    public sealed partial class EventBus : GameServiceBehaviour, IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        /// <inheritdoc/>
        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type type = typeof(TEvent);
            _handlers[type] = _handlers.TryGetValue(type, out Delegate existing)
                ? Delegate.Combine(existing, handler)
                : handler;

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
        }

        /// <inheritdoc/>
        public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out Delegate del))
                return;

            if (del is not Action<TEvent> typed)
                return;

            foreach (Delegate invocation in typed.GetInvocationList())
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
        public void Clear() => _handlers.Clear();
    }
}