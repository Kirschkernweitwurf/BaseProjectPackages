namespace Base.CorePackage.MenuManaging
{
    /// <summary>
    /// Implemented by components that should reset to a known baseline when their
    /// owning menu closes. The menu discovers these in its children automatically.
    /// </summary>
    public interface IMenuResettable
    {
        /// <summary>
        /// Resets the component to a known baseline state. Called by the owning menu when it closes.
        /// </summary>
        void ResetState();
    }
}