namespace Systems.Tweening.Core
{
    /// <summary>
    /// Interface for tween-like objects.
    /// </summary>
    public interface ITween
    {
        /// <summary>
        /// Indicates if the tween is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Indicates if the tween has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Starts the tween.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the tween.
        /// </summary>
        void Stop();

        /// <summary>
        /// Advances the tween by the given delta time.
        /// </summary>
        /// <param name="deltaTime">Time in seconds since the last tick.</param>
        void Tick(float deltaTime);
    }
}