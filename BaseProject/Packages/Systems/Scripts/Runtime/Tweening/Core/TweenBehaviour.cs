using System;
using Systems.Shutdown;
using Systems.Tweening.Core.Data;
using UnityEngine;

namespace Systems.Tweening.Core
{
    /// <summary>
    /// Generic base class providing tween lifecycle control, looping behavior, and default-value caching.
    /// </summary>
    public abstract class TweenBehaviour<T> : TweenBehaviourBase, IShutdownHandler
    {
        public override event Action OnFinished;

        [field: Tooltip("The settings for the tween.")]
        [field: SerializeField] public TweenSettings TweenSettings { get; private set; }

        [Tooltip("The settings for looping the tween, if needed.")]
        [SerializeField] private LoopSettings loopSettings;
        [Space]

        protected T DefaultValue;

        public bool HasShutDown { get; private set; }

        public override TweenBase ActiveTween => _activeTween;

        protected virtual void Awake() => DefaultValue = GetCurrentValue();

        protected virtual void Start() => ShutdownManager.Register(this);

        private TweenBase _activeTween;
        private int _currentLoop;

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

            TweenBase first = CreateTween(isReversed);
            StartTween(first);
        }

        public override void Stop()
        {
            if (_activeTween == null)
                return;

            _activeTween.OnComplete -= HandleTweenComplete;
            _activeTween.Stop();
            _activeTween = null;
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
                return;
            }

            _activeTween = tween;
            _activeTween.OnComplete += HandleTweenComplete;
            _activeTween.Start();
        }

        /// <summary>
        /// Called when the currently active tween instance completes.
        /// This method handles loop logic and only fires OnFinished when behaviour is truly done.
        /// </summary>
        private void HandleTweenComplete(TweenBase completedTween)
        {
            if (completedTween != null)
                completedTween.OnComplete -= HandleTweenComplete;

            if (loopSettings.LoopType == ELoopType.None
                || (loopSettings.LoopCount != -1 && _currentLoop >= loopSettings.LoopCount))
            {
                OnFinished?.Invoke();
                return;
            }

            _currentLoop++;

            switch (loopSettings.LoopType)
            {
                case ELoopType.Restart:
                    ApplyValue(DefaultValue);
                    StartTween(CreateTween(false));
                    break;

                case ELoopType.PingPong:
                    StartTween(CreateTween(true));
                    break;

                case ELoopType.Continue:
                    StartTween(CreateTween(false));
                    break;
            }
        }
    }
}