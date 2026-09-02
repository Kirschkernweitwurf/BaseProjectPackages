using System;
using System.Collections.Generic;
using Base.AttributesPackage;
using Base.ControllerSupportPackage.InputPrompts.Devices;
using Base.ServicesPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.ControllerSupportPackage.InputPrompts.Glyphs
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

            ValidateGlyphSets();

            // Without the tracker there is no active device, so every lookup would fail anyway.
            if (!ServiceLocator.TryGet(out _deviceTracker))
                return;

            _deviceTracker.OnDeviceChanged += HandleDeviceChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_deviceTracker == null)
                return;

            _deviceTracker.OnDeviceChanged -= HandleDeviceChanged;
        }
#endregion

        /// <summary>Tries to resolve the glyph sprite for an action on the active device.</summary>
        public bool TryGetSprite(InputActionReference actionReference, out Sprite sprite)
        {
            sprite = null;

            return TryResolveActiveSet(actionReference, out InputGlyphSet set)
                && set.TryGetSprite(actionReference, out sprite);
        }

        /// <summary>
        /// Tries to build a TextMeshPro sprite tag for an action, for example
        /// <c>&lt;sprite name="ButtonSouth"&gt;</c>.
        /// </summary>
        public bool TryGetTmpSpriteTag(InputActionReference actionReference, out string spriteTag)
        {
            spriteTag = string.Empty;

            if (!TryResolveActiveSet(actionReference, out InputGlyphSet set))
                return false;

            if (!set.TryGetTmpSpriteName(actionReference, out string spriteName))
                return false;

            spriteTag = CreateTmpSpriteTag(spriteName);
            return true;
        }

        private static string CreateTmpSpriteTag(string spriteName) => $"<sprite name=\"{spriteName}\">";

        // Empty slots are an authoring mistake, so they are reported once here instead of per lookup.
        private void ValidateGlyphSets()
        {
            foreach (InputGlyphSet set in glyphSets)
            {
                if (set == null)
                    CustomLogger.LogError($"An assigned {nameof(InputGlyphSet)} slot is empty.", this);
            }
        }

        // The set itself reports why a mapping failed, so this only logs what the provider alone knows.
        private bool TryResolveActiveSet(InputActionReference actionReference, out InputGlyphSet inputGlyphSet)
        {
            inputGlyphSet = null;

            if (actionReference == null)
            {
                CustomLogger.LogWarning("Cannot resolve a glyph for a null action reference.", this);
                return false;
            }

            if (_deviceTracker == null)
                return false;

            EInputDeviceType device = _deviceTracker.CurrentDevice;

            foreach (InputGlyphSet set in glyphSets)
            {
                if (set == null)
                    continue;

                if (set.DeviceType != device)
                    continue;

                inputGlyphSet = set;
                return true;
            }

            CustomLogger.LogWarning($"No {nameof(InputGlyphSet)} is assigned for device {device}.", this);
            return false;
        }

        private void HandleDeviceChanged(EInputDeviceType deviceType) => OnActiveDeviceChanged?.Invoke();
    }
}