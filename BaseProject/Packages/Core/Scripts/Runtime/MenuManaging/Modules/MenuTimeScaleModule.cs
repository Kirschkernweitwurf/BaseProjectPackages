using Base.CorePackage.PriorityTrackers;
using Base.CorePackage.Services;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Applies a custom timescale while the owning menu is open, scoped by the menu's priority.
    /// Removes it again on close or when destroyed.
    /// </summary>
    public sealed class MenuTimeScaleModule : ScopedMenuModule
    {
        [Tooltip("The time scale applied while the menu is open.")]
        [Min(0f)] [SerializeField] private float timeScale;

        protected override bool TryApply()
        {
            if (!ServiceLocator.TryGet(out TimeScaleManager timeScaleManager))
                return false;

            timeScaleManager.TimeScaleTracker.Add(timeScale, (uint)OwnerMenu.Priority, this);
            return true;
        }

        protected override void Release()
        {
            if (ServiceLocator.TryGet(out TimeScaleManager timeScaleManager))
                timeScaleManager.TimeScaleTracker.Remove(this);
        }
    }
}