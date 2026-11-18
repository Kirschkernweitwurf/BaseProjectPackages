using System;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// A single subscription, handed out as an <see cref="IDisposable"/> so the
    /// caller can unsubscribe cleanly. Immutable once created; only the active
    /// flag changes, when the subscription is disposed.
    /// </summary>
    public class Subscription : IDisposable
    {
        /// <summary>
        /// False once the subscription has been disposed.
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// The type-erased handler invoked with the matching action.
        /// </summary>
        public readonly Action<GameAction> Handler;

        /// <summary>
        /// Optional filter; the handler only runs when it returns true.
        /// </summary>
        public readonly Func<GameAction, bool> Condition;

        /// <summary>
        /// The action type this subscription was registered for.
        /// </summary>
        public readonly Type ActionType;

        /// <summary>
        /// Whether the handler runs in the pre- or post-window.
        /// </summary>
        public readonly EReactionTiming Timing;

        /// <summary>
        /// Sort key; higher priority runs first.
        /// </summary>
        public readonly int Priority;

        /// <summary>
        /// Registration sequence, used as a stable tiebreak for equal priorities.
        /// </summary>
        public readonly long Order;

        /// <summary>
        /// If true, the subscription disposes itself after its first hit.
        /// </summary>
        public readonly bool Once;

        private GameActionResolver _system;

        public Subscription(GameActionResolver system, Type actionType, EReactionTiming timing, int priority,
            long order, bool once, Action<GameAction> handler, Func<GameAction, bool> condition)
        {
            _system = system;
            ActionType = actionType;
            Timing = timing;
            Priority = priority;
            Order = order;
            Once = once;
            Handler = handler;
            Condition = condition;
        }

        /// <summary>
        /// Ends the subscription and removes it from the system.
        /// </summary>
        public void Dispose()
        {
            if (!IsActive)
                return;

            IsActive = false;

            if (_system == null)
                return;

            _system.Remove(this);
            _system = null;
        }
    }
}