using System;
using System.Collections.Generic;
using System.Threading;
using Base.UtilityPackage.Async;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Central dispatcher that resolves <see cref="GameAction"/>s and notifies reaction handlers
    /// around them.
    /// <para>
    /// Per action: the pre-window runs first (handlers may modify or cancel), then
    /// <see cref="GameAction.ExecuteAsync"/> applies the effect and plays its visuals if not
    /// canceled, then the post-window runs (handlers may trigger follow-up actions). The execute
    /// call is awaited in full, so a follow-up enqueued in the post-window only starts once the
    /// current action, including its animation, has finished.
    /// </para>
    /// <para>
    /// Resolution is depth-first. Actions performed from a reaction handler or from the synchronous
    /// start of <see cref="GameAction.ExecuteAsync"/> are children of the resolving action: they run
    /// right after it, in the order they were performed, before any sibling queued earlier. So a
    /// turn can queue "resolve card, drink, refill" up front and the card's own consequences still
    /// land before the drink. Anything performed outside that window (player input, after an await)
    /// joins the back of the queue.
    /// </para>
    /// <para>
    /// Plain C#, so a game can also run without a scene, like a headless simulation. Without
    /// presenters every action finishes at once and the whole chain resolves inside <see cref="Perform"/>.
    /// In the scene, <see cref="GameActionSystem"/> hosts one and ties it to the scene's lifetime.
    /// </para>
    /// </summary>
    public sealed class GameActionResolver : IGameActionSystem
    {
        /// <inheritdoc/>
        public event Action<GameAction, EReactionTiming> Observed;

        private readonly Dictionary<Type, List<Subscription>> _pre = new();
        private readonly Dictionary<Type, List<Subscription>> _on = new();
        private readonly Dictionary<Type, List<Subscription>> _post = new();
        private readonly LinkedList<GameAction> _queue = new();
        private readonly CancellationToken _lifetime;

        // Non-null only while the resolving action may spawn children; see the class remarks.
        private List<GameAction> _children;
        private bool _isProcessing;
        private long _sequence;

        /// <summary>Creates a resolver whose running actions cancel with <paramref name="lifetime"/>.</summary>
        public GameActionResolver(CancellationToken lifetime) => _lifetime = lifetime;

        /// <summary>
        /// Number of actions still waiting to be resolved.
        /// </summary>
        public int PendingCount => _queue.Count;

        /// <inheritdoc/>
        public bool IsBusy => _isProcessing;

        /// <inheritdoc/>
        public IDisposable Subscribe(Type actionType, EReactionTiming timing, Action<GameAction> handler,
            int priority = 0, bool once = false, Func<GameAction, bool> condition = null)
        {
            if (actionType == null)
            {
                CustomLogger.LogWarning("Attempted to subscribe with a null action type."
                    + " Subscription ignored.", null);

                return null;
            }

            if (!typeof(GameAction).IsAssignableFrom(actionType))
            {
                CustomLogger.LogWarning($"{actionType.Name} is not a {nameof(GameAction)}."
                    + " Subscription ignored.", null);

                return null;
            }

            if (handler == null)
            {
                CustomLogger.LogWarning("Attempted to subscribe with a null handler for"
                    + $" {actionType.Name} at {timing} timing. Subscription ignored.", null);

                return null;
            }

            Subscription subscription = new(this, actionType, timing, priority, _sequence++, once, handler, condition);

            Dictionary<Type, List<Subscription>> map = MapFor(timing);

            if (!map.TryGetValue(actionType, out List<Subscription> list))
            {
                list = new List<Subscription>();
                map[actionType] = list;
            }

            list.Add(subscription);

            return subscription;
        }

        /// <inheritdoc/>
        public IDisposable Subscribe<T>(EReactionTiming timing, Action<T> handler, int priority = 0,
            bool once = false, Func<T, bool> condition = null) where T : GameAction
        {
            if (handler == null)
            {
                CustomLogger.LogWarning("Attempted to subscribe with a null handler for"
                    + $" {typeof(T).Name} at {timing} timing. Subscription ignored.", null);

                return null;
            }

            return Subscribe(typeof(T),
                timing,
                handler: action => handler((T)action),
                priority,
                once,
                condition == null
                    ? null
                    : new Func<GameAction, bool>(action => condition((T)action)));
        }

        /// <inheritdoc/>
        public IDisposable SubscribePre<T>(Action<T> handler, int priority = 0, bool once = false,
            Func<T, bool> condition = null) where T : GameAction
            => Subscribe(EReactionTiming.Pre, handler, priority, once, condition);

        /// <inheritdoc/>
        public IDisposable SubscribePost<T>(Action<T> handler, int priority = 0, bool once = false,
            Func<T, bool> condition = null) where T : GameAction
            => Subscribe(EReactionTiming.Post, handler, priority, once, condition);

        /// <inheritdoc/>
        public void Perform(GameAction action)
        {
            if (action == null)
            {
                CustomLogger.LogWarning("Attempted to perform a null action. Ignored.", null);
                return;
            }

            if (_children != null)
            {
                _children.Add(action);
                return;
            }

            _queue.AddLast(action);

            // Re-entrant calls only enqueue; the running drain picks them up. This keeps resolution
            // linear and avoids deep recursion or overlapping drains in reaction chains.
            if (_isProcessing)
                return;

            DrainAsync().Forget();
        }

        /// <inheritdoc/>
        public IDisposable SubscribeOn<T>(Action<T> handler, int priority = 0, bool once = false,
            Func<T, bool> condition = null) where T : GameAction
            => Subscribe(EReactionTiming.On, handler, priority, once, condition);

        /// <summary>
        /// Removes a subscription from the system. This is called by <see cref="Subscription.Dispose"/>
        /// </summary>
        /// <param name="subscription">The subscription to remove. Must not be <c>null</c>.</param>
        public void Remove(Subscription subscription)
        {
            Dictionary<Type, List<Subscription>> map = MapFor(subscription.Timing);

            if (map.TryGetValue(subscription.ActionType, out List<Subscription> list))
                list.Remove(subscription);
        }

        /// <summary>
        /// Removes all subscriptions and clears the pending queue.
        /// </summary>
        public void Clear()
        {
            _pre.Clear();
            _on.Clear();
            _post.Clear();
            _queue.Clear();
            _children = null;
        }

        private static void Invoke(Subscription subscription, GameAction action)
        {
            try
            {
                subscription.Handler(action);
            }
            catch (Exception exception)
            {
                CustomLogger.LogError($"Reaction handler for {action.GetType().Name} threw: {exception}", null);
            }
        }

        private static int CompareByPriorityThenOrder(Subscription a, Subscription b)
        {
            int byPriority = b.Priority.CompareTo(a.Priority);
            return byPriority != 0
                ? byPriority
                : a.Order.CompareTo(b.Order);
        }

        private Dictionary<Type, List<Subscription>> MapFor(EReactionTiming timing) => timing switch
        {
            EReactionTiming.Pre => _pre,
            EReactionTiming.On => _on,
            _ => _post
        };

        /// <summary>
        /// Drains the queue one action at a time, awaiting each in full. A single drain runs at a
        /// time; re-entrant <see cref="Perform"/> calls only enqueue and are picked up by this loop.
        /// A failing action is logged and skipped, so one broken reaction can't stall the game.
        /// </summary>
        private async Awaitable DrainAsync()
        {
            _isProcessing = true;

            try
            {
                while (_queue.Count > 0)
                {
                    if (_lifetime.IsCancellationRequested)
                        return;

                    GameAction action = _queue.First.Value;
                    _queue.RemoveFirst();

                    try
                    {
                        await ResolveAsync(action);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        CustomLogger.LogError($"{action.GetType().Name} failed: {exception}", null);
                    }
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async Awaitable ResolveAsync(GameAction action)
        {
            List<GameAction> children = new();
            Awaitable execution = null;

            // The children were performed by reactions that already ran, e.g. a game over triggered by a hit.
            // They are scheduled even when the action itself throws, or the game would wait for them forever.
            try
            {
                _children = children;
                try
                {
                    Notify(action, EReactionTiming.Pre);

                    if (!action.IsCancelled)
                    {
                        Notify(action, EReactionTiming.On);
                        execution = action.ExecuteAsync(_lifetime);
                    }
                }
                finally
                {
                    _children = null;
                }

                if (action.IsCancelled)
                    action.OnCancelled();
                else
                {
                    if (execution != null)
                        await execution;

                    action.IsResolved = true;

                    _children = children;
                    try
                    {
                        Notify(action, EReactionTiming.Post);
                    }
                    finally
                    {
                        _children = null;
                    }
                }
            }
            finally
            {
                ScheduleNext(children);
            }
        }

        /// <summary>Puts the children at the front of the queue, keeping the order they were performed in.</summary>
        private void ScheduleNext(List<GameAction> children)
        {
            for (int index = children.Count - 1; index >= 0; index--)
                _queue.AddFirst(children[index]);
        }

        private void Notify(GameAction action, EReactionTiming timing)
        {
            Observed?.Invoke(action, timing);

            Dictionary<Type, List<Subscription>> map = MapFor(timing);

            // Walk the type hierarchy so a base-class subscription (e.g. GameAction) also matches
            // concrete actions. The copy is a snapshot, so handlers may subscribe or dispose during
            // iteration without breaking it.
            List<Subscription> matches = null;
            for (Type type = action.GetType(); type != null && typeof(GameAction).IsAssignableFrom(type);
                 type = type.BaseType)
            {
                if (!map.TryGetValue(type, out List<Subscription> registered)
                    || registered.Count == 0)
                    continue;

                matches ??= new List<Subscription>();

                matches.AddRange(registered);
            }

            if (matches == null)
                return;

            matches.Sort(CompareByPriorityThenOrder);

            foreach (Subscription subscription in matches)
            {
                if (!subscription.IsActive)
                    continue;

                if (subscription.Condition != null
                    && !subscription.Condition(action))
                    continue;

                Invoke(subscription, action);

                if (subscription.Once)
                    subscription.Dispose();

                if (timing == EReactionTiming.Pre
                    && action.IsCancelled)
                    break;
            }
        }
    }
}