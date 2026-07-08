using Base.CorePackage.PriorityTrackers;
using Base.CorePackage.Services;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Applies custom cursor settings while the owning menu is open, scoped by the menu's priority.
    /// Removes them again on close or when destroyed.
    /// </summary>
    public sealed class MenuCursorModule : MenuModule
    {
        [Tooltip("The cursor settings applied while the menu is open.")]
        [SerializeField] private CursorRequest cursorSettings = new();

        private bool _isApplied;

#region Unity Callbacks
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Release();
        }
#endregion

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