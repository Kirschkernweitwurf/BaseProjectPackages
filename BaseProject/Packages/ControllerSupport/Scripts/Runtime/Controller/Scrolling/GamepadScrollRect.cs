using Base.AttributePackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Controller.Scrolling
{
    /// <summary>
    /// Lets a stick (typically the right stick) scroll a <see cref="ScrollRect"/> directly. Reads a
    /// Vector2 action and applies it to the normalized scroll position using unscaled time, so it keeps
    /// working while menus pause the game.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class GamepadScrollRect : MonoBehaviour
    {
        private const float DefaultDeadZone = 0.15f;

        [Tooltip("The scroll view this component drives. Auto-assigned from the same GameObject.")]
        [GetComponent]
        [Required]
        [SerializeField] private ScrollRect scrollRect;

        [Tooltip("Vector2 action that drives scrolling, e.g. the right stick.")]
        [Required]
        [SerializeField] private InputActionReference scrollAction;

        [Tooltip("Scroll speed in normalized units per second.")]
        [Min(0)]
        [SerializeField] private float scrollSpeed = 1f;

        [Tooltip("Stick magnitude below this value is ignored.")]
        [MinMax(0f, 1f)]
        [SerializeField] private float deadZone = DefaultDeadZone;

        [Tooltip("If true, the vertical axis is inverted.")]
        [SerializeField] private bool invertVertical;

        private InputAction _scroll;

#region Unity Callbacks
        private void Awake()
        {
            _scroll = scrollAction.action;

            if (_scroll != null)
                return;

            // Disabling here also skips OnEnable, so the action is never touched while unresolved.
            CustomLogger.LogError($"\"{nameof(scrollAction)}\" resolves to no action, scrolling is off.", this);
            enabled = false;
        }

        private void OnEnable() => _scroll.Enable();

        private void Update()
        {
            Vector2 input = _scroll.ReadValue<Vector2>();

            if (input.sqrMagnitude < deadZone * deadZone)
                return;

            Apply(input);
        }

        private void OnDisable() => _scroll.Disable();
#endregion

        private void Apply(Vector2 input)
        {
            float vertical = invertVertical
                ? -input.y
                : input.y;

            float step = scrollSpeed * Time.unscaledDeltaTime;

            if (scrollRect.vertical)
                scrollRect.verticalNormalizedPosition =
                    Mathf.Clamp01(scrollRect.verticalNormalizedPosition + vertical * step);

            if (scrollRect.horizontal)
                scrollRect.horizontalNormalizedPosition =
                    Mathf.Clamp01(scrollRect.horizontalNormalizedPosition + input.x * step);
        }
    }
}