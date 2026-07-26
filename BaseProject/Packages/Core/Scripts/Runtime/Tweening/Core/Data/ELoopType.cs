namespace Base.CorePackage.Tweening.Core.Data
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
        /// The tween alternates between forward and reverse direction on each loop.
        /// </summary>
        PingPong = 2,

        /// <summary>
        /// The tween continues from the original start value to the original target value.
        /// Behaves like <see cref="Restart"/> but without snapping back to the default value
        /// between loops.
        /// </summary>
        Continue = 3
    }
}