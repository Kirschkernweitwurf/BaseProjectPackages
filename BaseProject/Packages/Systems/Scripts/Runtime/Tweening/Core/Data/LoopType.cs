namespace Systems.Tweening.Core.Data
{
    /// <summary>
    /// Defines how a tween should loop when finished.
    /// </summary>
    public enum ELoopType : byte
    {
        /// <summary>
        /// The tween plays once and stops.
        /// </summary>
        None = 0,

        /// <summary>
        /// The tween restarts from the beginning after finishing.
        /// </summary>
        Restart = 1,

        /// <summary>
        /// The tween reverses direction (ping-pong effect).
        /// </summary>
        PingPong = 2,

        /// <summary>
        /// The tween continues indefinitely.
        /// </summary>
        Continue = 3
    }
}