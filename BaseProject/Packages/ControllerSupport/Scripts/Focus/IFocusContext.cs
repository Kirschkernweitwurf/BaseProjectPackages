namespace Base.ControllerSupport.Focus
{
    /// <summary>
    /// A source of fallback focus for the <see cref="FocusWatchdog"/>. Implemented by whatever owns a
    /// screen's default selection, for example a menu component in the game layer.
    /// </summary>
    /// <remarks>
    /// This interface exists to keep the dependency one way. The Controller Support package can restore
    /// focus through <see cref="IFocusContext"/> without referencing the menu layer, so the menu layer
    /// depends on this package and never the reverse. That avoids a circular assembly reference.
    /// </remarks>
    public interface IFocusContext
    {
        /// <summary>
        /// Selects a sensible element for this context. Called by the watchdog when a gamepad is active
        /// and the current selection has become null or inactive.
        /// </summary>
        void RestoreFocus();
    }
}