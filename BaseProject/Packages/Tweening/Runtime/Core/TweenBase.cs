using System;
using Base.ServicesPackage;
using Base.TweeningPackage.Core.Data;
using UnityEngine;

namespace Base.TweeningPackage.Core
{
    /// <summary>
    /// Base type for all tween instances. Provides lifecycle and completion events, and the options every tween
    /// shares: a delay, unscaled or scaled time, a target to kill it by and a GameObject it lives and dies with.
    /// Set the options through the fluent calls in <see cref="TweenOptions"/>.
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

        private GameObject _link;
        private bool _hasLink;
        private bool _hasFinishedNested;

        /// <summary>True between <see cref="Start"/> and the tween ending, however it ends.</summary>
        public abstract bool IsRunning { get; }

        /// <summary>
        /// True once the tween reached its end value. A tween stopped without completing ends without
        /// this ever becoming true, which is what separates a finish from a cancel.
        /// </summary>
        public abstract bool IsCompleted { get; }

        /// <summary>
        /// Seconds from the start to the end, delay and loops included. Infinite for endless loops. Zero for
        /// tweens that end when someone says so.
        /// </summary>
        public virtual float Duration => 0f;

        /// <summary>Seconds the tween waits after <see cref="Start"/> before it moves.</summary>
        public float Delay { get; protected set; }

        /// <summary>
        /// Whether the tween runs on unscaled time, so it keeps moving while the game is paused through
        /// <see cref="Time.timeScale"/>. True unless set otherwise, as tweens of this package always did.
        /// </summary>
        public bool IsUnscaled { get; private set; } = true;

        /// <summary>
        /// What the tween animates, so <see cref="Tweens.Kill"/> can find it. The tweened component by default.
        /// </summary>
        public object Target { get; protected set; }

        /// <summary>
        /// True while the tween runs inside a <see cref="TweenTimeline"/>, which drives it instead of the runner.
        /// </summary>
        internal bool IsNested { get; private set; }

        /// <summary>Whether the GameObject the tween is linked to was destroyed, so the tween must stop.</summary>
        internal bool IsLinkLost => _hasLink
            && _link == null;

        /// <summary>Whether the tweened object still exists, so a timeline may write to it.</summary>
        internal virtual bool IsTargetAlive => true;

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

        /// <summary>Waits <paramref name="delay"/> seconds after the start before moving.</summary>
        internal virtual void ApplyDelay(float delay) => Delay = Mathf.Max(0f, delay);

        /// <summary>Repeats the tween. Tweens without loops ignore it.</summary>
        internal virtual void ApplyLoops(int count, ELoopType type)
        {
        }

        /// <summary>Picks unscaled or scaled time.</summary>
        internal void ApplyUnscaled(bool isUnscaled) => IsUnscaled = isUnscaled;

        /// <summary>Sets what <see cref="Tweens.Kill"/> finds the tween by.</summary>
        internal void ApplyTarget(object target) => Target = target;

        /// <summary>Stops the tween once <paramref name="gameObject"/> is destroyed.</summary>
        internal void ApplyLink(GameObject gameObject)
        {
            _link = gameObject;
            _hasLink = gameObject != null;
        }

        /// <summary>
        /// Moves the tween to <paramref name="time"/> seconds after its start and applies the value there, forward or
        /// backward. Used by <see cref="TweenTimeline"/>; tweens that cannot seek ignore it.
        /// </summary>
        internal virtual void Goto(float time)
        {
        }

        /// <summary>
        /// Hands the tween to a timeline: the runner stops ticking it, and the timeline seeks it instead.
        /// </summary>
        internal void Nest()
        {
            Unregister();
            IsNested = true;
            OnNested();
        }

        /// <summary>Resets what the tween did on its own before a timeline took it over.</summary>
        internal virtual void OnNested()
        {
        }

        /// <summary>Raises the end events of a nested tween once its timeline played it to the end.</summary>
        internal void FinishNested()
        {
            if (_hasFinishedNested)
                return;

            _hasFinishedNested = true;
            InvokeComplete();
            InvokeKill();
        }

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

        /// <summary>Hands the tween to the runner, unless a timeline drives it.</summary>
        protected void Register()
        {
            if (IsNested)
                return;

            if (ServiceLocator.TryGet(out TweenRunner runner))
                runner.RegisterTween(this);
        }

        /// <summary>
        /// Takes the tween off the runner. Optional, since stopping also runs on shutdown, where the runner is gone
        /// already and a second error would arrive once per tween.
        /// </summary>
        protected void Unregister()
        {
            if (IsNested)
                return;

            if (ServiceLocator.TryGetOptional(out TweenRunner runner))
                runner.UnregisterTween(this);
        }
    }
}
