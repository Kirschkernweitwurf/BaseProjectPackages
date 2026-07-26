using Base.CorePackage.Services.Shutdown;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Base for modules that apply a priority-scoped effect while the owning menu is open and remove it
    /// again on close or destroy. Keeps the apply/release bookkeeping in one place, so a concrete module
    /// only has to describe the effect itself.
    /// </summary>
    public abstract class ScopedMenuModule : MenuModule, IShutdownHandler
    {
        /// <summary><c>true</c> if the module has been shut down and is no longer valid.</summary>
        public bool HasShutDown { get; private set; }

        private bool _isApplied;

#region Unity Callbacks
        private void Awake() => ShutdownManager.Register(this);

        private void OnDestroy() => Shutdown();
#endregion

        /// <summary>
        /// Releases the effect and drops the shutdown registration. Runs automatically on destroy.
        /// </summary>
        public void Shutdown()
        {
            if (HasShutDown)
                return;

            HasShutDown = true;

            ReleaseIfApplied();
            ShutdownManager.Deregister(this);
        }

        /// <summary>
        /// Applies the effect. Returns <c>false</c> when it could not be applied, for example because
        /// the owning service is missing. <see cref="Release"/> only runs after a successful apply.
        /// </summary>
        protected abstract bool TryApply();

        /// <summary>Removes the effect that <see cref="TryApply"/> added.</summary>
        protected abstract void Release();

        protected override void OnMenuOpened()
        {
            if (_isApplied)
                return;

            _isApplied = TryApply();
        }

        protected override void OnMenuClosed() => ReleaseIfApplied();

        private void ReleaseIfApplied()
        {
            if (!_isApplied)
                return;

            _isApplied = false;
            Release();
        }
    }
}