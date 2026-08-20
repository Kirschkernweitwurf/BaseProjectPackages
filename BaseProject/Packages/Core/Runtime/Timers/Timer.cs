using System;
using Base.UtilityPackage.Logging;
using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Base.CorePackage.Timers
{
    /// <summary>
    /// A reusable countdown timer driven by <see cref="TimerManager"/>.
    /// Supports looping, pausing, progress reporting, and completion callbacks.
    /// </summary>
    public sealed class Timer
    {
        /// <summary>Shortest allowed duration, so progress never divides by zero.</summary>
        private const float MinimumDuration = 0.0001f;

        /// <summary>Raised when the timer reaches zero, on every pass when looping.</summary>
        public event Action Completed;

        /// <summary>Raised every frame the timer runs, passing the remaining seconds.</summary>
        public event Action<float> Ticked;

        /// <summary>Remaining time in seconds.</summary>
        public float Remaining { get; private set; }

        /// <summary>Progress from 0 (start) to 1 (complete), useful for UI bars.</summary>
        public float Progress => Mathf.Clamp01(1f - Remaining / _duration);

        /// <summary>True while the timer is actively counting down.</summary>
        public bool IsRunning => _isRunning && !_isPaused;

        private readonly float _duration;
        private readonly bool _loop;

        private bool _isPaused;
        private bool _isRunning;

        /// <summary>Creates a timer. Duration is in seconds.</summary>
        public Timer(float duration, bool loop = false)
        {
            if (duration < MinimumDuration)
                CustomLogger.LogError($"{nameof(duration)} must be positive, got {duration}.", null);

            _duration = Mathf.Max(duration, MinimumDuration);
            _loop = loop;
            Remaining = _duration;
        }

        /// <summary>Creates, starts and returns a one-shot countdown in a single call.</summary>
        public static Timer Countdown(float seconds, Action onComplete)
        {
            if (onComplete == null)
                CustomLogger.LogWarning($"{nameof(onComplete)} is null, the countdown does nothing.", null);

            Timer timer = new(seconds);
            timer.Completed += onComplete;
            timer.Start();

            return timer;
        }

        /// <summary>Starts or restarts the timer from its full duration.</summary>
        public void Start()
        {
            Remaining = _duration;
            _isRunning = true;
            _isPaused = false;
            TimerManager.Register(this);
        }

        /// <summary>Pauses without losing the remaining time.</summary>
        public void Pause() => _isPaused = true;

        /// <summary>Resumes after a pause.</summary>
        public void Resume() => _isPaused = false;

        /// <summary>Stops the timer and removes it from updates.</summary>
        public void Stop()
        {
            _isRunning = false;
            _isPaused = false;
            TimerManager.Unregister(this);
        }

        internal void Tick(float deltaTime)
        {
            if (!IsRunning)
                return;

            Remaining -= deltaTime;
            Ticked?.Invoke(Remaining);

            if (Remaining > 0f)
                return;

            if (_loop)
            {
                // Carry the overshoot into the next pass so long-running loops do not drift.
                Remaining += _duration;
                Completed?.Invoke();
                return;
            }

            Remaining = 0f;

            // Stop before the callback, so a listener may restart the timer from inside it.
            Stop();
            Completed?.Invoke();
        }
    }
}