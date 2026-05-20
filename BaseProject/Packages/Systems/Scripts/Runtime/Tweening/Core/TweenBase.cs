using System;

namespace Base.SystemsCorePackage.Tweening.Core
{
    /// <summary>
    /// Base type for all tween instances. Provides lifecycle and completion events.
    /// </summary>
    /// <remarks>
    /// Event order:
    /// <list type="bullet">
    ///   <item><description>Natural finish: <c>OnComplete</c> → <c>OnKill</c></description></item>
    ///   <item><description><c>Stop(complete: true)</c>: snap to end, <c>OnComplete</c> → <c>OnKill</c></description></item>
    ///   <item><description><c>Stop(complete: false)</c>: <c>OnKill</c> only</description></item>
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

        public abstract bool IsRunning { get; }

        public abstract bool IsCompleted { get; }

        public abstract void Start();

        public abstract void Stop(bool complete = false);

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