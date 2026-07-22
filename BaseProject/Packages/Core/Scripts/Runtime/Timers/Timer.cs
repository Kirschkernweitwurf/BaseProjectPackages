using System;
using Base.UtilityPackage.Logging;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
namespace Base.CorePackage.Timers
{
    /// <summary>
    /// A reusable countdown timer driven by <see cref="TimerManager"/>.
    /// Supports looping, pausing, progress reporting, and completion callbacks.
    /// </summary>
    public class Timer
    {
        /// <summary>Raised once when the timer reaches zero.</summary>
        public event Action Completed;

        /// <summary>Raised every frame the timer runs, passing the remaining seconds.</summary>
        public event Action<float> Ticked;

        /// <summary>Remaining time in seconds.</summary>
        public float Remaining { get; private set; }

        /// <summary>Progress from 0 (start) to 1 (complete), useful for UI bars.</summary>
        public float Progress => 1f - Remaining / Mathf.Max(_duration, Mathf.Epsilon);

        /// <summary>True while the timer is actively counting down.</summary>
        public bool IsRunning => _isRunning && !_isPaused;

        private readonly float _duration;
        private readonly bool _loop;

        private bool _isPaused;
        private bool _isRunning;

        /// <summary>Creates a timer. Duration is in seconds.</summary>
        public Timer(float duration, bool loop = false)
        {
            if (duration <= 0f)
                CustomLogger.LogError($"Timer duration must be positive, got {duration}.", null);

            _duration = duration;
            _loop = loop;
            Remaining = _duration;
        }

        /// <summary>Creates, starts and returns a one-shot countdown in a single call.</summary>
        public static Timer Countdown(float seconds, Action onComplete)
        {
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

            Completed?.Invoke();

            if (_loop)
            {
                Remaining = _duration;
                return;
            }

            Stop();
        }
    }
}