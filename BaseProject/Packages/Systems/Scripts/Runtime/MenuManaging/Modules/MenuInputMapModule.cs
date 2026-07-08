using Base.AttributePackage;
using Base.CorePackage.Input;
using Base.CorePackage.Services;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Overrides the active input action map while the owning menu is open, scoped by the menu's
    /// priority. Restores the previous map on close or when destroyed.
    /// </summary>
    public sealed class MenuInputMapModule : MenuModule
    {
        [Tooltip("The action map activated while the menu is open.")]
        [SerializeField] private InputActionMapReference actionMap;

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
            if (_isApplied || !actionMap.IsValid)
                return;

            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            if (!inputManager.TryResolveBaseMap(actionMap, out InputActionMap resolvedMap))
                return;

            inputManager.RegisterInputMap(resolvedMap, this, (uint)OwnerMenu.MenuPriority);
            _isApplied = true;
        }

        protected override void OnMenuClosed() => Release();

        private void Release()
        {
            if (!_isApplied)
                return;

            ServiceLocator.Get<InputManager>()?.DeregisterInputMap(this);
            _isApplied = false;
        }
    }
}