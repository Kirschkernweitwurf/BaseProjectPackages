using System;
using Base.ServicesPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Base MonoBehaviour that reacts to a single, inspector-chosen game action. Resolves
    /// the action type through <see cref="GameActionRegistry"/>, subscribes to the
    /// <see cref="IGameActionSystem"/> while enabled and forwards each hit to
    /// <see cref="React"/>. Subclasses decide what the reaction actually does.
    /// </summary>
    public abstract class GameActionReactor : MonoBehaviour
    {
        [Tooltip("The game action this component reacts to.")]
        [SerializeField] private GameActionReference action;

        [Tooltip("When it reacts: Pre before the action and its rules, On as it runs, Post once it is done.")]
        [SerializeField] private EReactionTiming timing = EReactionTiming.Post;

        private IGameActionSystem _system;
        private IDisposable _subscription;

#region Unity Callbacks
        protected virtual void OnEnable()
        {
            _system = ResolveActionSystem();

            if (_system == null)
            {
                CustomLogger.LogWarning($"No {nameof(IGameActionSystem)} available."
                    + " Reactor stays inactive.", this);

                return;
            }

            if (!action.TryResolve(out Type actionType))
            {
                CustomLogger.LogWarning($"Game action '{action.Id}' could not be resolved."
                    + " Reactor stays inactive.", this);

                return;
            }

            _subscription = _system.Subscribe(actionType, timing, React);
        }

        protected virtual void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
#endregion

        /// <summary>
        /// Resolves the action system the reactor subscribes to. Override to plug in a
        /// custom lookup; the default pulls it from the <see cref="ServiceLocator"/>.
        /// </summary>
        /// <returns>The action system or <c>null</c> when none is available.</returns>
        protected virtual IGameActionSystem ResolveActionSystem() => ServiceLocator.Get<IGameActionSystem>();

        /// <summary>
        /// Runs whenever the chosen action fires at the configured timing.
        /// </summary>
        /// <param name="gameAction">The action instance that triggered the reaction.</param>
        protected abstract void React(GameAction gameAction);
    }
}