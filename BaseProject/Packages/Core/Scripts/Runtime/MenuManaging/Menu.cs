using System;
using System.Collections.Generic;
using Base.CorePackage.MenuManaging.Identifier;
using Base.CorePackage.MenuManaging.Modules;
using Base.CorePackage.Services;
using Base.CorePackage.Services.Shutdown;
using Base.CorePackage.Tracking;
using Base.CorePackage.Tweening.Components.System;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CorePackage.MenuManaging
{
    /// <summary>
    /// Base class for all menus in the game. Handles lifecycle and open/close animations. System
    /// concerns such as cursor, timescale, input map and child reset live in their own
    /// <see cref="MenuModule"/> components and react to the events exposed here.
    /// </summary>
    public class Menu : MonoBehaviour, IShutdownHandler
    {
        /// <summary>Raised after the menu has opened and its open animation has started.</summary>
        public event Action Opened;

        /// <summary>Raised after the menu has fully closed and its close animation has finished.</summary>
        public event Action Closed;

        /// <summary>Raised when the menu closes in response to a back request.</summary>
        public event Action BackRequested;

        [field: Header("Menu Settings")]

        [Tooltip("The unique identifier asset for this menu.")]
        [field: SerializeField] public MenuIdentifier MenuIdentifier { get; private set; }

        [Tooltip("The root TweenGroup for this menu's open/close animations.")]
        [SerializeField] private TweenGroup contentRoot;

        [field: Tooltip("The priority of this menu in the stack.")]
        [field: SerializeField] public EPriority MenuPriority { get; private set; }

        [Space]
        [Tooltip("If true, this menu will automatically open on Start (with animation).")]
        [SerializeField] private bool openOnStart;

        [field: Tooltip("If true, this menu will listen to the OnBack action to close itself.")]
        [field: SerializeField] public bool ListenToOnBackAction { get; private set; } = true;

        [Tooltip("Menus that block this menu from opening if they are currently open.")]
        [SerializeField] private MenuIdentifier[] blockingMenus;

        public bool IsOpen { get; private set; }

        public bool HasShutDown { get; private set; }

        /// <summary>The root tween group driving this menu's open and close animation.</summary>
        public TweenGroup ContentRoot => contentRoot;

        private readonly List<MenuIdentifier> _childMenuIdentifiers = new();

        private Menu _parentMenu;

#region Unity Callbacks
        protected virtual void Awake()
        {
            ShutdownManager.Register(this);

            if (ServiceLocator.TryGet(out MenuManager menuManager))
                menuManager.RegisterMenu(this);

            if (MenuIdentifier == null)
                CustomLogger.LogWarning("Menu has no MenuIdentifier assigned.", this);

            if (contentRoot == null)
                CustomLogger.LogWarning($"Menu \"{MenuIdentifier}\" has no TweenGroup assigned.", this);
        }

        protected virtual void Start()
        {
            if (openOnStart)
                Open();
            else
                contentRoot.SetVisibility(false);
        }

        protected virtual void OnDestroy()
        {
            if (!HasShutDown)
                Shutdown();
        }
#endregion

        public virtual void Shutdown()
        {
            ShutdownManager.Deregister(this);

            if (ServiceLocator.TryGet(out MenuManager menuManager))
                menuManager.DeregisterMenu(this);

            HasShutDown = true;

            if (IsOpen)
                CleanupMenuState();

            Opened = null;
            Closed = null;
            BackRequested = null;
        }

        /// <summary>
        /// Opens the menu (always animated).
        /// </summary>
        public void Open(MenuIdentifier parentMenuIdentifier = null)
        {
            if (IsOpen)
            {
                CustomLogger.LogWarning($"Menu \"{MenuIdentifier}\" is already open.", this);
                return;
            }

            foreach (MenuIdentifier blockingMenu in blockingMenus)
            {
                if (blockingMenu == null)
                    continue;

                if (!ServiceLocator.TryGet(out MenuManager menuManager))
                    continue;

                if (!menuManager.IsMenuOpen(blockingMenu))
                    continue;

                CustomLogger.LogWarning(
                    $"Cannot open menu \"{MenuIdentifier}\" because blocking" + $" menu \"{blockingMenu}\" is open.",
                    this);

                return;
            }

            IsOpen = true;

            contentRoot.SetVisibility(true);
            contentRoot?.Show();

            RegisterParentMenu(parentMenuIdentifier);
            ServiceLocator.Get<MenuManager>()?.RegisterOpenMenu(this, (uint)MenuPriority, this);

            OnOpened();
            Opened?.Invoke();
        }

        /// <summary>
        /// Closes the menu (always animated).
        /// </summary>
        public void Close(MenuIdentifier closingMenuIdentifier = null)
        {
            if (!IsOpen)
            {
                CustomLogger.LogWarning($"Menu \"{MenuIdentifier}\" is not open.", this);
                return;
            }

            if (contentRoot != null)
            {
                contentRoot.OnFinished -= HandleCloseComplete;
                contentRoot.OnFinished += HandleCloseComplete;

                contentRoot.Hide();
            }
            else
            {
                HandleCloseComplete();
            }

            IsOpen = false;

            return;

            void HandleCloseComplete()
            {
                contentRoot.OnFinished -= HandleCloseComplete;
                contentRoot.SetVisibility(false);
                CleanupMenuState(closingMenuIdentifier);
                OnClosed();
                Closed?.Invoke();
            }
        }

        public void Back()
        {
            Close();
            OnBack();
            BackRequested?.Invoke();
        }

        protected virtual void OnOpened() { }

        protected virtual void OnClosed() { }

        protected virtual void OnBack() { }

        private void RegisterParentMenu(MenuIdentifier parentMenuIdentifier)
        {
            if (parentMenuIdentifier == null)
                return;

            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (!menuManager.TryGetMenu(parentMenuIdentifier, out Menu parentMenu))
            {
                CustomLogger.LogWarning($"Parent menu {parentMenuIdentifier} not found.", this);
                return;
            }

            _parentMenu = parentMenu;
            _parentMenu.RegisterChildMenu(MenuIdentifier);
        }

        private void CleanupMenuState(MenuIdentifier closingMenuIdentifier = null)
        {
            MenuManager menuManager = ServiceLocator.Get<MenuManager>();

            // Close child menus first
            foreach (MenuIdentifier childMenuIdentifier in _childMenuIdentifiers)
                menuManager?.CloseMenu(childMenuIdentifier, MenuIdentifier);

            _childMenuIdentifiers.Clear();

            // Only detach from parent if not closed by it
            if (_parentMenu != null && _parentMenu.MenuIdentifier != closingMenuIdentifier)
                _parentMenu._childMenuIdentifiers.Remove(MenuIdentifier);

            _parentMenu = null;

            menuManager?.DeregisterOpenMenu(this);
        }

        private void RegisterChildMenu(MenuIdentifier childMenuIdentifierToRegister)
        {
            if (childMenuIdentifierToRegister == null)
                return;

            if (!IsOpen)
            {
                CustomLogger.LogError($"Cannot register child menu when \"{MenuIdentifier}\" is not open.", this);
                return;
            }

            if (_childMenuIdentifiers.Contains(childMenuIdentifierToRegister))
            {
                CustomLogger.LogError($"Child menu {childMenuIdentifierToRegister} is already registered.", this);
                return;
            }

            _childMenuIdentifiers.Add(childMenuIdentifierToRegister);
        }
    }
}