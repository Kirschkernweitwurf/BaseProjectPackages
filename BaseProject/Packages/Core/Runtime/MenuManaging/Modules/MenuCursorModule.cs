using Base.CorePackage.PriorityTrackers;
using Base.ServicesPackage;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Applies custom cursor settings while the owning menu is open, scoped by the menu's priority.
    /// Removes them again on close or when destroyed.
    /// </summary>
    public sealed class MenuCursorModule : ScopedMenuModule
    {
        [Tooltip("The cursor settings applied while the menu is open.")]
        [SerializeField] private CursorRequest cursorSettings = new();

        protected override bool TryApply()
        {
            if (!ServiceLocator.TryGet(out CursorManager cursorManager))
                return false;

            cursorManager.CursorTracker.Add(cursorSettings, (uint)OwnerMenu.Priority, this);
            return true;
        }

        protected override void Release()
        {
            // Optional on the way out. Releasing runs when a menu closes or is destroyed, and on a
            // scene unload the service can already be gone. Reporting that would spam the console
            // with an error about a teardown that went exactly right.
            if (ServiceLocator.TryGetOptional(out CursorManager cursorManager))
                cursorManager.CursorTracker.Remove(this);
        }
    }
}