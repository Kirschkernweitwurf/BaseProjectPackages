using System;
using System.Collections.Generic;
using Base.ControllerSupport.InputPrompts.Devices;
using Base.SystemsCorePackage.Services;
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
        public bool TryGetSprite(InputActionReference action, out Sprite sprite)
        {
            sprite = null;
            InputGlyphSet set = ResolveActiveSet();
            return set != null && set.TryGetSprite(action, out sprite);
        }

        /// <summary>
        /// Returns a TextMeshPro sprite tag for an action, e.g. <c>&lt;sprite name="ButtonSouth"&gt;</c>.
        /// Returns an empty string when no glyph is mapped.
        /// </summary>
        public string GetTmpSpriteTag(InputActionReference action)
        {
            InputGlyphSet set = ResolveActiveSet();

            if (set == null || !set.TryGetTmpSpriteName(action, out string spriteName))
                return string.Empty;

            return $"<sprite name=\"{spriteName}\">";
        }

        private InputGlyphSet ResolveActiveSet()
        {
            EInputDeviceType device = _deviceTracker != null
                ? _deviceTracker.CurrentDevice
                : EInputDeviceType.MouseKeyboard;

            foreach (InputGlyphSet set in glyphSets)
            {
                if (set != null && set.DeviceType == device)
                    return set;
            }

            return null;
        }

        private void HandleDeviceChanged(EInputDeviceType deviceType) => OnActiveDeviceChanged?.Invoke();
    }
}