using Base.CorePackage.Input;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.Services;
using Base.CorePackage.Tweening.Components.System;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Base.CorePackage.DebugMenu
{
    /// <summary>
    /// Debug menu that hosts the cheat console and the log console. It is toggled by input and
    /// switches between the two consoles, opening each one as a child of this menu. The console that
    /// was shown last is remembered and restored the next time the debug menu opens.
    /// </summary>
    public sealed class DebugMenuController : Menu
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Header("Consoles")]

        [SerializeField] private Button cheatConsoleButton;
        [SerializeField] private TweenGroup cheatConsoleTweenGroup;
        [SerializeField] private Button logConsoleButton;
        [SerializeField] private TweenGroup logConsoleTweenGroup;
        [SerializeField] private Menu cheatConsole;
        [SerializeField] private Menu logConsole;

        private Menu _activeConsole;

        protected override void Awake()
        {
            base.Awake();

            if (cheatConsoleButton == null)
                CustomLogger.LogError("Cheat console button reference is missing.", this);

            if (logConsoleButton == null)
                CustomLogger.LogError("Log console button reference is missing.", this);

            if (cheatConsole == null)
                CustomLogger.LogError("Cheat console reference is missing.", this);

            if (logConsole == null)
                CustomLogger.LogError("Log console reference is missing.", this);
        }

        private void OnEnable()
        {
            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            inputManager.BaseInputActions.Permanent.ToggleCheatConsole.performed += OnToggleConsole;

            cheatConsoleButton.onClick.AddListener(ShowCheatConsole);
            logConsoleButton.onClick.AddListener(ShowLogConsole);
        }

        private void OnDisable()
        {
            if (ServiceLocator.TryGet(out InputManager inputManager))
                inputManager.BaseInputActions.Permanent.ToggleCheatConsole.performed -= OnToggleConsole;

            cheatConsoleButton.onClick.RemoveListener(ShowCheatConsole);
            logConsoleButton.onClick.RemoveListener(ShowLogConsole);
        }

        protected override void OnOpened()
        {
            base.OnOpened();

            ShowConsole(_activeConsole == null
                ? cheatConsole
                : _activeConsole);
        }

        private void ShowCheatConsole() => ShowConsole(cheatConsole);

        private void ShowLogConsole() => ShowConsole(logConsole);

        private void ShowConsole(Menu target)
        {
            if (target.IsOpen)
                return;

            _activeConsole = target;

            Menu other = target == cheatConsole
                ? logConsole
                : cheatConsole;

            if (other.IsOpen)
                other.Close();

            if (!target.IsOpen)
                target.Open(MenuIdentifier);

            if (target == cheatConsole)
            {
                cheatConsoleTweenGroup.Show();
                logConsoleTweenGroup.Hide();
            }
            else if (target == logConsole)
            {
                logConsoleTweenGroup.Show();
                cheatConsoleTweenGroup.Hide();
            }
        }

        private void OnToggleConsole(InputAction.CallbackContext _)
        {
            if (IsTransitioning)
                return;

            if (IsOpen)
                Close();
            else
                Open();
        }
#endif
    }
}