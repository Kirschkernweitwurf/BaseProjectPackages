using System;
using Base.UtilityPackage.Logging;

// ReSharper disable MemberCanBePrivate.Global
namespace Base.SystemsCorePackage.Timers
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
        public float Progress => 1f - Remaining / duration;

        /// <summary>True while the timer is actively counting down.</summary>
        public bool IsRunning => isRunning && !isPaused;

        private readonly float duration;
        private readonly bool loop;

        private bool isPaused;
        private bool isRunning;

        /// <summary>Creates a timer. Duration is in seconds.</summary>
        public Timer(float duration, bool loop = false)
        {
            if (duration <= 0f)
                CustomLogger.LogError($"Timer duration must be positive, got {duration}.", null);

            this.duration = duration;
            this.loop = loop;
            Remaining = duration;
        }

        /// <summary>Creates, starts, and returns a one-shot countdown in a single call.</summary>
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
            Remaining = duration;
            isRunning = true;
            isPaused = false;
            TimerManager.Register(this);
        }

        /// <summary>Pauses without losing the remaining time.</summary>
        public void Pause() => isPaused = true;

        /// <summary>Resumes after a pause.</summary>
        public void Resume() => isPaused = false;

        /// <summary>Stops the timer and removes it from updates.</summary>
        public void Stop()
        {
            isRunning = false;
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

            if (loop)
            {
                Remaining = duration;
                return;
            }

            Stop();
        }
    }
}