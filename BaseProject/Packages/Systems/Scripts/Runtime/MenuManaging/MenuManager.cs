using System;
using System.Linq;
using Systems.Input;
using Systems.Services;
using Systems.Shutdown;
using Tracking;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility.Logging;

namespace Systems.MenuManaging
{
    /// <summary>
    /// Manages the registration, opening, and closing of menus in the game.
    /// </summary>
    [DefaultExecutionOrder(-5)]
    public class MenuManager : GameServiceBehaviour, IShutdownHandler
    {
        public bool HasShutDown { get; private set; }

        private readonly PriorityTracker<Menu> _menuPriorityTracker = new();
        private readonly Tracker<EMenuIdentifier, Menu> _menuTracker = new();

        /// <summary>
        /// The menu with the highest priority that is currently open and should receive back input.
        /// </summary>
        private Menu _highestPriorityBackMenu;

        protected override void Awake()
        {
            base.Awake();

            ShutdownManager.Register(this);

            _menuPriorityTracker.OnCurrentActiveItemChanged += OnCurrentActiveItemChanged;

            if (ServiceLocator.TryGet(out InputManager inputManager))
                inputManager.InputActions.Permanent.Back.performed += OnBackActionPerformed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!HasShutDown)
                Shutdown();
        }

        public void Shutdown()
        {
            ShutdownManager.Deregister(this);

            HasShutDown = true;

            _menuPriorityTracker.OnCurrentActiveItemChanged -= OnCurrentActiveItemChanged;

            if (ServiceLocator.TryGet(out InputManager inputManager))
                inputManager.InputActions.Permanent.Back.performed -= OnBackActionPerformed;
        }

        /// <summary>
        /// Adds the specified menu to the general menu tracker.
        /// </summary>
        /// <param name="menu">The menu to register.</param>
        public void RegisterMenu(Menu menu) => _menuTracker.Register(menu.MenuIdentifier, menu);

        /// <summary>
        /// Removes the specified menu from the general menu tracker.
        /// </summary>
        /// <param name="menu">The menu to unregister.</param>
        public void DeregisterMenu(Menu menu) => _menuTracker.Remove(menu.MenuIdentifier);

        /// <summary>
        /// Registers a menu to be tracked for opening with a specified priority.
        /// </summary>
        /// <param name="item">The menu to register.</param>
        /// <param name="priority">The priority of the menu. Higher values indicate higher priority.</param>
        /// <param name="caller">The object registering the menu, used for tracking purposes.</param>
        public void RegisterOpenMenu(Menu item, uint priority, object caller)
        {
            _menuPriorityTracker.Add(item, priority, caller);
        }

        /// <summary>
        /// Deregisters a menu from being tracked for opening.
        /// </summary>
        /// <param name="caller">The object that registered the menu.</param>
        public void DeregisterOpenMenu(object caller) => _menuPriorityTracker.Remove(caller);

        /// <summary>
        /// Opens the menu associated with the given identifier.
        /// Optionally closes all other menus before opening the specified one.
        /// </summary>
        /// <param name="identifier">The identifier of the menu to open.</param>
        /// <param name = "parentMenuIdentifier">The identifier of the parent menu, if any.
        /// This is used for hierarchical menu structures. So, e.g. if a settings menu is opened
        /// from the pause menu, the pause menu can be set as the parent and the settings menu
        /// is closed when the pause menu is.</param>
        /// <param name="closeOthers">If set to <c>true</c>, closes all
        /// other menus before opening the specified one.</param>
        public void OpenMenu(EMenuIdentifier identifier, EMenuIdentifier parentMenuIdentifier = EMenuIdentifier.None,
            bool closeOthers = false)
        {
            if (closeOthers)
                CloseAll();

            if (_menuTracker.TryGet(identifier, out Menu menu))
                menu.Open(parentMenuIdentifier);
            else
                CustomLogger.LogWarning($"Menu of identifier {identifier} not found.", this);
        }

        /// <summary>
        /// Closes the menu associated with the given identifier if it is currently open.
        /// </summary>
        /// <param name="identifier">The identifier of the menu to close.</param>
        /// <param name = "closingMenuIdentifier">The identifier of the menu that is closing this menu, if any.</param>
        public void CloseMenu(EMenuIdentifier identifier, EMenuIdentifier closingMenuIdentifier = EMenuIdentifier.None)
        {
            if (_menuTracker.TryGet(identifier, out Menu menu))
                menu.Close(closingMenuIdentifier);
            else
                CustomLogger.LogWarning($"Menu of identifier {identifier} not found.", this);
        }

        /// <summary>
        /// Checks if the menu with the given identifier is currently open.
        /// </summary>
        /// <param name="identifier">The identifier of the menu to check.</param>
        /// <returns><c>true</c> if the menu is open; otherwise, <c>false</c>.</returns>
        public bool IsMenuOpen(EMenuIdentifier identifier)
        {
            return _menuTracker.TryGet(identifier, out Menu menu) && menu.IsOpen;
        }

        /// <summary>
        /// Tries to get the menu associated with the given identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the menu to retrieve.</param>
        /// <param name="menu">The retrieved menu if found; otherwise, null.</param>
        /// <returns><c>true</c> if the menu was found; otherwise, <c>false</c>.</returns>
        public bool TryGetMenu(EMenuIdentifier identifier, out Menu menu) => _menuTracker.TryGet(identifier, out menu);

        /// <summary>
        /// Closes all currently open menus.
        /// </summary>
        private void CloseAll()
        {
            foreach (EMenuIdentifier type in Enum.GetValues(typeof(EMenuIdentifier)))
                if (_menuTracker.TryGet(type, out Menu menu) && menu.IsOpen)
                    menu.Close();
        }

        /// <summary>
        /// Called when the current active menu changes in the priority tracker.
        /// Updates the highest priority menu and back menu accordingly.
        /// </summary>
        private void OnCurrentActiveItemChanged(TrackedItem<Menu> trackedItem)
        {
            if (trackedItem == null)
            {
                _highestPriorityBackMenu = null;
                return;
            }

            _highestPriorityBackMenu = _menuPriorityTracker.TrackedItems
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.Order)
                .FirstOrDefault(x => x.Item.ListenToOnBackAction)?.Item;

        }

        /// <summary>
        /// Handles the back action input.
        /// </summary>
        private void OnBackActionPerformed(InputAction.CallbackContext obj)
        {
            if (_highestPriorityBackMenu == null)
            {
                // If no menu is available to handle the back action, open the Pause Menu
                if (_menuTracker.TryGet(EMenuIdentifier.Pause, out Menu pauseMenu) && !pauseMenu.IsOpen)
                    pauseMenu.Open();

                return;
            }

            _highestPriorityBackMenu?.Back();
        }
    }
}