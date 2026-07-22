using Base.AttributePackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Base.ControllerSupport.Controller.Scrolling
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

        [Tooltip("Vector2 action that drives scrolling, e.g. the right stick.")]
        [Required] [SerializeField] private InputActionReference scrollAction;

        [Tooltip("Scroll speed in normalized units per second.")]
        [SerializeField] private float scrollSpeed = 1f;

        [Tooltip("Stick magnitude below this value is ignored.")]
        [MinMax(0f, 1f)]
        [SerializeField] private float deadZone = DefaultDeadZone;

        [Tooltip("If true, the vertical axis is inverted.")]
        [SerializeField] private bool invertVertical;

        private ScrollRect _scrollRect;

#region Unity Callbacks
        private void Awake() => _scrollRect = GetComponent<ScrollRect>();

        private void OnEnable()
        {
            if (scrollAction.action == null)
            {
                CustomLogger.LogWarning("Scroll action is not assigned "
                    + $"in {nameof(GamepadScrollRect)} on {name}.", this);

                return;
            }

            scrollAction.action.Enable();
        }

        private void Update()
        {
            Vector2 input = scrollAction.action.ReadValue<Vector2>();

            if (input.sqrMagnitude < deadZone * deadZone)
                return;

            Apply(input);
        }

        private void OnDisable() => scrollAction?.action?.Disable();
#endregion

        private void Apply(Vector2 input)
        {
            float vertical = invertVertical
                ? -input.y
                : input.y;

            float step = scrollSpeed * Time.unscaledDeltaTime;

            if (_scrollRect.vertical)
                _scrollRect.verticalNormalizedPosition =
                    Mathf.Clamp01(_scrollRect.verticalNormalizedPosition + vertical * step);

            if (_scrollRect.horizontal)
                _scrollRect.horizontalNormalizedPosition =
                    Mathf.Clamp01(_scrollRect.horizontalNormalizedPosition + input.x * step);
        }
    }
}