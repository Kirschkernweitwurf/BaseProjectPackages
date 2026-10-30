using System;
using System.Collections.Generic;
using System.Linq;
using Base.AttributesPackage;
using Base.CorePackage.Input;
using Base.CorePackage.MenuManaging.Identifier;
using Base.ServicesPackage;
using Base.ServicesPackage.Shutdown;
using Base.ServicesPackage.Tracking;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.CorePackage.MenuManaging
{
    /// <summary>
    /// Manages the registration, opening, and closing of menus in the game. Keeps the one stack of open menus that
    /// decides what is on top: the menu with the highest priority, and between equal priorities the one opened last.
    /// Back, the focus and every input shortcut ask it through <see cref="TopMenu"/> and <see cref="IsOnTop"/>, so
    /// nothing behind the top screen reacts to input.
    /// </summary>
    [DefaultExecutionOrder(-5)]
    public class MenuManager : GameServiceBehaviour, IShutdownHandler
    {
        [Title("Back Action Settings")]
        [Tooltip("The menu to open when Back is pressed and nothing on the top screen takes it, like the pause menu. "
            + "It does not open below a menu of higher priority, where it could not be seen. Leave empty to "
            + "disable this fallback.")]
        [SerializeField] private MenuIdentifier defaultBackMenu;

        /// <summary>Raised when another menu comes on top, or the last one closes.</summary>
        public event Action TopMenuChanged;

        /// <summary><c>true</c> if the manager has been shut down and is no longer valid.</summary>
        public bool HasShutDown { get; private set; }

        /// <summary>
        /// The open menu on top: highest priority first, then the one opened last. A menu stops counting as soon as
        /// its close starts. <c>null</c> while no menu is open, when the scene itself has the input.
        /// </summary>
        public Menu TopMenu { get; private set; }

        private readonly PriorityTracker<Menu> _menuPriorityTracker = new();
        private readonly Tracker<MenuIdentifier, Menu> _menuTracker = new();
        private readonly List<BackClaim> _backClaims = new();

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            ShutdownManager.Register(this);

            if (ServiceLocator.TryGet(out InputManager inputManager))
                inputManager.BaseInputActions.Permanent.Back.performed += OnBackActionPerformed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!HasShutDown)
                Shutdown();
        }
