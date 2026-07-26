using System;
using System.Collections.Generic;
using Base.AttributePackage;
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
        [field: Required] [field: SerializeField] public MenuIdentifier MenuIdentifier { get; private set; }

        [Tooltip("The root TweenGroup for this menu's open/close animations.")]
        [Required] [SerializeField] private TweenGroup contentRoot;

        [field: Tooltip("The priority of this menu in the stack.")]
        [field: SerializeField] public EPriority Priority { get; private set; }

        [Space]
        [Tooltip("If true, this menu will automatically open on Start (with animation).")]
        [SerializeField] private bool openOnStart;

        [field: Tooltip("If true, this menu will listen to the OnBack action to close itself.")]
        [field: SerializeField] public bool ListenToOnBackAction { get; private set; } = true;

        [Tooltip("Menus that block this menu from opening if they are currently open.")]
        [SerializeField] private MenuIdentifier[] blockingMenus;

        /// <summary><c>true</c> if the menu is currently open.</summary>
        public bool IsOpen { get; private set; }

        /// <summary><c>true</c> if the menu has been shut down and is no longer valid.</summary>
        public bool HasShutDown { get; private set; }

        /// <summary>The root tween group driving this menu's open and close animation.</summary>
        public TweenGroup ContentRoot => contentRoot;

        /// <summary><c>true</c> if the menu is currently transitioning between open and closed states.</summary>
        protected bool IsTransitioning { get; private set; }

        private readonly List<MenuIdentifier> _childMenuIdentifiers = new();

        private Menu _parentMenu;

#region Unity Callbacks
        protected virtual void Awake()
        {
            ShutdownManager.Register(this);

            if (ServiceLocator.TryGet(out MenuManager menuManager))
                menuManager.RegisterMenu(this);
        }

        protected virtual void Start()
        {
            if (openOnStart)
            {
                Open();
                return;
            }

            if (!IsOpen)
                contentRoot.SetVisibility(false);
        }

        protected virtual void OnDestroy()
        {
            if (!HasShutDown)
                Shutdown();
        }
#endregion

        /// <summary>
        /// Drops all registrations and clears the events. Runs automatically when the menu is destroyed.
        /// </summary>
        public virtual void Shutdown()
        {
            if (HasShutDown)
                return;

            HasShutDown = true;

            ShutdownManager.Deregister(this);

            if (ServiceLocator.TryGet(out MenuManager menuManager))
                menuManager.DeregisterMenu(this);

            if (IsOpen)
                CleanupMenuState();

            Opened = null;
            Closed = null;
            BackRequested = null;
        }

        /// <summary>
        /// Opens the menu (always animated).
        /// </summary>
        /// <param name="parentMenuIdentifier">
        /// The menu this one is opened from, if any. The parent closes this menu when it closes itself.
        /// </param>
        public void Open(MenuIdentifier parentMenuIdentifier = null)
        {
            if (IsOpen)
            {
                CustomLogger.LogWarning($"Menu \"{MenuIdentifier}\" is already open.", this);
                return;
            }

            if (IsTransitioning)
                return;

            IsTransitioning = true;
            IsOpen = true;

            contentRoot.SetVisibility(true);

            contentRoot.OnFinished -= HandleOpened;
            contentRoot.OnFinished += HandleOpened;
            contentRoot.Show();

            RegisterParentMenu(parentMenuIdentifier);

            if (ServiceLocator.TryGet(out MenuManager menuManager))
                menuManager.RegisterOpenMenu(this, (uint)Priority, this);

            OnOpened();
            Opened?.Invoke();

            return;

            void HandleOpened()
            {
                contentRoot.OnFinished -= HandleOpened;
                IsTransitioning = false;
            }
        }

        /// <summary>
        /// Closes the menu (always animated).
        /// </summary>
        /// <param name="closingMenuIdentifier">
        /// The menu that triggered this close, if any. Used to skip detaching from a parent that is
        /// closing this menu itself.
        /// </param>
        public void Close(MenuIdentifier closingMenuIdentifier = null)
        {
            if (!IsOpen)
            {
                CustomLogger.LogWarning($"Menu \"{MenuIdentifier}\" is not open.", this);
                return;
            }

            if (IsTransitioning)
                return;

            IsTransitioning = true;

            contentRoot.OnFinished -= HandleCloseComplete;
            contentRoot.OnFinished += HandleCloseComplete;
            contentRoot.Hide();

            return;

            void HandleCloseComplete()
            {
                contentRoot.OnFinished -= HandleCloseComplete;

                IsTransitioning = false;
                IsOpen = false;

                contentRoot.SetVisibility(false);

                CleanupMenuState(closingMenuIdentifier);

                OnClosed();
                Closed?.Invoke();
            }
        }

        /// <summary>
        /// Closes the menu in response to a back request.
        /// </summary>
        public void Back()
        {
            Close();
            OnBack();
            BackRequested?.Invoke();
        }

        /// <summary>Runs right after the menu opened, before <see cref="Opened"/> is raised.</summary>
        protected virtual void OnOpened() { }

        /// <summary>Runs once the close animation finished, before <see cref="Closed"/> is raised.</summary>
        protected virtual void OnClosed() { }

        /// <summary>
        /// Runs after a back request closed the menu, before <see cref="BackRequested"/> is raised.
        /// </summary>
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
            bool hasMenuManager = ServiceLocator.TryGet(out MenuManager menuManager);

            // Close child menus first, they cannot outlive their parent.
            if (hasMenuManager)
                foreach (MenuIdentifier childMenuIdentifier in _childMenuIdentifiers)
                    menuManager.CloseMenu(childMenuIdentifier, MenuIdentifier);

            _childMenuIdentifiers.Clear();

            // Only detach from the parent if it is not the one closing this menu.
            if (_parentMenu != null
                && _parentMenu.MenuIdentifier != closingMenuIdentifier)
                _parentMenu._childMenuIdentifiers.Remove(MenuIdentifier);

            _parentMenu = null;

            if (hasMenuManager)
                menuManager.DeregisterOpenMenu(this);
        }

        private void RegisterChildMenu(MenuIdentifier childMenuIdentifierToRegister)
        {
            if (childMenuIdentifierToRegister == null)
            {
                CustomLogger.LogWarning($"Cannot register null child menu for \"{MenuIdentifier}\".", this);
                return;
            }

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