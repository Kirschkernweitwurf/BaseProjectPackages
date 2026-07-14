using Base.CorePackage.PriorityTrackers;
using Base.CorePackage.Services;
using Base.CorePackage.Services.Shutdown;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Applies custom cursor settings while the owning menu is open, scoped by the menu's priority.
    /// Removes them again on close or when destroyed.
    /// </summary>
    public sealed class MenuCursorModule : MenuModule, IShutdownHandler
    {
        [Tooltip("The cursor settings applied while the menu is open.")]
        [SerializeField] private CursorRequest cursorSettings = new();

        public bool HasShutDown { get; private set; }

        private bool _isApplied;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            ShutdownManager.Register(this);
        }

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

            if (!ServiceLocator.TryGet(out CursorManager cursorManager))
                return;

            cursorManager.CursorTracker.Add(cursorSettings, (uint)OwnerMenu.MenuPriority, this);
            _isApplied = true;
        }

        protected override void OnMenuClosed() => Release();

        private void Release()
        {
            if (!_isApplied)
                return;

            ServiceLocator.Get<CursorManager>()?.CursorTracker.Remove(this);
            _isApplied = false;
        }
    }
}