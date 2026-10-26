using System;
using System.Collections.Generic;
using Base.TweeningPackage.Core.Data;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.TweeningPackage.Core
{
    /// <summary>
    /// Places tweens and callbacks on a timeline: one after another (<see cref="Append"/>), side by side
    /// (<see cref="Join"/>) or at a fixed time (<see cref="Insert"/>). The timeline plays them as one tween, so it can
    /// loop, play back and forth and be stopped or completed as a whole. Create one with <see cref="Tweens.Timeline"/>.
    /// </summary>
    /// <remarks>
    /// A tween added to a timeline belongs to it: the runner no longer ticks it, and its start value is read when the
    /// timeline reaches it, not when it was made. Set a tween's options (delay, ease, loops) before adding it, since
    /// its length is measured when it is placed. Endless loops have no length and cannot be placed.
    /// </remarks>
    public sealed class TweenTimeline : TweenBase
    {
        private const int InfiniteLoops = -1;
        private const float MinLength = 0.0001f;

        private readonly List<Entry> _entries = new();

        private float _length;
        private float _lastStart;
        private float _position;
        private float _lastLocalTime;
        private int _lastLoop;
        private int _loops = 1;
        private ELoopType _loopType = ELoopType.Restart;
        private bool _isRunning;
        private bool _isCompleted;

        /// <inheritdoc/>
        public override bool IsRunning => _isRunning;

        /// <inheritdoc/>
        public override bool IsCompleted => _isCompleted;

        /// <inheritdoc/>
        public override float Duration => _loops == InfiniteLoops
            ? float.PositiveInfinity
            : Delay + _length * _loops;

        private bool IsInfinite => _loops == InfiniteLoops;

        /// <summary>Places <paramref name="tween"/> after everything placed so far.</summary>
        public TweenTimeline Append(TweenBase tween)
        {
            float start = _length;
            if (Place(start, tween))
                _lastStart = start;

            return this;
        }

        /// <summary>Places <paramref name="tween"/> at the start of the tween appended or inserted last, so both run
        /// together.</summary>
        public TweenTimeline Join(TweenBase tween)
        {
            Place(_lastStart, tween);
            return this;
        }

        /// <summary>Places <paramref name="tween"/> <paramref name="time"/> seconds after the timeline's
        /// start.</summary>
        public TweenTimeline Insert(float time, TweenBase tween)
        {
            float start = Mathf.Max(0f, time);
            if (Place(start, tween))
                _lastStart = start;

            return this;
        }

        /// <summary>Places <paramref name="tween"/> before everything placed so far, which moves back by its
        /// length.</summary>
        public TweenTimeline Prepend(TweenBase tween)
        {
            if (!CanPlace(tween))
                return this;

            Shift(tween.Duration);
            Place(0f, tween);
            return this;
        }

        /// <summary>Adds <paramref name="seconds"/> of nothing at the end.</summary>
        public TweenTimeline AppendInterval(float seconds)
        {
            _length += Mathf.Max(0f, seconds);
            return this;
        }

        /// <summary>Adds <paramref name="seconds"/> of nothing at the start, which moves everything back.</summary>
        public TweenTimeline PrependInterval(float seconds)
        {
            Shift(Mathf.Max(0f, seconds));
            return this;
        }

        /// <summary>Calls <paramref name="callback"/> once the timeline reaches its current end.</summary>
        public TweenTimeline AppendCallback(Action callback) => InsertCallback(_length, callback);

        /// <summary>Calls <paramref name="callback"/> <paramref name="time"/> seconds after the timeline's
        /// start.</summary>
        public TweenTimeline InsertCallback(float time, Action callback)
        {
            if (callback == null)
            {
                CustomLogger.LogWarning("Tried to place a null callback on a timeline. Ignoring it.", null);
                return this;
            }

            float start = Mathf.Max(0f, time);
            _entries.Add(new Entry(start, null, callback));
            _length = Mathf.Max(_length, start);
            return this;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            _position = 0f;
            _lastLocalTime = -1f;
            _lastLoop = 0;
            _isRunning = true;
            _isCompleted = false;
            Register();
        }

        /// <inheritdoc/>
        public override void Stop(bool complete = false)
        {
            if (_isCompleted)
                return;

            if (complete)
                MoveTo(IsInfinite
                    ? Delay + _length
                    : Duration);

            _isRunning = false;
            _isCompleted = true;
            Unregister();

            if (complete)
            {
                FinishChildren();
                InvokeComplete();
            }

            InvokeKill();
        }

        /// <inheritdoc/>
        public override void Tick(float deltaTime)
        {
            if (!_isRunning || _isCompleted)
                return;

            MoveTo(_position + deltaTime);

            if (_position < Duration)
                return;

            _isRunning = false;
            _isCompleted = true;
            FinishChildren();

            // The TweenRunner detects IsCompleted after this Tick returns and removes the timeline.
            InvokeComplete();
            InvokeKill();
        }

        /// <inheritdoc/>
        internal override void ApplyLoops(int count, ELoopType type)
        {
            _loops = count < 0
                ? InfiniteLoops
                : Mathf.Max(1, count);

            _loopType = type == ELoopType.PingPong
                ? ELoopType.PingPong
                : ELoopType.Restart;
        }

        /// <inheritdoc/>
        internal override void Goto(float time)
        {
            _isRunning = true;
            MoveTo(time);
        }

        private static bool CanPlace(TweenBase tween)
        {
            if (tween == null)
            {
                CustomLogger.LogWarning("Tried to place a null tween on a timeline. Ignoring it.", null);
                return false;
            }

            if (!float.IsInfinity(tween.Duration))
                return true;

            CustomLogger.LogWarning("A tween with endless loops has no end, so it cannot be placed on a timeline. "
                + "Loop the timeline instead.", null);
            return false;
        }

        private bool Place(float start, TweenBase tween)
        {
            if (!CanPlace(tween))
                return false;

            tween.Nest();
            _entries.Add(new Entry(start, tween, null));
            _length = Mathf.Max(_length, start + tween.Duration);
            return true;
        }

        private void Shift(float seconds)
        {
            foreach (Entry entry in _entries)
                entry.Start += seconds;

            _length += seconds;
            _lastStart += seconds;
        }

        private void MoveTo(float time)
        {
            _position = time;

            if (time < Delay)
                return;

            float local = time - Delay;
            int loop = 0;
            float localTime = local;

            if (_length > MinLength)
            {
                loop = Mathf.FloorToInt(local / _length);
                localTime = local - loop * _length;
            }

            if (!IsInfinite && loop >= _loops)
            {
                loop = _loops - 1;
                localTime = _length;
            }

            // A new loop that restarts runs its callbacks again. Back and forth only fires them on the way forth.
            if (loop != _lastLoop)
            {
                FinishPass(_lastLoop);
                _lastLoop = loop;
                _lastLocalTime = -1f;
                ResetCallbacks();
            }

            bool isBackward = _loopType == ELoopType.PingPong
                && loop % 2 == 1;

            float at = isBackward
                ? _length - localTime
                : localTime;

            Seek(at, isForward: !isBackward);
            _lastLocalTime = localTime;
        }

        // Before leaving a loop, the pass must reach its end, or a big frame step could skip its last values.
        private void FinishPass(int loop)
        {
            bool wasBackward = _loopType == ELoopType.PingPong
                && loop % 2 == 1;

            Seek(wasBackward
                ? 0f
                : _length, isForward: !wasBackward);
        }

        private void Seek(float at, bool isForward)
        {
            foreach (Entry entry in _entries)
            {
                if (entry.Tween == null)
                {
                    if (isForward
                        && !entry.HasFired
                        && at >= entry.Start)
                    {
                        entry.HasFired = true;
                        entry.Callback();
                    }

                    continue;
                }

                if (!entry.Tween.IsTargetAlive)
                    continue;

                float childTime = at - entry.Start;

                // Not reached yet: a tween that never ran keeps its hands off the value.
                if (childTime < 0f && !entry.HasStarted)
                    continue;

                entry.HasStarted = true;
                entry.Tween.Goto(Mathf.Clamp(childTime, 0f, entry.Tween.Duration));

                // A placed tween reports its end once, the first time the timeline plays past it.
                if (isForward
                    && childTime >= entry.Tween.Duration)
                    entry.Tween.FinishNested();
            }
        }

        private void ResetCallbacks()
        {
            foreach (Entry entry in _entries)
            {
                if (entry.Tween == null)
                    entry.HasFired = false;
            }
        }

        private void FinishChildren()
        {
            foreach (Entry entry in _entries)
            {
                if (entry.Tween != null)
                    entry.Tween.FinishNested();
            }
        }

        /// <summary>One tween or callback on the timeline.</summary>
        private sealed class Entry
        {
            /// <summary>Places a tween or a callback at <paramref name="start"/>.</summary>
            public Entry(float start, TweenBase tween, Action callback)
            {
                Start = start;
                Tween = tween;
                Callback = callback;
            }

            /// <summary>Seconds after the timeline's start.</summary>
            public float Start { get; set; }

            /// <summary>The tween, or null for a callback.</summary>
            public TweenBase Tween { get; }

            /// <summary>The callback, or null for a tween.</summary>
            public Action Callback { get; }

            /// <summary>Whether the callback ran in the current loop.</summary>
            public bool HasFired { get; set; }

            /// <summary>Whether the timeline reached the tween at least once.</summary>
            public bool HasStarted { get; set; }
        }
    }
}