#endregion

        /// <summary>
        /// Drops all subscriptions. Runs automatically when the manager is destroyed.
        /// </summary>
        public void Shutdown()
        {
            if (HasShutDown)
                return;

            HasShutDown = true;

            ShutdownManager.Deregister(this);

            // Optional on the way out. Shutting down runs on a scene unload, where the input service
            // may already be gone, and reporting that would turn every scene change into an error.
            if (ServiceLocator.TryGetOptional(out InputManager inputManager))
                inputManager.BaseInputActions.Permanent.Back.performed -= OnBackActionPerformed;
        }

        /// <summary>
        /// Adds the specified menu to the general menu tracker.
        /// </summary>
        /// <param name="menu">The menu to register.</param>
        public void RegisterMenu(Menu menu)
        {
            if (menu == null)
            {
                CustomLogger.LogWarning("Cannot register menu: menu is null.", this);
                return;
            }

            if (menu.MenuIdentifier == null)
            {
                CustomLogger.LogWarning("Cannot register menu: menu identifier is null.", this);
                return;
            }

            _menuTracker.Register(menu.MenuIdentifier, menu);
        }

        /// <summary>
        /// Removes the specified menu from the general menu tracker.
        /// </summary>
        /// <param name="menu">The menu to unregister.</param>
        public void DeregisterMenu(Menu menu)
        {
            if (menu == null)
            {
                CustomLogger.LogWarning("Cannot deregister menu: menu is null.", this);
                return;
            }

            if (menu.MenuIdentifier == null)
            {
                CustomLogger.LogWarning("Cannot deregister menu: menu identifier is null.", this);
                return;
            }

            _menuTracker.Remove(menu.MenuIdentifier);
        }

        /// <summary>
        /// Registers a menu to be tracked for opening with a specified priority.
        /// </summary>
        /// <param name="item">The menu to register.</param>
        /// <param name="priority">The priority of the menu. Higher values indicate higher priority.</param>
        /// <param name="caller">The object registering the menu, used for tracking purposes.</param>
        public void RegisterOpenMenu(Menu item, uint priority, object caller)
        {
            _menuPriorityTracker.Add(item, priority, caller);
            RefreshTopMenu();
        }

        /// <summary>
        /// Deregisters a menu from being tracked for opening.
        /// </summary>
        /// <param name="caller">The object that registered the menu.</param>
        public void DeregisterOpenMenu(object caller)
        {
            _menuPriorityTracker.Remove(caller);
            RefreshTopMenu();
        }

        /// <summary>
        /// Opens the menu associated with the given identifier.
        /// Optionally closes all other menus before opening the specified one.
        /// </summary>
        /// <param name="identifier">The identifier of the menu to open.</param>
        /// <param name="parentMenuIdentifier">
        /// The identifier of the parent menu, if any.
        /// This is used for hierarchical menu structures. So, e.g. if a settings menu is opened
        /// from the pause menu, the pause menu can be set as the parent and the settings menu
        /// is closed when the pause menu is.
        /// </param>
        /// <param name="closeOthers">
        /// If set to <c>true</c>, closes all
        /// other menus before opening the specified one.
        /// </param>
        public void OpenMenu(MenuIdentifier identifier, MenuIdentifier parentMenuIdentifier = null,
            bool closeOthers = false)
        {
            if (closeOthers)
                CloseAll();

            if (identifier == null)
            {
                CustomLogger.LogWarning("Cannot open menu: identifier is null.", this);
                return;
            }

            if (_menuTracker.TryGet(identifier, out Menu menu))
                menu.Open(parentMenuIdentifier);
            else
                CustomLogger.LogWarning($"Menu of identifier {identifier} not found.", this);
        }

        /// <summary>
        /// Closes the menu associated with the given identifier if it is currently open.
        /// </summary>
        /// <param name="identifier">The identifier of the menu to close.</param>
        /// <param name="closingMenuIdentifier">The identifier of the menu that is closing this menu, if any.</param>
        public void CloseMenu(MenuIdentifier identifier, MenuIdentifier closingMenuIdentifier = null)
        {
            if (identifier == null)
            {
                CustomLogger.LogWarning("Cannot close menu: identifier is null.", this);
                return;
            }

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
        public bool IsMenuOpen(MenuIdentifier identifier)
        {
            if (identifier == null)
            {
                CustomLogger.LogWarning("Cannot check menu: identifier is null.", this);
                return false;
            }

            if (!_menuTracker.TryGet(identifier, out Menu menu))
            {
                CustomLogger.LogWarning($"Menu of identifier {identifier} not found.", this);
                return false;
            }

            return menu.IsOpen;
        }

        /// <summary>
        /// Tries to get the menu associated with the given identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the menu to retrieve.</param>
        /// <param name="menu">The retrieved menu if found; otherwise, null.</param>
        /// <returns><c>true</c> if the menu was found; otherwise, <c>false</c>.</returns>
        public bool TryGetMenu(MenuIdentifier identifier, out Menu menu)
        {
            if (identifier == null)
            {
                CustomLogger.LogWarning("Cannot check menu: identifier is null.", this);
                menu = null;
                return false;
            }

            return _menuTracker.TryGet(identifier, out menu);
        }

        /// <summary>
        /// Whether <paramref name="component"/> belongs to the screen on top, so its input shortcuts may act. A
        /// component belongs to the nearest menu above it, and a tab belongs to the screen of the menu that holds it.
        /// Outside any menu it belongs to the scene, which is on top while no menu is open.
        /// </summary>
        /// <param name="component">Anything on screen, like a button with a shortcut or a gameplay input.</param>
        /// <returns><c>true</c> when nothing covers it.</returns>
        public bool IsOnTop(Component component)
        {
            if (component == null)
            {
                CustomLogger.LogWarning("Cannot check a null component for being on top.", this);
                return false;
            }

            return IsScreenOnTop(component.GetComponentInParent<Menu>(includeInactive: true));
        }

        /// <summary>
        /// Whether the screen of <paramref name="menu"/> is on top, the same rule as <see cref="IsOnTop"/> for code
        /// that already knows its menu. <c>null</c> stands for the scene.
        /// </summary>
        /// <param name="menu">The menu to check, or <c>null</c> for the scene.</param>
        /// <returns><c>true</c> when nothing covers that screen.</returns>
        public bool IsScreenOnTop(Menu menu)
        {
            if (menu == null)
                return TopMenu == null;

            return TopMenu != null
                && GetScreen(menu) == GetScreen(TopMenu);
        }

        /// <summary>
        /// Whether <paramref name="menu"/> would come on top if it opened now: no open menu has a higher priority.
        /// A menu opened below a higher one stays out of sight and out of reach, like a pause menu behind a full
        /// screen, so shortcuts that open a menu on their own, like Pause, ask this first.
        /// </summary>
        /// <param name="menu">The menu to open.</param>
        /// <returns><c>true</c> when it would be on top.</returns>
        public bool CanShowOnTop(Menu menu)
        {
            if (menu == null)
            {
                CustomLogger.LogWarning("Cannot check a null menu for showing on top.", this);
                return false;
            }

            return TopMenu == null
                || menu.Priority >= TopMenu.Priority;
        }

        /// <summary>
        /// Takes Back for a step of <paramref name="owner"/>'s own, like putting down a held card or leaving a page,
        /// and calls <paramref name="onBack"/> instead. A claim only acts while its owner's screen is on top (see
        /// <see cref="IsOnTop"/>), so a menu opened over it takes Back first, and the claim works again once that
        /// menu closed. Between claims of the same screen the newest wins. Claiming again replaces the callback.
        /// Give it back with <see cref="ReleaseBack"/>.
        /// </summary>
        /// <param name="owner">Who claims Back. Its place on screen decides when the claim acts.</param>
        /// <param name="onBack">Runs on Back while the claim acts.</param>
        public void ClaimBack(Component owner, Action onBack)
        {
            if (owner == null
                || onBack == null)
            {
                CustomLogger.LogWarning("Tried to claim Back without an owner or a callback.", this);
                return;
            }

            RemoveClaim(owner);
            _backClaims.Add(new BackClaim(owner, onBack));
        }

        /// <summary>Gives Back back to the menus. Does nothing for an owner without a claim.</summary>
        /// <param name="owner">The owner that claimed it with <see cref="ClaimBack"/>.</param>
        public void ReleaseBack(Component owner)
        {
            if (owner != null)
                RemoveClaim(owner);
        }

        // Walks up through embedded menus, so a tab and the menu holding it count as one screen.
        private static Menu GetScreen(Menu menu)
        {
            while (menu.IsEmbedded
                && menu.ParentMenu != null)
                menu = menu.ParentMenu;

            return menu;
        }

        /// <summary>
        /// Closes all currently open menus.
        /// </summary>
        private void CloseAll()
        {
            // Iterate over a copy since Close() mutates the tracked list.
            foreach (TrackedItem<Menu> trackedItem in _menuPriorityTracker.TrackedItems.ToArray())
            {
                if (trackedItem == null
                    || trackedItem.Item == null
                    || !trackedItem.Item.IsOpen)
                    continue;

                trackedItem.Item.Close();
            }
        }

        /// <summary>
        /// Picks the menu on top. Runs on every open and close, not only when the newest menu changes: a menu below the
        /// top, like one closing while another opens at the same priority, must never stay on top after it closed.
        /// </summary>
        private void RefreshTopMenu()
        {
            TrackedItem<Menu> best = null;

            foreach (TrackedItem<Menu> candidate in _menuPriorityTracker.TrackedItems)
            {
                if (candidate == null
                    || candidate.Item == null)
                    continue;

                if (best == null
                    || candidate.Priority > best.Priority
                    || candidate.Priority == best.Priority && candidate.Order > best.Order)
                    best = candidate;
            }

            Menu top = best?.Item;
            if (top == TopMenu)
                return;

            TopMenu = top;
            TopMenuChanged?.Invoke();
        }

        /// <summary>
        /// Gives Back to exactly one receiver, the first of: the newest claim on the top screen, the innermost menu of
        /// the top screen that listens to Back, the default back menu. A top screen that does not listen keeps Back
        /// from every menu behind it, so Back never closes something the player can't see.
        /// </summary>
        private void OnBackActionPerformed(InputAction.CallbackContext context)
        {
            if (TryGetActiveClaim(out BackClaim claim))
            {
                claim.OnBack();
                return;
            }

            if (TryGetBackListener(out Menu listener))
            {
                listener.Back();
                return;
            }

            OpenDefaultBackMenu();
        }

        // A tab that ignores Back, like a console inside a debug menu, hands it to the menu that holds it.
        private bool TryGetBackListener(out Menu listener)
        {
            listener = TopMenu;

            while (listener != null)
            {
                if (listener.ListenToOnBackAction)
                    return true;

                listener = listener.IsEmbedded
                    ? listener.ParentMenu
                    : null;
            }

            return false;
        }

        private bool TryGetActiveClaim(out BackClaim active)
        {
            active = default;

            for (int index = _backClaims.Count - 1; index >= 0; index--)
            {
                BackClaim claim = _backClaims[index];

                // An owner destroyed without releasing, like on a scene unload, drops out on its own.
                if (claim.Owner == null)
                {
                    _backClaims.RemoveAt(index);
                    continue;
                }

                if (!IsOnTop(claim.Owner))
                    continue;

                active = claim;
                return true;
            }

            return false;
        }

        private void OpenDefaultBackMenu()
        {
            // Below a menu of higher priority it would open out of sight and only stop the game.
            if (defaultBackMenu == null
                || !_menuTracker.TryGet(defaultBackMenu, out Menu fallbackMenu)
                || fallbackMenu.IsOpen
                || !CanShowOnTop(fallbackMenu))
                return;

            fallbackMenu.Open();
        }

        private void RemoveClaim(Component owner)
        {
            for (int index = _backClaims.Count - 1; index >= 0; index--)
            {
                if (_backClaims[index].Owner == owner)
                    _backClaims.RemoveAt(index);
            }
        }
    }
}