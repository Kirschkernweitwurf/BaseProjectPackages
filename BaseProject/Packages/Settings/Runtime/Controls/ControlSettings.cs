using System;
using Base.ServicePackage;
using UnityEngine;

namespace Base.SettingsPackage.Controls
{
    /// <summary>
    /// Holds the control values the player picked, so gameplay code reads them from one place instead of
    /// looking up settings by key. The setting components push into this; nothing here knows about
    /// persistence, which keeps the values usable in a scene that has no settings menu at all.
    /// </summary>
    /// <example>
    /// <code>
    /// if (ServiceLocator.TryGet(out ControlSettings controls))
    ///     _lookDelta = controls.ApplyLook(rawLook);
    /// </code>
    /// </example>
    [DefaultExecutionOrder(ExecutionOrder)]
    public sealed class ControlSettings : GameServiceBehaviour
    {
        private const float DefaultSensitivity = 1f;

        // Low enough that the service exists before any setting component applies its loaded value.
        private const int ExecutionOrder = -97;

        private const float InvertedSign = -1f;
        private const float NormalSign = 1f;

        /// <summary>Raised whenever the sensitivity or one of the invert flags changes.</summary>
        public event Action OnControlsChanged;

        /// <summary>Multiplier applied to raw look input. Starts at one until a setting overrides it.</summary>
        public float LookSensitivity { get; private set; } = DefaultSensitivity;

        /// <summary>Whether left and right are flipped.</summary>
        public bool IsHorizontalInverted { get; private set; }

        /// <summary>Whether up and down are flipped.</summary>
        public bool IsVerticalInverted { get; private set; }

        /// <summary>Sets the multiplier applied to raw look input.</summary>
        /// <param name="sensitivity">The new multiplier.</param>
        public void SetLookSensitivity(float sensitivity)
        {
            if (Mathf.Approximately(LookSensitivity, sensitivity))
                return;

            LookSensitivity = sensitivity;
            OnControlsChanged?.Invoke();
        }

        /// <summary>Flips or unflips one look axis.</summary>
        /// <param name="axis">The axis to change.</param>
        /// <param name="isInverted">Whether that axis should be flipped.</param>
        public void SetInverted(ELookAxis axis, bool isInverted)
        {
            if (axis == ELookAxis.Horizontal)
            {
                if (IsHorizontalInverted == isInverted)
                    return;

                IsHorizontalInverted = isInverted;
            }
            else
            {
                if (IsVerticalInverted == isInverted)
                    return;

                IsVerticalInverted = isInverted;
            }

            OnControlsChanged?.Invoke();
        }

        /// <summary>Applies the sensitivity and both invert flags to a raw look delta.</summary>
        /// <param name="rawLook">The unmodified look input.</param>
        /// <returns>The delta to feed into the camera.</returns>
        public Vector2 ApplyLook(Vector2 rawLook) => new(rawLook.x * LookSensitivity * Sign(IsHorizontalInverted),
            rawLook.y * LookSensitivity * Sign(IsVerticalInverted));

        private static float Sign(bool isInverted) => isInverted
            ? InvertedSign
            : NormalSign;
    }
}