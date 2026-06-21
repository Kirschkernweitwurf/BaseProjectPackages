using System;
using Base.SystemsCorePackage.Services;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Base.ControllerSupport.Devices
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

        public EInputDeviceType CurrentDevice { get; private set; } = EInputDeviceType.Unknown;

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

        private void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // Only state changes carry actuation. Anything else is noise we ignore.
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;

            EInputDeviceType deviceType = ResolveDeviceType(device);

            if (deviceType == EInputDeviceType.Unknown)
                return;

            // Require at least one control to cross the threshold so resting sticks do not trigger.
            foreach (InputControl _ in eventPtr.EnumerateChangedControls(device, ActivationThreshold))
            {
                SetCurrentDevice(deviceType);
                return;
            }
        }

        private void SetCurrentDevice(EInputDeviceType deviceType)
        {
            if (CurrentDevice == deviceType)
                return;

            CurrentDevice = deviceType;
            OnDeviceChanged?.Invoke(deviceType);
        }
    }
}