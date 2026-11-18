using System.Threading;
using UnityEngine;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Base type for every discrete, reactable event in the game (playing a card,
    /// dealing damage, drawing and so on).
    /// <para>
    /// An action carries the intent and applies its own effect and visuals in
    /// <see cref="ExecuteAsync"/>. The action system awaits that call in full before the next
    /// queued action resolves, so a follow-up reaction chains only once this action, including
    /// its animation, has finished. Cross-cutting rules (triggers, buffs, shields, counters) live
    /// in reaction handlers and stay fully decoupled from the action.
    /// </para>
    /// </summary>
    public abstract class GameAction
    {
        /// <summary>
        /// True once a pre-reaction has canceled this action. Canceled actions never execute.
        /// Actions also cancel themselves when they are built with invalid input.
        /// </summary>
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// True once the effect has been applied, that is after <see cref="ExecuteAsync"/> has run.
        /// </summary>
        public bool IsResolved { get; internal set; }

        /// <summary>
        /// Prevents this action from resolving. Only meaningful during the
        /// <see cref="EReactionTiming.Pre"/> window; ignored afterward.
        /// </summary>
        public void Cancel() => IsCancelled = true;

        /// <summary>
        /// Applies the action's effect and plays its visuals. The action system awaits the returned
        /// operation before resolving the next action, so animations gate the reaction chain. Return
        /// <c>null</c> from a purely synchronous action that has already finished its work. Actions
        /// performed before the first await are children of this one and resolve right after it.
        /// </summary>
        /// <param name="token">Cancels when the action system is torn down mid-animation.</param>
        protected internal abstract Awaitable ExecuteAsync(CancellationToken token);

        /// <summary>
        /// Runs once when the action is dropped because it was cancelled, instead of executing. Override
        /// to undo preparation done before the action was performed (for example a card already lifted
        /// out of the hand), so a cancel never leaves the game in a half-done state.
        /// </summary>
        protected internal virtual void OnCancelled() { }
    }
}