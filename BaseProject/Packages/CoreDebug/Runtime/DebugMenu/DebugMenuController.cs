using Base.AttributesPackage;
using Base.CorePackage.Input;
using Base.CorePackage.MenuManaging;
using Base.ServicesPackage;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Base.CoreDebugPackage.DebugMenu
{
    /// <summary>
    /// Debug menu that hosts the cheat console and the log console. It is toggled by input and
    /// switches between the two consoles, opening each one as a child of this menu. The tabs work like a
    /// segmented control: the open console's tab is filled and bold. The console that was shown last is
    /// remembered and restored the next time the debug menu opens.
    /// </summary>
    public sealed class DebugMenuController : Menu
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Title("Consoles")]
        [Required] [SerializeField] private Button cheatConsoleButton;
        [Required] [SerializeField] private Button logConsoleButton;
        [Required] [SerializeField] private Menu cheatConsole;
        [Required] [SerializeField] private Menu logConsole;

        [Title("Tabs")]
        [Tooltip("Fill of the open console's tab.")]
        [SerializeField] private Color openTabColor = new(1f, 1f, 1f, 0.18f);

        [Tooltip("Fill of the other tab.")]
        [SerializeField] private Color closedTabColor = new(1f, 1f, 1f, 0f);

        [Tooltip("Label of the open console's tab.")]
        [SerializeField] private Color openLabelColor = Color.white;

        [Tooltip("Label of the other tab.")]
        [SerializeField] private Color closedLabelColor = new(0.557f, 0.557f, 0.576f);

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

            // Optional on the way out. Disabling runs on a scene unload, where the input service may
            // already be gone, and reporting that would fill the console on every scene change.
            if (ServiceLocator.TryGetOptional(out InputManager inputManager))
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
            MarkTab(cheatConsoleButton, isCheatConsole);
            MarkTab(logConsoleButton, !isCheatConsole);
        }

        private void MarkTab(Button tab, bool isOpen)
        {
            tab.targetGraphic.color = isOpen
                ? openTabColor
                : closedTabColor;

            TMP_Text label = tab.GetComponentInChildren<TMP_Text>();
            if (label == null)
                return;

            label.color = isOpen
                ? openLabelColor
                : closedLabelColor;
            label.fontStyle = isOpen
                ? FontStyles.Bold
                : FontStyles.Normal;
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