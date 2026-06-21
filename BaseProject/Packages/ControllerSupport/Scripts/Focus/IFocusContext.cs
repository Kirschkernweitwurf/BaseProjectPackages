namespace Base.ControllerSupport.Focus
{
    /// <summary>
    /// Implement on a component to define a focus context for the <see cref="FocusWatchdog"/>.
    /// The watchdog will call <see cref="RestoreFocus"/> on the
    /// active context whenever it detects that the current selection
    /// </summary>
    public interface IFocusContext
    {
        /// <summary>
        /// Called by the <see cref="FocusWatchdog"/> when it detects
        /// that the current selection is <c>null</c> or inactive.
        /// </summary>
        void RestoreFocus();
    }
}