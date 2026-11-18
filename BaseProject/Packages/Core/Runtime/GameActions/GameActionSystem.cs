using System;
using Base.ServicesPackage;
using UnityEngine;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Scene host of the <see cref="GameActionResolver"/>: registers it as the <see cref="IGameActionSystem"/>
    /// service on Awake, so consumers resolve it through the <see cref="ServiceLocator"/> and stay decoupled from
    /// the scene and cancels its running actions when the scene unloads. The rules of resolution live in the
    /// resolver, so a game without a scene resolves the same way.
    /// </summary>
    public sealed class GameActionSystem : MonoBehaviour, IGameActionSystem
    {
        private GameActionResolver _resolver;

        /// <inheritdoc/>
        public event Action<GameAction, EReactionTiming> Observed
        {
            add => Resolver.Observed += value;
            remove => Resolver.Observed -= value;
        }

        /// <summary>
        /// Number of actions still waiting to be resolved.
        /// </summary>
        public int PendingCount => Resolver.PendingCount;

        /// <inheritdoc/>
        public bool IsBusy => Resolver.IsBusy;

        // Created on first use, since tests add the component without running Awake.
        private GameActionResolver Resolver => _resolver ??= new GameActionResolver(destroyCancellationToken);

#region Unity Callbacks
        private void Awake() => ServiceLocator.Register<IGameActionSystem>(this);

        private void OnDestroy() => ServiceLocator.Deregister<IGameActionSystem>(this);
#endregion

        /// <inheritdoc/>
        public void Perform(GameAction action) => Resolver.Perform(action);

        /// <inheritdoc/>
        public IDisposable Subscribe(Type actionType, EReactionTiming timing, Action<GameAction> handler,
            int priority = 0, bool once = false, Func<GameAction, bool> condition = null)
            => Resolver.Subscribe(actionType, timing, handler, priority, once, condition);

        /// <inheritdoc/>
        public IDisposable Subscribe<T>(EReactionTiming timing, Action<T> handler, int priority = 0,
            bool once = false, Func<T, bool> condition = null) where T : GameAction
            => Resolver.Subscribe(timing, handler, priority, once, condition);

        /// <inheritdoc/>
        public IDisposable SubscribePre<T>(Action<T> handler, int priority = 0, bool once = false,
            Func<T, bool> condition = null) where T : GameAction
            => Resolver.SubscribePre(handler, priority, once, condition);

        /// <inheritdoc/>
        public IDisposable SubscribeOn<T>(Action<T> handler, int priority = 0, bool once = false,
            Func<T, bool> condition = null) where T : GameAction
            => Resolver.SubscribeOn(handler, priority, once, condition);

        /// <inheritdoc/>
        public IDisposable SubscribePost<T>(Action<T> handler, int priority = 0, bool once = false,
            Func<T, bool> condition = null) where T : GameAction
            => Resolver.SubscribePost(handler, priority, once, condition);

        /// <summary>
        /// Removes all subscriptions and clears the pending queue.
        /// </summary>
        public void Clear() => Resolver.Clear();
    }
}