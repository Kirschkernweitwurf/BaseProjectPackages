using System;
using Base.SystemsCorePackage.Services.Shutdown;
using Base.SystemsCorePackage.Tweening.Core.Data;
using UnityEngine;

namespace Base.SystemsCorePackage.Tweening.Core
{
    /// <summary>
    /// Generic base class providing tween lifecycle control, looping behavior, and default-value caching.
    /// </summary>
    public abstract class TweenBehaviour<T> : TweenBehaviourBase, IShutdownHandler
    {
        public override event Action OnFinished;

        public override event Action OnKilled;

        [field: Tooltip("The settings for the tween.")]
        [field: SerializeField] public TweenSettings TweenSettings { get; private set; }

        [Tooltip("The settings for looping the tween, if needed.")]
        [SerializeField] private LoopSettings loopSettings;
        [Space]

        protected T DefaultValue;

        public bool HasShutDown { get; private set; }

        public override TweenBase ActiveTween => _activeTween;

        private TweenBase _activeTween;
        private int _currentLoop;
        private bool _currentReversed;

        protected virtual void Awake() => DefaultValue = GetCurrentValue();

        protected virtual void Start() => ShutdownManager.Register(this);

        protected virtual void OnDestroy()
        {
            if (!HasShutDown)
                Shutdown();
        }

        public void Shutdown()
        {
            ShutdownManager.Deregister(this);

            Stop();
            HasShutDown = true;
        }

        public override void Play(bool isReversed)
        {
            Stop();

            _currentLoop = 0;
            _currentReversed = isReversed;

            TweenBase first = CreateTween(isReversed);
            StartTween(first);
        }

        public override void Stop(bool complete = false)
        {
            if (_activeTween == null)
                return;

            TweenBase tween = _activeTween;
            _activeTween = null;

            // Detach our handler so the tween's own Stop doesn't reroute back into loop logic.
            tween.OnComplete -= HandleTweenComplete;
            tween.Stop(complete);

            if (complete)
                OnFinished?.Invoke();

            OnKilled?.Invoke();
        }

        /// <summary>
        /// Sets the delay of the tween to the specified value.
        /// </summary>
        public void SetDelay(float delay) => TweenSettings.SetDelay(delay);

        /// <summary>
        /// Sets the duration of the tween to the specified value.
        /// </summary>
        public void SetDuration(float duration) => TweenSettings.SetDuration(duration);

        /// <summary>
        /// Returns the current value of the property being tweened.
        /// </summary>
        protected abstract T GetCurrentValue();

        /// <summary>
        /// Applies the given value to the property being tweened.
        /// </summary>
        /// <param name="value">The value to apply.</param>
        protected abstract void ApplyValue(T value);

        /// <summary>
        /// Implementations must create a tween. If <c>isReversed==true</c> they should swap from/to
        /// and make use of <see cref="DefaultValue"/> for the original start value.
        /// </summary>
        protected abstract TweenBase CreateTween(bool isReversed);

        /// <summary>
        /// Centralized start for a tween instance (subscribes and starts).
        /// </summary>
        private void StartTween(TweenBase tween)
        {
            if (tween == null)
            {
                OnFinished?.Invoke();
                OnKilled?.Invoke();
                return;
            }

            _activeTween = tween;
            _activeTween.OnComplete += HandleTweenComplete;
            _activeTween.Start();
        }

        /// <summary>
        /// Called when the currently active tween instance completes naturally.
        /// This method handles loop logic and only fires OnFinished when the behaviour is truly done.
        /// </summary>
        private void HandleTweenComplete(TweenBase completedTween)
        {
            completedTween.OnComplete -= HandleTweenComplete;
            _activeTween = null;

            bool hasLoopBudget = loopSettings.LoopCount == -1 || _currentLoop < loopSettings.LoopCount;

            if (loopSettings.LoopType == ELoopType.None || !hasLoopBudget)
            {
                OnFinished?.Invoke();
                OnKilled?.Invoke();
                return;
            }

            _currentLoop++;

            switch (loopSettings.LoopType)
            {
                case ELoopType.Restart:
                    ApplyValue(DefaultValue);
                    StartTween(CreateTween(_currentReversed));
                    break;

                case ELoopType.PingPong:
                    _currentReversed = !_currentReversed;
                    StartTween(CreateTween(_currentReversed));
                    break;

                case ELoopType.Continue:
                    StartTween(CreateTween(_currentReversed));
                    break;
            }
        }
    }
}