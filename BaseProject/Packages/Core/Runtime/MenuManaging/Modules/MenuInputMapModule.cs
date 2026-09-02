using Base.CorePackage.Input;
using Base.ServicesPackage;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Overrides the active input action map while the owning menu is open, scoped by the menu's
    /// priority. Restores the previous map on close or when destroyed.
    /// </summary>
    public sealed class MenuInputMapModule : ScopedMenuModule
    {
        [Tooltip("The action map activated while the menu is open.")]
        [SerializeField] private InputActionMapReference actionMap;

        protected override bool TryApply()
        {
            if (!actionMap.IsValid)
                return false;

            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return false;

            if (!inputManager.TryResolveBaseMap(actionMap, out InputActionMap resolvedMap))
                return false;

            inputManager.RegisterInputMap(resolvedMap, this, (uint)OwnerMenu.Priority);
            return true;
        }

        protected override void Release()
        {
            if (ServiceLocator.TryGet(out InputManager inputManager))
                inputManager.DeregisterInputMap(this);
        }
    }
}