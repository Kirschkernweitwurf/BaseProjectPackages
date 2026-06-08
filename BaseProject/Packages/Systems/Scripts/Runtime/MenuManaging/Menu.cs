using System.Collections.Generic;
using Base.AttributePackage;
using Base.SystemsCorePackage.Input;
using Base.SystemsCorePackage.MenuManaging.Identifier;
using Base.SystemsCorePackage.PriorityTrackers;
using Base.SystemsCorePackage.Services;
using Base.SystemsCorePackage.Services.Shutdown;
using Base.SystemsCorePackage.Tweening.Components.System;
using Base.SystemsCorePackage.Tracking;
using UnityEngine;
using Base.UtilityPackage.Logging;

namespace Base.SystemsCorePackage.MenuManaging
{
    /// <summary>
    /// Base class for all menus in the game. Handles lifecycle, input, cursor, timescale and animations.
    /// </summary>
    public class Menu : MonoBehaviour, IShutdownHandler
    {
        [field: Header("Menu Settings")]
        [Tooltip("The unique identifier asset for this menu.")]
        [field: SerializeField] public MenuIdentifier MenuIdentifier { get; private set; }

        [Tooltip("The priority of this menu in the stack.")]
        [SerializeField] private EPriority menuPriority;

        [Tooltip("The root TweenGroup for this menu's open/close animations.")]
        [SerializeField] private TweenGroup contentRoot;

        [Space]
        [Tooltip("If true, this menu will automatically open on Start (with animation).")]
        [SerializeField] private bool openOnStart;

        [field: Tooltip("If true, this menu will listen to the OnBack action to close itself.")]
        [field: SerializeField] public bool ListenToOnBackAction { get; private set; } = true;

        [Header("Cursor Settings")]
        [Tooltip("If true, this menu will apply custom cursor settings when opened.")]
        [SerializeField] private bool applyCustomCursorSettings;

        [Tooltip("The cursor settings to apply when this menu is opened.")]
        [SerializeField] private CursorRequest cursorSettings = new();

        [Header("Time Scale Settings")]
        [Tooltip("If true, this menu will apply custom time scale settings when opened.")]
        [SerializeField] private bool applyCustomTimeScaleSettings;

        [Tooltip("The time scale to apply when this menu is opened.")]
        [SerializeField] private float timeScale;

        [Header("Input Map")]
        [Tooltip("If true, this menu will override the current action map when opened.")]
        [SerializeField] private bool overrideActionMap;

        [Tooltip("The action map to switch to when this menu is opened.")]
        [SerializeField] private InputActionMapReference actionMap;

        [Tooltip("Menus that block this menu from opening if they are currently open.")]
        [SerializeField] private MenuIdentifier[] blockingMenus;

        public bool IsOpen { get; private set; }

        public bool HasShutDown { get; private set; }

        private readonly List<MenuIdentifier> _childMenuIdentifiers = new();

        private Menu _parentMenu;
        private IMenuResettable[] _resettables;

        protected virtual void Awake()
        {
            ShutdownManager.Register(this);

            if (ServiceLocator.TryGet(out MenuManager menuManager))
                menuManager.RegisterMenu(this);

            CacheResettables();

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

        public virtual void Shutdown()
        {
            ShutdownManager.Deregister(this);

            if (ServiceLocator.TryGet(out MenuManager menuManager))
                menuManager.DeregisterMenu(this);

            HasShutDown = true;

            if (IsOpen)
                CleanupMenuState();
        }

        protected virtual void OnDestroy()
        {
            if (!HasShutDown)
                Shutdown();
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

                CustomLogger.LogWarning($"Cannot open menu \"{MenuIdentifier}\" because blocking" +
                                        $" menu \"{blockingMenu}\" is open.", this);
                return;
            }

            IsOpen = true;

            contentRoot.SetVisibility(true);
            contentRoot?.Show();

            RegisterParentMenu(parentMenuIdentifier);
            ApplySystemSettings();
            ServiceLocator.Get<MenuManager>()?.RegisterOpenMenu(this, (uint)menuPriority, this);

            OnOpened();
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
                ResetChildren();
                CleanupMenuState(closingMenuIdentifier);
                OnClosed();
            }
        }

        public void Back()
        {
            Close();
            OnBack();
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

            if (applyCustomCursorSettings)
                ServiceLocator.Get<CursorManager>()?.CursorTracker.Remove(this);

            if (applyCustomTimeScaleSettings)
                ServiceLocator.Get<TimeScaleManager>()?.TimeScaleTracker.Remove(this);

            if (overrideActionMap && actionMap.IsValid)
                ServiceLocator.Get<InputManager>()?.DeregisterInputMap(this);

            menuManager?.DeregisterOpenMenu(this);
        }

        private void ApplySystemSettings()
        {
            if (applyCustomCursorSettings)
                ServiceLocator.Get<CursorManager>()?.CursorTracker.Add(cursorSettings, (uint)menuPriority, this);

            if (applyCustomTimeScaleSettings)
                ServiceLocator.Get<TimeScaleManager>()?.TimeScaleTracker.Add(timeScale, (uint)menuPriority, this);

            if (overrideActionMap && actionMap.IsValid)
                ServiceLocator.Get<InputManager>()?.RegisterInputMap(actionMap, this, (uint)menuPriority);
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

        private void CacheResettables()
        {
            IMenuResettable[] found = GetComponentsInChildren<IMenuResettable>(includeInactive: true);
            List<IMenuResettable> filtered = new(found.Length);

            foreach (IMenuResettable resettable in found)
            {
                // Skip the content root: its open/close animation is driven by the menu itself.
                if (ReferenceEquals(resettable, contentRoot))
                    continue;

                filtered.Add(resettable);
            }

            _resettables = filtered.ToArray();
        }

        private void ResetChildren()
        {
            foreach (IMenuResettable resettable in _resettables)
                resettable?.ResetState();
        }
    }
}