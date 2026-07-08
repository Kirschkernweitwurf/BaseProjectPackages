namespace Base.CorePackage.Tweening.Core
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
        /// Stops the tween. By default the tween is killed without firing <c>OnComplete</c>.
        /// When <paramref name="complete"/> is <c>true</c>, the tween snaps to its end value and
        /// <c>OnComplete</c> is fired before <c>OnKill</c> — useful to resolve gameplay logic
        /// that depends on a tween finishing.
        /// </summary>
        /// <param name="complete">If <c>true</c>, complete the tween instead of just killing it.</param>
        void Stop(bool complete = false);

        /// <summary>
        /// Advances the tween by the given delta time.
        /// </summary>
        /// <param name="deltaTime">Time in seconds since the last tick.</param>
        void Tick(float deltaTime);
    }
}