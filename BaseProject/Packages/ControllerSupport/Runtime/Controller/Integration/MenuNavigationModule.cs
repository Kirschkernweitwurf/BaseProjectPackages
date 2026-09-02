using Base.AttributesPackage;
using Base.ControllerSupportPackage.Controller.Navigation;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.MenuManaging.Modules;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.ControllerSupportPackage.Controller.Integration
{
    /// <summary>
    /// Bridges a <see cref="Menu"/>'s lifecycle to a <see cref="NavigableGroup"/>. Activates the group
    /// when the menu opens and deactivates it when the menu closes. This is the single deliberate seam
    /// between the menu layer and the controller package, so navigation stays menu agnostic everywhere
    /// else. It depends on both layers, so it belongs in the assembly that already references the menu
    /// package rather than in the core navigation package.
    /// </summary>
    public sealed class MenuNavigationModule : MenuModule
    {
        [Tooltip("Group activated while the owning menu is open.")]
        [Required]
        [Child]
        [SerializeField] private NavigableGroup navigableGroup;

#region Unity Callbacks
        // Reported at startup instead of silently fixed, so the authored setup stays the single truth.
        private void Awake()
        {
            if (navigableGroup.AutoActivate)
                CustomLogger.LogWarning($"The linked {nameof(NavigableGroup)} has Auto Activate enabled, but the "
                    + "menu is the one activating it. Disable Auto Activate on the group.", this);

            if (navigableGroup.Priority != OwnerMenu.Priority)
                CustomLogger.LogWarning($"The linked {nameof(NavigableGroup)} has priority "
                    + $"{navigableGroup.Priority} while the menu has {OwnerMenu.Priority}. Match them so focus "
                    + "order follows menu order.", this);
        }
#endregion

        protected override void OnMenuOpened() => navigableGroup.Activate();

        protected override void OnMenuClosed() => navigableGroup.Deactivate();
    }
}