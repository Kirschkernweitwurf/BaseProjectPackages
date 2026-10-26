using System;
using Base.TweeningPackage.Core.Data;
using Base.UtilityPackage.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Core
{
    /// <summary>
    /// Generic tween that interpolates from a captured start value to a target value over time.
    /// <para>
    /// When a <c>fromGetter</c> is supplied the start value is captured once the configured delay has
    /// elapsed, so external motion or layout during the delay cannot make it stale. A literal
    /// <c>from</c> value is applied at <see cref="Start"/> instead and holds through the delay.
    /// </para>
    /// <para>
    /// The value is a function of the time since the start, so the tween can loop, play back and forth, and be
    /// moved to any point by a <see cref="TweenTimeline"/>. Ease and loops are set with
    /// <see cref="SetEase(EEasingType)"/> and <see cref="TweenOptions.SetLoops{T}"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The value type being tweened.</typeparam>
    public sealed class Tween<T> : TweenBase
    {
        private const int InfiniteLoops = -1;
        private const float MinDuration = 0.0001f;

        /// <summary>
        /// Optional initial value used if <see cref="_fromGetter"/> is not supplied.
        /// Applied at <see cref="Start"/> and held for the length of any delay.
        /// </summary>
        private readonly T _from;

        /// <summary>
        /// Optional function that returns the current value at the moment the tween begins moving,
        /// which is after any delay has elapsed. Read exactly once, and nothing is written before it.
        /// </summary>
        private readonly Func<T> _fromGetter;

        /// <summary>
        /// Target value the tween approaches by the end of its duration.
        /// </summary>
        private readonly T _to;

        /// <summary>
        /// Duration of one loop in seconds.
        /// </summary>
        private readonly float _duration;

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

        /// <summary>
        /// Whether the tween was made for a Unity object. Tweens of plain values have none and never stop for it.
        /// </summary>
        private readonly bool _hasTargetObj;

        /// <summary>
        /// Easing function that remaps the normalized time [0..1] to another value (may overshoot).
        /// If null, an identity mapping is used.
        /// </summary>
        private Func<float, float> _ease;

        private int _loops = 1;
        private ELoopType _loopType = ELoopType.Restart;
        private float _position;
        private bool _isRunning;
        private bool _isCompleted;
        private bool _hasCaptured;
        private T _capturedFromValue;

        /// <summary>
        /// Creates a tween with the specified configuration.
        /// All delegates must be non-null.
        /// </summary>
        public Tween(T to, float duration, Action<T> setter, Func<T, T, float, T> lerpFunc, Func<float, float> ease,
            Object targetObj, float delay = 0f, Func<T> fromGetter = null, T from = default)
        {
            _to = to;
            _duration = Mathf.Max(0f, duration);
            _setter = setter;
            _lerpFunc = lerpFunc;
            _ease = ease;
            _fromGetter = fromGetter;
            _from = from;
            _targetObj = targetObj;
            _hasTargetObj = targetObj != null;
            Delay = Mathf.Max(0f, delay);
            Target = targetObj;
        }

        /// <inheritdoc/>
        public override bool IsRunning => _isRunning;

        /// <inheritdoc/>
        public override bool IsCompleted => _isCompleted;

        /// <inheritdoc/>
        public override float Duration => _loops == InfiniteLoops
            ? float.PositiveInfinity
            : Delay + _duration * _loops;

        /// <inheritdoc/>
        internal override bool IsTargetAlive => !_hasTargetObj
            || _targetObj != null;

        private bool IsInfinite => _loops == InfiniteLoops;

        /// <summary>Shapes the motion with <paramref name="type"/>.</summary>
        public Tween<T> SetEase(EEasingType type)
        {
            _ease = Easings.Get(type);
            return this;
        }

        /// <summary>
        /// Shapes the motion with <paramref name="type"/>. <paramref name="overshoot"/> sets how far the back easings
        /// shoot past the end, 1.70158 by default; other easings ignore it.
        /// </summary>
        public Tween<T> SetEase(EEasingType type, float overshoot)
        {
            _ease = Easings.Get(type, overshoot);
            return this;
        }

        /// <summary>Shapes the motion with a custom curve that maps 0..1 to the progress.</summary>
        public Tween<T> SetEase(Func<float, float> ease)
        {
            _ease = ease;
            return this;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            if (_lerpFunc == null || _setter == null)
            {
                CustomLogger.LogWarning("Requires LerpFunc and Setter.", null);
                return;
            }

            _position = 0f;
            _isRunning = true;
            _isCompleted = false;

            // The start value is read at the moment movement begins. Without a delay that moment is
            // now. With a delay in front of it a getter has to wait, or it reads a value the delay then
            // makes stale. A literal from value cannot go stale, so it is applied now either way and
            // holds through the delay, which is what a fade in wants.
            _hasCaptured = false;

            if (_fromGetter == null || Delay <= 0f)
                CaptureFromValue();

            Register();
        }

        /// <inheritdoc/>
        public override void Stop(bool complete = false)
        {
            if (_isCompleted)
                return;

            _isRunning = false;
            _isCompleted = true;
            Unregister();

            if (complete && _setter != null)
            {
                if (!_hasCaptured)
                    CaptureFromValue();

                _setter(EndValue());
                InvokeComplete();
            }

            InvokeKill();
        }

        /// <inheritdoc/>
        public override void Tick(float deltaTime)
        {
            if (!_isRunning || _isCompleted)
                return;

            if (!IsTargetAlive)
            {
                CustomLogger.LogWarning("Tween target object was destroyed, stopping tween.", null);
                Stop();
                return;
            }

            MoveTo(_position + deltaTime);

            if (_position < Duration)
                return;

            _isCompleted = true;
            _isRunning = false;
            _setter(EndValue());

            // The TweenRunner detects IsCompleted after this Tick returns and removes the tween.
            InvokeComplete();
            InvokeKill();
        }

        /// <inheritdoc/>
        internal override void ApplyLoops(int count, ELoopType type)
        {
            _loops = count < 0
                ? InfiniteLoops
                : Mathf.Max(1, count);

            _loopType = type == ELoopType.None
                ? ELoopType.Restart
                : type;
        }

        /// <inheritdoc/>
        internal override void OnNested()
        {
            // A tween placed in a timeline starts there, so a start value read when it was made would be stale.
            _isRunning = false;
            _position = 0f;

            if (_fromGetter != null)
                _hasCaptured = false;
        }

        /// <inheritdoc/>
        internal override void Goto(float time)
        {
            if (_setter == null
                || _lerpFunc == null)
                return;

            _isRunning = true;
            MoveTo(time);
        }

        private void MoveTo(float time)
        {
            _position = time;

            if (time < Delay)
                return;

            if (!_hasCaptured)
                CaptureFromValue();

            _setter(ValueAt(time - Delay));
        }

        /// <summary>The value <paramref name="time"/> seconds after the delay, loops and direction included.</summary>
        private T ValueAt(float time)
        {
            int loop;
            float t;

            if (_duration <= MinDuration)
            {
                loop = IsInfinite
                    ? 0
                    : _loops - 1;

                t = 1f;
            }
            else
            {
                loop = Mathf.FloorToInt(time / _duration);
                t = (time - loop * _duration) / _duration;
            }

            if (!IsInfinite && loop >= _loops)
            {
                loop = _loops - 1;
                t = 1f;
            }

            bool isBackward = _loopType == ELoopType.PingPong
                && loop % 2 == 1;

            float progress = Ease(isBackward
                ? 1f - t
                : t);

            if (_loopType == ELoopType.Continue)
                return _lerpFunc(_capturedFromValue, _to, progress + loop);

            return progress switch
            {
                1f => _to,
                0f => _capturedFromValue,
                _ => _lerpFunc(_capturedFromValue, _to, progress)
            };
        }

        // Lands exactly on the end, not near it: the target for a forward finish, the start after an even number of
        // back and forth loops.
        private T EndValue()
        {
            if (IsInfinite)
                return _to;

            if (_loopType == ELoopType.Continue)
                return _lerpFunc(_capturedFromValue, _to, _loops);

            bool isBackAtStart = _loopType == ELoopType.PingPong
                && _loops % 2 == 0;

            return isBackAtStart
                ? _capturedFromValue
                : _to;
        }

        private float Ease(float t) => _ease?.Invoke(t) ?? t;

        /// <summary>Reads the start value and writes it, once, at the moment movement begins.</summary>
        private void CaptureFromValue()
        {
            _capturedFromValue = _fromGetter != null
                ? _fromGetter()
                : _from;

            _setter(_capturedFromValue);
            _hasCaptured = true;
        }
    }
}
