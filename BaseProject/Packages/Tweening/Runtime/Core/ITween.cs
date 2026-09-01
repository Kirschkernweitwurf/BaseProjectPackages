namespace Base.TweeningPackage.Core
{
    /// <summary>
    /// Interface for tween-like objects.
    /// </summary>

    // The runner only drives a tween, so it calls Tick and IsCompleted and nothing else. IsRunning,
    // Start and Stop are the caller's half of the contract and are used on the concrete tween types.
    // They stay on the interface because a tween that cannot be started through it is not a tween.
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
        /// Stops the tween. By default, the tween is killed without firing <c>OnComplete</c>.
        /// When <paramref name="complete"/> is <c>true</c>, the tween snaps to its end value and
        /// <c>OnComplete</c> is fired before <c>OnKill</c>. This is useful for any
        /// logic that depends on a tween finishing.
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