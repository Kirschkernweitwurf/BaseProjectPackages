using Base.CorePackage.PriorityTrackers;
using Base.CorePackage.Services;
using Base.CorePackage.Services.Shutdown;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Applies a custom timescale while the owning menu is open, scoped by the menu's priority.
    /// Removes it again on close or when destroyed.
    /// </summary>
    public sealed class MenuTimeScaleModule : MenuModule, IShutdownHandler
    {
        [Tooltip("The time scale applied while the menu is open.")]
        [SerializeField] private float timeScale;

        public bool HasShutDown { get; private set; }

        private bool _isApplied;

#region Unity Callbacks
        private void Awake() => ShutdownManager.Register(this);

        private void OnDestroy() => Shutdown();
#endregion

        public void Shutdown()
        {
            if (HasShutDown)
                return;

            HasShutDown = true;
            Release();
            ShutdownManager.Deregister(this);
        }

        protected override void OnMenuOpened()
        {
            if (_isApplied)
                return;

            if (!ServiceLocator.TryGet(out TimeScaleManager timeScaleManager))
                return;

            timeScaleManager.TimeScaleTracker.Add(timeScale, (uint)OwnerMenu.Priority, this);
            _isApplied = true;
        }

        protected override void OnMenuClosed() => Release();

        private void Release()
        {
            if (!_isApplied)
                return;

            ServiceLocator.Get<TimeScaleManager>()?.TimeScaleTracker.Remove(this);
            _isApplied = false;
        }
    }
}