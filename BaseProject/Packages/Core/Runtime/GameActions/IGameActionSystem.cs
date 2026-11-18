using System;
using Base.ServicesPackage;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Public contract of the action/reaction system. Consumers (cards, effects,
    /// UI) depend on this abstraction rather than the concrete class, which keeps
    /// the code decoupled and easy to test.
    /// </summary>
    public interface IGameActionSystem : IGameService
    {
        /// <summary>
        /// Raised for every action and every timing, even with no subscribers. This is the plain
        /// event channel: use it to observe that an action happened without joining the pre- / post-
        /// windows or the reaction chain (logging, analytics, replay recording).
        /// </summary>
        event Action<GameAction, EReactionTiming> Observed;

        /// <summary>
        /// Whether an action is resolving right now, animations included. Stays true until the queue has run dry, so
        /// an action and every follow-up it chains count as one busy stretch.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Submits an action for resolution. Actions resolve asynchronously and in order: each
        /// action's <see cref="GameAction.ExecuteAsync"/>, including its animation, is awaited in
        /// full before the next queued action starts. A follow-up enqueued from a post-reaction
        /// therefore chains only after the current action has finished. If called while another
        /// action is resolving, the new action is queued rather than resolved recursively and the
        /// call returns immediately. Called from a reaction handler or the synchronous start of an
        /// execute, the new action is a child and resolves right after the current one, before
        /// anything queued earlier (depth-first).
        /// </summary>
        void Perform(GameAction action);

        /// <summary>
        /// Subscribes to reactions on actions of the given runtime type, including
        /// derived types. Use this overload when the action type is only known at
        /// runtime (for example resolved from an inspector reference); it avoids the
        /// reflection that a generic call would otherwise need.
        /// </summary>
        /// <param name="actionType">
        /// An action type or a base class. Use <c>typeof(GameAction)</c> to react to
        /// everything. Must be assignable to <see cref="GameAction"/>.
        /// </param>
        /// <param name="timing">Pre (before), On (as it starts) or Post (after).</param>
        /// <param name="handler">Invoked with the matching action.</param>
        /// <param name="priority">
        /// Higher priority runs first. Equal priorities run in subscription order.
        /// </param>
        /// <param name="once">If true, the subscription ends after its first hit.</param>
        /// <param name="condition">
        /// Optional filter; the handler only runs when it returns true.
        /// </param>
        /// <returns>An <see cref="IDisposable"/>; calling Dispose ends the subscription.</returns>
        IDisposable Subscribe(Type actionType,
            EReactionTiming timing,
            Action<GameAction> handler,
            int priority = 0,
            bool once = false,
            Func<GameAction, bool> condition = null);

        /// <summary>
        /// Subscribes to reactions on actions of type <typeparamref name="T"/>,
        /// including derived types.
        /// </summary>
        /// <typeparam name="T">
        /// An action type or a base class. Use <see cref="GameAction"/> to react
        /// to everything.
        /// </typeparam>
        /// <param name="timing">Pre (before), On (as it starts) or Post (after).</param>
        /// <param name="handler">Invoked with the matching action.</param>
        /// <param name="priority">
        /// Higher priority runs first. Equal priorities run in subscription order.
        /// </param>
        /// <param name="once">If true, the subscription ends after its first hit.</param>
        /// <param name="condition">
        /// Optional filter; the handler only runs when it returns true.
        /// </param>
        /// <returns>An <see cref="IDisposable"/>; calling Dispose ends the subscription.</returns>
        IDisposable Subscribe<T>(EReactionTiming timing,
            Action<T> handler,
            int priority = 0,
            bool once = false,
            Func<T, bool> condition = null) where T : GameAction;

        /// <summary>
        /// Shorthand for <see cref="Subscribe{T}"/> with
        /// <see cref="EReactionTiming.Pre"/>.
        /// </summary>
        IDisposable SubscribePre<T>(Action<T> handler, int priority = 0, bool once = false,
            Func<T, bool> condition = null) where T : GameAction;

        /// <summary>
        /// Shorthand for <see cref="Subscribe{T}"/> with
        /// <see cref="EReactionTiming.On"/>.
        /// </summary>
        IDisposable SubscribeOn<T>(Action<T> handler, int priority = 0, bool once = false,
            Func<T, bool> condition = null) where T : GameAction;

        /// <summary>
        /// Shorthand for <see cref="Subscribe{T}"/> with
        /// <see cref="EReactionTiming.Post"/>.
        /// </summary>
        IDisposable SubscribePost<T>(Action<T> handler, int priority = 0, bool once = false,
            Func<T, bool> condition = null) where T : GameAction;
    }
}