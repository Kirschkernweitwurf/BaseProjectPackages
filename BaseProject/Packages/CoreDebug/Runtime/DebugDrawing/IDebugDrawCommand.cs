namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// A queued debug draw command that knows when it has outlived the duration it was queued with.
    /// </summary>
    internal interface IDebugDrawCommand
    {
        /// <summary>
        /// Reports whether the command still has to be drawn.
        /// </summary>
        /// <param name="frame">The current frame count.</param>
        /// <param name="unscaledTime">The current unscaled time.</param>
        /// <returns>True while the command is still alive; otherwise false.</returns>
        bool IsAlive(int frame, float unscaledTime);
    }
}