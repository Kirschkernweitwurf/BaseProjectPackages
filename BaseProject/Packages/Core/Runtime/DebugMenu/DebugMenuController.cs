using Base.AttributePackage;
using Base.CorePackage.Input;
using Base.CorePackage.MenuManaging;
using Base.ServicePackage;
using Base.TweeningPackage.Components.System;
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
        [Title("Consoles")]
        [Required] [SerializeField] private Button cheatConsoleButton;
        [Required] [SerializeField] private TweenGroup cheatConsoleTweenGroup;
        [Required] [SerializeField] private Button logConsoleButton;
        [Required] [SerializeField] private TweenGroup logConsoleTweenGroup;
        [Required] [SerializeField] private Menu cheatConsole;
        [Required] [SerializeField] private Menu logConsole;

        private Menu _activeConsole;

#region Unity Callbacks
        private void OnEnable()
        {
            cheatConsoleButton.onClick.AddListener(ShowCheatConsole);
            logConsoleButton.onClick.AddListener(ShowLogConsole);

            // The buttons stay usable even without input, so they are hooked up before the service lookup.
            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            inputManager.BaseInputActions.Permanent.ToggleCheatConsole.performed += OnToggleConsole;
        }

        private void OnDisable()
        {
            cheatConsoleButton.onClick.RemoveListener(ShowCheatConsole);
            logConsoleButton.onClick.RemoveListener(ShowLogConsole);

            if (ServiceLocator.TryGet(out InputManager inputManager))
                inputManager.BaseInputActions.Permanent.ToggleCheatConsole.performed -= OnToggleConsole;
        }
#endregion

        protected override void OnOpened()
        {
            base.OnOpened();

            Menu console = _activeConsole == null
                ? cheatConsole
                : _activeConsole;

            ShowConsole(console);
        }

        private void ShowCheatConsole() => ShowConsole(cheatConsole);

        private void ShowLogConsole() => ShowConsole(logConsole);

        private void ShowConsole(Menu target)
        {
            if (target.IsOpen)
                return;

            _activeConsole = target;
            bool isCheatConsole = target == cheatConsole;

            Menu other = isCheatConsole
                ? logConsole
                : cheatConsole;

            if (other.IsOpen)
                other.Close();

            target.Open(MenuIdentifier);

            if (isCheatConsole)
            {
                cheatConsoleTweenGroup.Show();
                logConsoleTweenGroup.Hide();
            }
            else
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