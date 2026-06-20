using Base.SystemsCorePackage.PriorityTrackers;
using Base.SystemsCorePackage.Services;
using UnityEngine;

namespace Base.SystemsCorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Applies a custom timescale while the owning menu is open, scoped by the menu's priority.
    /// Removes it again on close or when destroyed.
    /// </summary>
    public sealed class MenuTimeScaleModule : MenuModule
    {
        [Tooltip("The time scale applied while the menu is open.")]
        [SerializeField] private float timeScale;

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

            if (!ServiceLocator.TryGet(out TimeScaleManager timeScaleManager))
                return;

            timeScaleManager.TimeScaleTracker.Add(timeScale, (uint)OwnerMenu.MenuPriority, this);
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