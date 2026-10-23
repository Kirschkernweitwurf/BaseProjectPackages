using Base.AttributesPackage;
using Base.ControllerSupportPackage.InputPrompts.Devices;
using Base.ControllerSupportPackage.InputPrompts.Glyphs;
using Base.ServicesPackage;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.InputPrompts
{
    /// <summary>
    /// Shows the button icon of one action for the device in use, like the A button on a gamepad or Enter on a
    /// keyboard, and follows the player when they switch devices. A prompt for an input only one device has, like
    /// a shoulder button or the Tab key, shows only while that device is in use.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputPromptView : MonoBehaviour
    {
        [Tooltip("The action whose button is shown.")]
        [Required] [SerializeField] private InputActionReference action;

        [Tooltip("Shows the button icon. Auto-assigned from the same GameObject.")]
        [Required] [GetComponent] [SerializeField] private Image icon;

        [Tooltip("Optional. Hidden together with the icon, like the prompt's label. Leave empty to hide only the "
            + "icon.")]
        [SerializeField] private GameObject group;

        [Tooltip("On which devices the prompt shows. Pick one device for inputs the other one does not have.")]
        [SerializeField] private EPromptVisibility shownOn = EPromptVisibility.Always;

        private InputGlyphProvider _glyphs;
        private InputDeviceTracker _devices;

#region Unity Callbacks
        private void Start()
        {
            // TryGet reports a missing service itself; disabling keeps the prompt from asking again.
            if (!ServiceLocator.TryGet(out _glyphs)
                || !ServiceLocator.TryGet(out _devices))
            {
                SetVisible(false);
                enabled = false;
                return;
            }

            _glyphs.OnActiveDeviceChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_glyphs != null)
                _glyphs.OnActiveDeviceChanged -= Refresh;
        }
#endregion

        private void Refresh()
        {
            bool isWanted = shownOn switch
            {
                EPromptVisibility.GamepadOnly => _devices.IsUsingGamepad,
                EPromptVisibility.MouseKeyboardOnly => _devices.CurrentDevice == EInputDeviceType.MouseKeyboard,
                _ => true
            };

            if (!isWanted
                || !_glyphs.TryGetSprite(action, out Sprite sprite))
            {
                SetVisible(false);
                return;
            }

            icon.sprite = sprite;
            SetVisible(true);
        }

        private void SetVisible(bool isVisible)
        {
            if (group == null)
            {
                icon.enabled = isVisible;
                return;
            }

            group.SetActive(isVisible);
        }
    }
}