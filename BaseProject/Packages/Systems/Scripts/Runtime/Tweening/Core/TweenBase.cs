using System;

namespace Systems.Tweening.Core
{
    /// <summary>
    /// Base type for all tween instances. Provides lifecycle and completion events.
    /// </summary>
    public abstract class TweenBase : ITween
    {
        /// <summary>
        /// Event invoked when the tween completes.
        /// </summary>
        public event Action<TweenBase> OnComplete;

        public abstract bool IsRunning { get; }

        public abstract bool IsCompleted { get; }

        public abstract void Start();

        public abstract void Stop();

        public abstract void Tick(float deltaTime);

        /// <summary>
        /// Invokes the completion event.
        /// </summary>
        protected void Complete() => OnComplete?.Invoke(this);
    }
}