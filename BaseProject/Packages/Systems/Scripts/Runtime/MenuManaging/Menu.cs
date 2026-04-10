using System.Collections.Generic;
using Attributes;
using Systems.Input;
using Systems.PriorityTrackers;
using Systems.Services;
using Systems.Shutdown;
using Systems.Tweening.Components.System;
using Tracking;
using UnityEngine;
using Utility.Logging;

namespace Systems.MenuManaging
{
    /// <summary>
    /// Base class for all menus in the game. Handles lifecycle, input, cursor, timescale and animations.
    /// </summary>
    public class Menu : MonoBehaviour, IShutdownHandler
    {
        [field: Header("Menu Settings")]
        [Tooltip("The unique identifier for this menu.")]
        [field: SerializeField] public EMenuIdentifier MenuIdentifier { get; private set; }

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
        [InputActionMapName] [SerializeField] private string actionMap;

        [Tooltip("Menus that block this menu from opening if they are currently open.")]
        [SerializeField] private EMenuIdentifier[] blockingMenus;

        public bool IsOpen { get; private set; }

        public bool HasShutDown { get; private set; }

        private readonly List<EMenuIdentifier> _childMenuKeys = new();
        private Menu _parentMenu;

        protected virtual void Awake()
        {
            ShutdownManager.Register(this);

            if (ServiceLocator.TryGet(out MenuManager menuManager))
                menuManager.RegisterMenu(this);

            if (MenuIdentifier == EMenuIdentifier.None)
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
        public void Open(EMenuIdentifier parentMenuIdentifier = EMenuIdentifier.None)
        {
            if (IsOpen)
            {
                CustomLogger.LogWarning($"Menu \"{MenuIdentifier}\" is already open.", this);
                return;
            }

            foreach (EMenuIdentifier blockingMenu in blockingMenus)
            {
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
            contentRoot?.Play();

            RegisterParentMenu(parentMenuIdentifier);
            ApplySystemSettings();
            ServiceLocator.Get<MenuManager>()?.RegisterOpenMenu(this, (uint)menuPriority, this);

            OnOpened();
        }

        /// <summary>
        /// Closes the menu (always animated).
        /// </summary>
        public void Close(EMenuIdentifier closingMenuIdentifier = EMenuIdentifier.None)
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

                contentRoot.Reverse();
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

        private void RegisterParentMenu(EMenuIdentifier parentMenuIdentifier)
        {
            if (parentMenuIdentifier == EMenuIdentifier.None)
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

        private void CleanupMenuState(EMenuIdentifier closingMenuIdentifier = EMenuIdentifier.None)
        {
            MenuManager menuManager = ServiceLocator.Get<MenuManager>();

            // Close child menus first
            foreach (EMenuIdentifier childMenuKey in _childMenuKeys)
                menuManager?.CloseMenu(childMenuKey, MenuIdentifier);

            _childMenuKeys.Clear();

            // Only detach from parent if not closed by it
            if (_parentMenu != null && _parentMenu.MenuIdentifier != closingMenuIdentifier)
                _parentMenu._childMenuKeys.Remove(MenuIdentifier);

            _parentMenu = null;

            if (applyCustomCursorSettings)
                ServiceLocator.Get<CursorManager>()?.CursorTracker.Remove(this);

            if (applyCustomTimeScaleSettings)
                ServiceLocator.Get<TimeScaleManager>()?.TimeScaleTracker.Remove(this);

            if (overrideActionMap && !string.IsNullOrWhiteSpace(actionMap))
                ServiceLocator.Get<InputManager>()?.DeregisterInputMap(this);

            menuManager?.DeregisterOpenMenu(this);
        }

        private void ApplySystemSettings()
        {
            if (applyCustomCursorSettings)
                ServiceLocator.Get<CursorManager>()?.CursorTracker.Add(cursorSettings, (uint)menuPriority, this);

            if (applyCustomTimeScaleSettings)
                ServiceLocator.Get<TimeScaleManager>()?.TimeScaleTracker.Add(timeScale, (uint)menuPriority, this);

            if (overrideActionMap && !string.IsNullOrWhiteSpace(actionMap))
                ServiceLocator.Get<InputManager>()?.RegisterInputMap(actionMap, this, (uint)menuPriority);
        }

        private void RegisterChildMenu(EMenuIdentifier childMenuIdentifierToRegister)
        {
            if (childMenuIdentifierToRegister == EMenuIdentifier.None)
                return;

            if (!IsOpen)
            {
                CustomLogger.LogError($"Cannot register child menu when \"{MenuIdentifier}\" is not open.", this);
                return;
            }

            if (_childMenuKeys.Contains(childMenuIdentifierToRegister))
            {
                CustomLogger.LogError($"Child menu {childMenuIdentifierToRegister} is already registered.", this);
                return;
            }

            _childMenuKeys.Add(childMenuIdentifierToRegister);
        }
    }
}