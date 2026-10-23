using System;
using Base.ServicesPackage;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Base.ControllerSupportPackage.InputPrompts.Devices
{
    /// <summary>
    /// Single source of truth for the currently active input device. Listens to raw input events and
    /// flips between mouse/keyboard and gamepad based on real actuation, ignoring noise. For a gamepad it also
    /// knows the <see cref="EGamepadFamily"/>, taken from the gamepad that was actually pressed, so prompts can
    /// match the buttons printed on it.
    /// </summary>
    public sealed class InputDeviceTracker : GameServiceBehaviour
    {
        private const float ActivationThreshold = 0.5f;

        private int _lastGamepadId = InputDevice.InvalidDeviceId;

        /// <summary>
        /// Raised whenever the active device family changes, also when the player switches to another kind of
        /// gamepad.
        /// </summary>
        public event Action<EInputDeviceType> OnDeviceChanged;

        /// <summary>The device family that produced the most recent real actuation.</summary>
        public EInputDeviceType CurrentDevice { get; private set; } = EInputDeviceType.Unknown;

        /// <summary>True while the gamepad is the active device family.</summary>
        public bool IsUsingGamepad => CurrentDevice == EInputDeviceType.Gamepad;

        /// <summary>The kind of the gamepad pressed last. Generic until a gamepad was used.</summary>
        public EGamepadFamily CurrentGamepadFamily { get; private set; } = EGamepadFamily.Generic;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();
            InputSystem.onEvent += HandleInputEvent;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            InputSystem.onEvent -= HandleInputEvent;
        }
#endregion

        private static EInputDeviceType ResolveDeviceType(InputDevice device) => device switch
        {
            Gamepad => EInputDeviceType.Gamepad,
            Mouse or Keyboard => EInputDeviceType.MouseKeyboard,
            _ => EInputDeviceType.Unknown
        };

        // At least one control has to cross the threshold, so resting sticks do not flip the device.
        private static bool HasActuation(InputEventPtr eventPtr, InputDevice device)
        {
            foreach (InputControl _ in eventPtr.EnumerateChangedControls(device, ActivationThreshold))
                return true;

            return false;
        }

        private void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // Only state changes carry actuation. Anything else is noise we ignore.
            if (!eventPtr.IsA<StateEvent>()
                && !eventPtr.IsA<DeltaStateEvent>())
                return;

            EInputDeviceType deviceType = ResolveDeviceType(device);

            // Events from the already active device cannot change anything, so skip the actuation scan.
            // This is the hot path: the active device fires events constantly.
            if (deviceType == EInputDeviceType.Unknown
                || IsCurrent(deviceType, device))
                return;

            if (!HasActuation(eventPtr, device))
                return;

            if (deviceType == EInputDeviceType.Gamepad)
            {
                _lastGamepadId = device.deviceId;
                CurrentGamepadFamily = GamepadFamilyDetector.Detect(device.description.product,
                    device.description.manufacturer, device.layout, device.name);
            }

            CurrentDevice = deviceType;
            OnDeviceChanged?.Invoke(deviceType);
        }

        // A second gamepad counts as a change, since it may be another kind with other buttons.
        private bool IsCurrent(EInputDeviceType deviceType, InputDevice device) => deviceType == CurrentDevice
            && (deviceType != EInputDeviceType.Gamepad
                || device.deviceId == _lastGamepadId);
    }
}