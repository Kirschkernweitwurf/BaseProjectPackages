using System;
using Base.ServicePackage;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Base.ControllerSupportPackage.InputPrompts.Devices
{
    /// <summary>
    /// Single source of truth for the currently active input device. Listens to raw input events and
    /// flips between mouse/keyboard and gamepad based on real actuation, ignoring noise.
    /// </summary>
    public sealed class InputDeviceTracker : GameServiceBehaviour
    {
        private const float ActivationThreshold = 0.5f;

        /// <summary>Raised whenever the active device family changes.</summary>
        public event Action<EInputDeviceType> OnDeviceChanged;

        /// <summary>The device family that produced the most recent real actuation.</summary>
        public EInputDeviceType CurrentDevice { get; private set; } = EInputDeviceType.Unknown;

        /// <summary>True while the gamepad is the active device family.</summary>
        public bool IsUsingGamepad => CurrentDevice == EInputDeviceType.Gamepad;

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

            // Events from the already active family cannot change anything, so skip the actuation scan.
            // This is the hot path: the active device fires events constantly.
            if (deviceType == EInputDeviceType.Unknown
                || deviceType == CurrentDevice)
                return;

            if (!HasActuation(eventPtr, device))
                return;

            CurrentDevice = deviceType;
            OnDeviceChanged?.Invoke(deviceType);
        }
    }
}