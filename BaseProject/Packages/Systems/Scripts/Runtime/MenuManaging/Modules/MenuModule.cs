using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Base for components that react to a <see cref="Menu"/>'s lifecycle. Lets single-purpose menu
    /// concerns (cursor, timescale, input map, focus, reset) live in their own components instead of
    /// bloating <see cref="Menu"/>. Must sit on the same GameObject as the menu it extends.
    /// </summary>
    [RequireComponent(typeof(Menu))]
    public abstract class MenuModule : MonoBehaviour
    {
        protected Menu OwnerMenu { get; private set; }

#region Unity Callbacks
        protected virtual void Awake()
        {
            OwnerMenu = GetComponent<Menu>();

            if (OwnerMenu == null)
            {
                CustomLogger.LogError("MenuModule requires a sibling Menu component.", this);
                return;
            }

            OwnerMenu.Opened += HandleMenuOpened;
            OwnerMenu.Closed += HandleMenuClosed;
        }

        protected virtual void OnDestroy()
        {
            if (OwnerMenu == null)
                return;

            OwnerMenu.Opened -= HandleMenuOpened;
            OwnerMenu.Closed -= HandleMenuClosed;
        }
#endregion

        /// <summary>Called once the owning menu has opened.</summary>
        protected virtual void OnMenuOpened() { }

        /// <summary>Called once the owning menu has fully closed.</summary>
        protected virtual void OnMenuClosed() { }

        private void HandleMenuOpened() => OnMenuOpened();

        private void HandleMenuClosed() => OnMenuClosed();
    }
}