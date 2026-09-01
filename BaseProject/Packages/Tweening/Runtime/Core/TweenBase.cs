using System;

namespace Base.TweeningPackage.Core
{
    /// <summary>
    /// Base type for all tween instances. Provides lifecycle and completion events.
    /// </summary>
    /// <remarks>
    /// Event order:
    /// <list type="bullet">
    /// <item><description>Natural finish: <c>OnComplete</c> → <c>OnKill</c></description></item>
    /// <item>
    /// <description>
    /// <c>Stop(complete: true)</c>: snap to end,
    /// <c>OnComplete</c> → <c>OnKill</c>
    /// </description>
    /// </item>
    /// <item><description><c>Stop(complete: false)</c>: <c>OnKill</c> only</description></item>
    /// </list>
    /// </remarks>
    public abstract class TweenBase : ITween
    {
        /// <summary>
        /// Event invoked when the tween completes (either naturally or via <c>Stop(complete: true)</c>).
        /// </summary>
        public event Action<TweenBase> OnComplete;

        /// <summary>
        /// Event invoked when the tween ends for any reason (natural finish, manual stop, or kill).
        /// Always fires after <c>OnComplete</c> when both apply.
        /// </summary>
        public event Action<TweenBase> OnKill;

        /// <summary>True between <see cref="Start"/> and the tween ending, however it ends.</summary>
        public abstract bool IsRunning { get; }

        /// <summary>
        /// True once the tween reached its end value. A tween stopped without completing ends without
        /// this ever becoming true, which is what separates a finish from a cancel.
        /// </summary>
        public abstract bool IsCompleted { get; }

        /// <summary>Begins the tween. Calling this on a running tween does nothing.</summary>
        public abstract void Start();

        /// <summary>Ends the tween early.</summary>
        /// <param name="complete">
        /// True to snap to the end value first, which raises the completion event as if it had
        /// finished. False to leave the value where it is and only raise the kill event.
        /// </param>
        public abstract void Stop(bool complete = false);

        /// <summary>Advances the tween by one frame. Called by whatever owns the tween.</summary>
        /// <param name="deltaTime">Seconds since the last tick, scaled or unscaled as the owner decides.</param>
        public abstract void Tick(float deltaTime);

        /// <summary>
        /// Invokes the completion event. Should be called by derived classes when the tween
        /// reaches its end value.
        /// </summary>
        protected void InvokeComplete() => OnComplete?.Invoke(this);

        /// <summary>
        /// Invokes the kill event. Should be called by derived classes when the tween ends
        /// for any reason, after <c>InvokeComplete</c> when both apply.
        /// </summary>
        protected void InvokeKill() => OnKill?.Invoke(this);
    }
}