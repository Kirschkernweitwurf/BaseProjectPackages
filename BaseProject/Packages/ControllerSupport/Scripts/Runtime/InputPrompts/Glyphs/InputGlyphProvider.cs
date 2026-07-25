using System;
using System.Collections.Generic;
using Base.AttributePackage;
using Base.ControllerSupport.InputPrompts.Devices;
using Base.CorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.ControllerSupport.InputPrompts.Glyphs
{
    /// <summary>
    /// Resolves the correct prompt glyph for an action based on the active input device. Returns a
    /// sprite for image display or a ready-to-use TextMeshPro sprite tag for inline text. Raises
    /// <see cref="OnActiveDeviceChanged"/> so labels can refresh when the device switches.
    /// </summary>
    public sealed class InputGlyphProvider : GameServiceBehaviour
    {
        /// <summary>Raised when the active device changes and prompts should be refreshed.</summary>
        public event Action OnActiveDeviceChanged;

        [Tooltip("One glyph set per supported device family.")]
        [NotNullOrEmpty]
        [SerializeField] private List<InputGlyphSet> glyphSets = new();

        private InputDeviceTracker _deviceTracker;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            if (ServiceLocator.TryGet(out _deviceTracker))
                _deviceTracker.OnDeviceChanged += HandleDeviceChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_deviceTracker != null)
                _deviceTracker.OnDeviceChanged -= HandleDeviceChanged;
        }
#endregion

        /// <summary>Tries to resolve the glyph sprite for an action on the active device.</summary>
        public bool TryGetSprite(InputActionReference actionReference, out Sprite sprite)
        {
            sprite = null;

            if (actionReference == null)
            {
                CustomLogger.LogWarning("Can't get sprite for null action.", this);
                return false;
            }

            if (!TryResolveActiveSet(out InputGlyphSet set))
            {
                CustomLogger.LogWarning($"Can't get sprite. No active {nameof(InputGlyphSet)} found for device "
                    + $"{_deviceTracker.CurrentDevice} in {nameof(InputGlyphProvider)}.", this);

                return false;
            }

            if (!set.TryGetSprite(actionReference, out sprite))
            {
                CustomLogger.LogWarning($"No sprite found for action {actionReference.action?.name} in "
                    + $"{nameof(InputGlyphSet)} for device {_deviceTracker.CurrentDevice}.", this);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a TextMeshPro sprite tag for an action, e.g. <c>&lt;sprite name="ButtonSouth"&gt;</c>.
        /// Returns an empty string when no glyph is mapped.
        /// </summary>
        public bool TryGetTmpSpriteTag(InputActionReference actionReference, out string spriteTag)
        {
            spriteTag = string.Empty;

            if (actionReference == null)
            {
                CustomLogger.LogWarning("Can't get sprite tag for null action.", this);
                return false;
            }

            if (!TryResolveActiveSet(out InputGlyphSet set))
            {
                CustomLogger.LogWarning($"Can't get TMP sprite tag. No active {nameof(InputGlyphSet)} found for device "
                    + $"{_deviceTracker.CurrentDevice} in {nameof(InputGlyphProvider)}.", this);

                return false;
            }

            if (!set.TryGetTmpSpriteName(actionReference, out string spriteName))
            {
                CustomLogger.LogWarning($"No TMP sprite name found for action {actionReference.action?.name} in "
                    + $"{nameof(InputGlyphSet)} for device {_deviceTracker.CurrentDevice}.", this);

                return false;
            }

            spriteTag = CreateTmpSpriteTag(spriteName);
            return true;
        }

        private static string CreateTmpSpriteTag(string spriteName) => $"<sprite name=\"{spriteName}\">";

        private bool TryResolveActiveSet(out InputGlyphSet inputGlyphSet)
        {
            inputGlyphSet = null;
            EInputDeviceType device = _deviceTracker.CurrentDevice;

            foreach (InputGlyphSet set in glyphSets)
            {
                if (set == null)
                {
                    CustomLogger.LogWarning($"Null {nameof(InputGlyphSet)} found in {nameof(InputGlyphProvider)}.",
                        this);

                    continue;
                }

                if (set.DeviceType == device)
                    return set;
            }

            return false;
        }

        private void HandleDeviceChanged(EInputDeviceType deviceType) => OnActiveDeviceChanged?.Invoke();
    }
}