using System;
using Systems.Services;
using UnityEngine;
using Utility.Logging;
using Object = UnityEngine.Object;

namespace Systems.Tweening.Core
{
    /// <summary>
    /// Generic tween that interpolates from a captured start value to a target value over time.
    /// The starting value is lazily captured after any configured delay has elapsed to avoid
    /// staleness caused by external motion/layout during the delay period.
    /// </summary>
    /// <typeparam name="T">The value type being tweened.</typeparam>
    public sealed class Tween<T> : TweenBase
    {
        /// <summary>
        /// Optional initial value used if <see cref="_fromGetter"/> is not supplied.
        /// This value is not applied until the tween actually starts after any delay.
        /// </summary>
        private readonly T _from;

        /// <summary>
        /// Optional function that returns the current value at the moment the tween begins moving.
        /// If provided, this is used to capture the true starting value exactly once after the delay.
        /// </summary>
        private readonly Func<T> _fromGetter;

        /// <summary>
        /// Target value the tween approaches by the end of its duration.
        /// </summary>
        private readonly T _to;

        /// <summary>
        /// Total duration of the tween in seconds.
        /// </summary>
        private readonly float _duration;

        /// <summary>
        /// Delay before the tween starts moving, in seconds.
        /// </summary>
        private readonly float _delay;

        /// <summary>
        /// Easing function that remaps the normalized time [0..1] to another value (may overshoot).
        /// If null, an identity mapping is used.
        /// </summary>
        private readonly Func<float, float> _ease;

        /// <summary>
        /// Interpolation function that maps (from, to, t) → value. Should be an unclamped Lerp.
        /// </summary>
        private readonly Func<T, T, float, T> _lerpFunc;

        /// <summary>
        /// Setter invoked every tick with the interpolated value.
        /// </summary>
        private readonly Action<T> _setter;

        /// <summary>
        /// Reference to the target Unity object used to determine validity of the tween.
        /// </summary>
        private readonly Object _targetObj;

        private float _elapsedTime;
        private float _delayTimer;
        private bool _isRunning;
        private bool _isCompleted;
        private bool _hasStarted;
        private T _capturedFromValue;

        public override bool IsRunning => _isRunning;

        public override bool IsCompleted => _isCompleted;

        /// <summary>
        /// Creates a tween with the specified configuration.
        /// All delegates must be non-null.
        /// </summary>
        public Tween(T to, float duration, Action<T> setter, Func<T, T, float, T> lerpFunc, Func<float, float> ease,
            Object targetObj, float delay = 0f, Func<T> fromGetter = null, T from = default)
        {
            _to = to;
            _duration = duration;
            _setter = setter;
            _lerpFunc = lerpFunc;
            _ease = ease;
            _delay = delay;
            _fromGetter = fromGetter;
            _from = from;
            _targetObj = targetObj;
        }

        public override void Start()
        {
            if (_lerpFunc == null || _setter == null)
            {
                CustomLogger.LogWarning("Requires LerpFunc and Setter.", null);
                return;
            }

            _elapsedTime = 0f;
            _delayTimer = 0f;
            _isRunning = true;
            _isCompleted = false;
            _hasStarted = false;

            _capturedFromValue = _fromGetter != null ? _fromGetter() : _from;
            _setter(_capturedFromValue);

            if (ServiceLocator.TryGet(out TweenRunner runner))
                runner.RegisterTween(this);
        }

        public override void Stop()
        {
            _isRunning = false;
            _isCompleted = true;

            if (ServiceLocator.TryGet(out TweenRunner runner))
                runner.UnregisterTween(this);
        }

        public override void Tick(float deltaTime)
        {
            if (!_isRunning || _isCompleted)
                return;

            if (_targetObj == null)
            {
                CustomLogger.LogWarning("Tween target object was destroyed, stopping tween.", null);
                Stop();
                return;
            }

            if (!_hasStarted)
            {
                if (_delayTimer < _delay)
                {
                    _delayTimer += deltaTime;
                    return;
                }

                _hasStarted = true;
            }

            _elapsedTime += deltaTime;

            float denom = Mathf.Max(_duration, 0.0001f);
            float t01 = Mathf.Clamp01(_elapsedTime / denom);
            float eased = _ease?.Invoke(t01) ?? t01;

            T value = _lerpFunc(_capturedFromValue, _to, eased);
            _setter(value);

            if (_elapsedTime < _duration)
                return;

            _isCompleted = true;
            _isRunning = false;
            _setter(_to);
            Complete();
        }
    }
}