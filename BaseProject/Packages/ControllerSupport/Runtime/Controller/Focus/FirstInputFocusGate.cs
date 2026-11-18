using Base.AttributesPackage;
using Base.ControllerSupportPackage.Controller.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.ControllerSupportPackage.Controller.Focus
{
    /// <summary>
    /// Keeps the focus off a screen until the player steers it. Without it the focus watchdog selects the default
    /// element as soon as the screen takes input, so spamming A through the lock screen pressed Play and started a
    /// run. The first press only lights the focus. The group stays the focus owner meanwhile and only holds the focus
    /// empty, so nothing on a screen behind it keeps the selection.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavigableGroup))]
    public sealed class FirstInputFocusGate : MonoBehaviour
    {
        [Required] [GetComponent]
        [Tooltip("The group whose focus stays empty until the first input.")]
        [SerializeField] private NavigableGroup group;

        [Required]
        [Tooltip("The screen that takes the input, like the homescreen. Presses before it is interactable, like the "
            + "one that unlocks the lock screen, do not count.")]
        [SerializeField] private CanvasGroup screen;

        [Required]
        [Tooltip("The navigate action of the UI, like the stick and the arrow keys.")]
        [SerializeField] private InputActionReference navigateAction;

        [Required]
        [Tooltip("The submit action of the UI, like A or Enter.")]
        [SerializeField] private InputActionReference submitAction;

        private bool _hasInput;

#region Unity Callbacks
        // Held from the start, since the menu can open the group before the screen hides itself.
        private void Awake() => group.IsFocusHeld = true;

        private void OnEnable()
        {
            navigateAction.action.performed += OnInput;
            submitAction.action.performed += OnInput;
        }

        // Runs after the UI module, so the press that opens the gate already went to an empty selection and only
        // the focus appears.
        private void LateUpdate()
        {
            if (!_hasInput)
                return;

            group.IsFocusHeld = false;
            enabled = false;
        }

        private void OnDisable()
        {
            navigateAction.action.performed -= OnInput;
            submitAction.action.performed -= OnInput;
        }
#endregion

        private void OnInput(InputAction.CallbackContext context)
        {
            if (screen.interactable)
                _hasInput = true;
        }
    }
}