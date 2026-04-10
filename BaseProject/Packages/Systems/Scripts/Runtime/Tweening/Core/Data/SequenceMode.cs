namespace Systems.Tweening.Core.Data
{
    /// <summary>
    /// Defines how the sequence should execute.
    /// </summary>
    public enum ESequenceMode : byte
    {
        /// <summary>
        /// All tweens play simultaneously.
        /// </summary>
        Parallel = 0,

        /// <summary>
        /// Tweens play one after another.
        /// </summary>
        Sequential = 1
    }
}