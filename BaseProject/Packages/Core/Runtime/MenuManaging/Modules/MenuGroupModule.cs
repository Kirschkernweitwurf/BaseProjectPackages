using System;
using System.Collections.Generic;
using Base.CorePackage.MenuManaging.Identifier;
using Base.ServicesPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Turns the owning menu into a container of tabs: menus shown inside it one at a time, so opening one tab
    /// closes the others. The container opens its start tab by itself and takes the tabs down when it closes.
    /// A tab may hold a group of its own, which nests groups, e.g. settings tabs inside a phone app.
    /// </summary>
    /// <remarks>
    /// The group keeps a wanted tab and moves the real menus towards it, retrying every frame while a menu still
    /// animates. Menus ignore open and close calls during their animation, so this is what makes fast clicking
    /// and switching during a close safe.
    /// </remarks>
    public sealed class MenuGroupModule : MenuModule
    {
        private const int NextStep = 1;
        private const int PreviousStep = -1;

        /// <summary>Raised when the selected tab changes. Passes the identifier of the new tab.</summary>
        public event Action<MenuIdentifier> ActiveTabChanged;

        private readonly Dictionary<Menu, (Action opened, Action closed)> _tabHandlers = new();
        private readonly List<Menu> _tabMenus = new();

        [Tooltip("The menus shown as tabs inside this menu. The order is the order of the next and previous "
            + "tab actions.")]
        [SerializeField] private MenuIdentifier[] tabs;

        [Tooltip("The tab that opens together with this menu. Leave empty to use the first tab.")]
        [SerializeField] private MenuIdentifier defaultTab;

        [Tooltip("If true, this menu reopens on the tab that was open last instead of the default tab.")]
        [SerializeField] private bool rememberLastTab;

        [Tooltip("What happens when the open tab is closed, e.g. by the Back button.")]
        [SerializeField] private EMenuGroupBackBehavior backBehavior;

        [Tooltip("If true, the next tab action on the last tab goes to the first one, and the previous tab action "
            + "on the first tab goes to the last one.")]
        [SerializeField] private bool wrapAround = true;

        [Tooltip("Optional. Selects the next tab, e.g. a shoulder button. Only the innermost open group reacts, and "
            + "only while its screen is on top.")]
        [SerializeField] private InputActionReference nextTabAction;

        [Tooltip("Optional. Selects the previous tab. Only the innermost open group reacts, and only while its "
            + "screen is on top.")]
        [SerializeField] private InputActionReference previousTabAction;

        private Menu _activeTab;
        private Menu _defaultTabMenu;
        private Menu _deepLinkTab;
        private MenuManager _menuManager;
        private bool _isInitialized;
        private bool _isSyncing;

        /// <summary>The identifier of the selected tab, or <c>null</c> before a tab was selected.</summary>
        public MenuIdentifier ActiveTab => _activeTab == null
            ? null
            : _activeTab.MenuIdentifier;

#region Unity Callbacks
        private void Start() => TryInitialize();

        private void Update()
        {
            if (!_isSyncing)
                return;

            SyncTabs();
        }

        private void OnDestroy()
        {
            UnsubscribeFromInput();

            foreach (KeyValuePair<Menu, (Action opened, Action closed)> pair in _tabHandlers)
            {
                // Tabs can be destroyed before the group on a scene unload.
                if (pair.Key == null)
                    continue;

                pair.Key.Opened -= pair.Value.opened;
                pair.Key.Closed -= pair.Value.closed;
            }

            _tabHandlers.Clear();
        }
#endregion

        /// <summary>
        /// Selects the given tab and closes the open one. Opens this menu first when it is closed, so a button
        /// anywhere can jump straight to a tab.
        /// </summary>
        /// <param name="tab">The identifier of one of this group's tabs.</param>
        public void Select(MenuIdentifier tab)
        {
            if (tab == null)
            {
                CustomLogger.LogWarning("Cannot select a tab: identifier is null.", this);
                return;
            }

            if (!TryInitialize())
                return;

            if (!TryGetTabMenu(tab, out Menu tabMenu))
            {
                CustomLogger.LogWarning($"\"{tab}\" is not a tab of \"{OwnerMenu.MenuIdentifier}\".", this);
                return;
            }

            if (!OwnerMenu.IsOpen)
            {
                OpenOwnerOn(tabMenu);
                return;
            }

            SetActiveTab(tabMenu);
        }

        protected override void OnMenuOpened()
        {
            if (!TryInitialize())
                return;

            SubscribeToInput();

            Menu startTab = _deepLinkTab != null
                ? _deepLinkTab
                : GetStartTab();

            SetActiveTab(startTab);
        }

        protected override void OnMenuClosed()
        {
            UnsubscribeFromInput();

            // Children close with this menu already. This catches tabs opened without it as parent.
            _isSyncing = true;
            SyncTabs();
        }

        /// <summary>
        /// Resolves the tabs once. Runs from <c>Start</c> and again from every entry point, because the owning
        /// menu can open on start before this module's <c>Start</c> ran.
        /// </summary>
        /// <returns><c>true</c> when the group is ready to use.</returns>
        private bool TryInitialize()
        {
            if (_isInitialized)
                return true;

            if (!ServiceLocator.TryGet(out _menuManager))
            {
                // Turning off also drops the menu subscriptions, so the missing manager is reported only once.
                enabled = false;
                return false;
            }

            ResolveTabs(_menuManager);
            _defaultTabMenu = ResolveDefaultTab();
            SubscribeToTabs();

            _isInitialized = true;

            return true;
        }

        private void ResolveTabs(MenuManager menuManager)
        {
            if (tabs == null)
                return;

            foreach (MenuIdentifier identifier in tabs)
            {
                if (identifier == null)
                {
                    CustomLogger.LogWarning($"\"{OwnerMenu.MenuIdentifier}\" has an empty tab entry.", this);
                    continue;
                }

                if (identifier == OwnerMenu.MenuIdentifier)
                {
                    CustomLogger.LogError($"\"{identifier}\" cannot be a tab of itself.", this);
                    continue;
                }

                if (!menuManager.TryGetMenu(identifier, out Menu tabMenu))
                {
                    CustomLogger.LogError($"Tab \"{identifier}\" of \"{OwnerMenu.MenuIdentifier}\" not found.",
                        this);

                    continue;
                }

                if (_tabMenus.Contains(tabMenu))
                {
                    CustomLogger.LogWarning($"Tab \"{identifier}\" is listed twice.", this);
                    continue;
                }

                // Back goes to the open menu with the highest priority, so a lower tab would never receive it.
                if (tabMenu.Priority < OwnerMenu.Priority)
                    CustomLogger.LogWarning($"Tab \"{identifier}\" has a lower priority than "
                        + $"\"{OwnerMenu.MenuIdentifier}\", so Back skips the tab.", this);

                _tabMenus.Add(tabMenu);
            }
        }

        private Menu ResolveDefaultTab()
        {
            Menu firstTab = _tabMenus.Count > 0
                ? _tabMenus[0]
                : null;

            if (defaultTab == null)
                return firstTab;

            if (TryGetTabMenu(defaultTab, out Menu defaultTabMenu))
                return defaultTabMenu;

            CustomLogger.LogError($"Default tab \"{defaultTab}\" is not in the tab list, using the first tab.",
                this);

            return firstTab;
        }

        private void SubscribeToTabs()
        {
            foreach (Menu tabMenu in _tabMenus)
            {
                (Action opened, Action closed) handlers = (() => HandleTabOpened(tabMenu),
                    () => HandleTabClosed(tabMenu));

                tabMenu.Opened += handlers.opened;
                tabMenu.Closed += handlers.closed;

                _tabHandlers.Add(tabMenu, handlers);
            }
        }

        private bool TryGetTabMenu(MenuIdentifier identifier, out Menu tabMenu)
        {
            foreach (Menu candidate in _tabMenus)
            {
                if (candidate == null
                    || candidate.MenuIdentifier != identifier)
                    continue;

                tabMenu = candidate;
                return true;
            }

            tabMenu = null;
            return false;
        }

        /// <summary>
        /// Opens this menu on the given tab instead of the start tab. <see cref="Menu.Open"/> raises its event
        /// right away, so the deep link only has to live for the duration of the call.
        /// </summary>
        private void OpenOwnerOn(Menu tabMenu)
        {
            _deepLinkTab = tabMenu;
            OwnerMenu.Open();
            _deepLinkTab = null;
        }

        private void SubscribeToInput()
        {
            SubscribeToAction(nextTabAction, HandleNextTabPerformed);
            SubscribeToAction(previousTabAction, HandlePreviousTabPerformed);
        }

        private Menu GetStartTab() => rememberLastTab && _activeTab != null
            ? _activeTab
            : _defaultTabMenu;

        private void SetActiveTab(Menu tabMenu)
        {
            if (tabMenu != _activeTab)
            {
                _activeTab = tabMenu;
                ActiveTabChanged?.Invoke(ActiveTab);
            }

            _isSyncing = true;
            SyncTabs();
        }

        /// <summary>
        /// Moves the real menus one step towards the wanted state: only the active tab open, and no tab at all
        /// while this menu is closed or closing. The old tab finishes closing before the new one opens, so two
        /// tabs never show at once.
        /// </summary>
        private void SyncTabs()
        {
            Menu targetTab = OwnerMenu.IsOpen && !OwnerMenu.IsClosing
                ? _activeTab
                : null;

            bool isWaiting = false;

            foreach (Menu tabMenu in _tabMenus)
            {
                if (tabMenu == null
                    || tabMenu == targetTab
                    || !tabMenu.IsOpen)
                    continue;

                // A tab that still animates ignores the call, so it is retried next frame.
                if (!tabMenu.IsTransitioning)
                    tabMenu.Close();

                isWaiting = true;
            }

            if (isWaiting)
                return;

            // Selected again while it was closing, so reopen it once the close is done.
            if (targetTab != null
                && targetTab.IsClosing)
                return;

            _isSyncing = false;

            if (targetTab != null
                && !targetTab.IsOpen)
                targetTab.Open(OwnerMenu.MenuIdentifier, isEmbedded: true);
        }

        private void HandleTabOpened(Menu tabMenu)
        {
            // Opened from outside while this menu is closed, e.g. by an Open Menu Button somewhere else.
            if (!OwnerMenu.IsOpen)
            {
                OpenOwnerOn(tabMenu);
                return;
            }

            // A tab opened from outside joins this menu's screen, like one the group opened itself.
            tabMenu.EmbedIn(OwnerMenu);

            if (tabMenu == _activeTab)
                return;

            SetActiveTab(tabMenu);
        }

        /// <summary>
        /// Reacts to the active tab closing without the group asking for it, mostly through Back or a Close Menu
        /// Button on the tab. Closes started by the group itself are part of a switch and are skipped.
        /// </summary>
        private void HandleTabClosed(Menu tabMenu)
        {
            if (_isSyncing
                || tabMenu != _activeTab)
                return;

            if (!OwnerMenu.IsOpen
                || OwnerMenu.IsClosing)
                return;

            if (backBehavior == EMenuGroupBackBehavior.ReturnToDefaultTab
                && _defaultTabMenu != null
                && tabMenu != _defaultTabMenu)
            {
                SetActiveTab(_defaultTabMenu);
                return;
            }

            OwnerMenu.Close();
        }

        private void HandleNextTabPerformed(InputAction.CallbackContext context) => StepIfInnermost(NextStep);

        private void HandlePreviousTabPerformed(InputAction.CallbackContext context)
            => StepIfInnermost(PreviousStep);

        /// <summary>
        /// Steps through the tabs, unless the active tab holds an open group itself. Nested groups may share the
        /// same actions this way, and the innermost one wins. A screen opened on top, like a dialog, keeps the tabs
        /// behind it still.
        /// </summary>
        private void StepIfInnermost(int step)
        {
            if (_tabMenus.Count == 0
                || !_menuManager.IsScreenOnTop(OwnerMenu))
                return;

            if (_activeTab != null
                && _activeTab.IsOpen
                && _activeTab.TryGetComponent(out MenuGroupModule _))
                return;

            int index = _tabMenus.IndexOf(_activeTab) + step;

            if (wrapAround)
                index = (index % _tabMenus.Count + _tabMenus.Count) % _tabMenus.Count;
            else if (index < 0
                || index >= _tabMenus.Count)
                return;

            SetActiveTab(_tabMenus[index]);
        }

        private void UnsubscribeFromInput()
        {
            UnsubscribeFromAction(nextTabAction, HandleNextTabPerformed);
            UnsubscribeFromAction(previousTabAction, HandlePreviousTabPerformed);
        }

        private void SubscribeToAction(InputActionReference reference, Action<InputAction.CallbackContext> handler)
        {
            if (reference == null
                || reference.action == null)
                return;

            // Never disabled again: nested groups can share the action, and closing the inner group must not
            // switch it off for the outer one.
            reference.action.Enable();
            reference.action.performed -= handler;
            reference.action.performed += handler;
        }

        private void UnsubscribeFromAction(InputActionReference reference,
            Action<InputAction.CallbackContext> handler)
        {
            if (reference == null
                || reference.action == null)
                return;

            reference.action.performed -= handler;
        }
    }
}