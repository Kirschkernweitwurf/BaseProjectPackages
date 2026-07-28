using System;
using System.Collections.Generic;
using Base.AttributePackage;
using Base.ControllerSupport.InputPrompts.Devices;
using Base.ToolPackage.MenuManagerWindow;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.ControllerSupport.InputPrompts.Glyphs
{
    /// <summary>
    /// A set of action-to-glyph mappings for one device family. Author one asset per device type
    /// (mouse/keyboard, gamepad) and assign them to the <see cref="InputGlyphProvider"/>.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Input/New Glyph Set", "IGS_GlyphSet")]
    public sealed class InputGlyphSet : ScriptableObject
    {
        [field: Tooltip("The device family these glyphs represent.")]
        [field: EnumToggleButtons]
        [field: SerializeField] public EInputDeviceType DeviceType { get; private set; }

        [Tooltip("Action to glyph mappings for this device.")]
        [NotNullOrEmpty]
        [SerializeField] private List<InputGlyphEntry> entries = new();

        private Dictionary<Guid, InputGlyphEntry> _lookup;

#region Unity Callbacks
        private void OnValidate() => _lookup = null;
#endregion

        /// <summary>Tries to resolve the sprite for an action on this device.</summary>
        public bool TryGetSprite(InputActionReference action, out Sprite sprite)
        {
            sprite = null;

            if (!TryFind(action, out InputGlyphEntry entry))
                return false;

            sprite = entry.Sprite;

            if (sprite != null)
                return true;

            CustomLogger.LogWarning($"The glyph entry for action \"{action.action.name}\" has no sprite assigned. "
                + $"Device type: {DeviceType}.", this);

            return false;
        }

        /// <summary>Tries to resolve the TextMeshPro sprite name for an action on this device.</summary>
        public bool TryGetTmpSpriteName(InputActionReference action, out string spriteName)
        {
            spriteName = string.Empty;

            if (!TryFind(action, out InputGlyphEntry entry))
                return false;

            spriteName = entry.TmpSpriteName;

            if (!string.IsNullOrEmpty(spriteName))
                return true;

            CustomLogger.LogWarning($"The glyph entry for action \"{action.action.name}\" has no TMP sprite name. "
                + $"Device type: {DeviceType}.", this);

            return false;
        }

        private bool TryFind(InputActionReference action, out InputGlyphEntry entry)
        {
            entry = null;

            if (action == null)
            {
                CustomLogger.LogWarning($"The action reference is null. Device type: {DeviceType}.", this);
                return false;
            }

            if (action.action == null)
            {
                CustomLogger.LogWarning($"The action reference \"{action.name}\" has no action assigned. "
                    + $"Device type: {DeviceType}.", this);

                return false;
            }

            _lookup ??= BuildLookup();
            entry = _lookup.GetValueOrDefault(action.action.id);

            if (entry != null)
                return true;

            CustomLogger.LogWarning($"No glyph entry found for action \"{action.action.name}\" "
                + $"(id {action.action.id}). Device type: {DeviceType}.", this);

            return false;
        }

        private Dictionary<Guid, InputGlyphEntry> BuildLookup()
        {
            Dictionary<Guid, InputGlyphEntry> lookup = new();

            for (int i = 0; i < entries.Count; i++)
            {
                InputGlyphEntry entry = entries[i];

                if (entry == null)
                {
                    CustomLogger.LogWarning($"Entry {i} in glyph set \"{name}\" is empty. "
                        + $"Device type: {DeviceType}.", this);

                    continue;
                }

                if (entry.Action == null
                    || entry.Action.action == null)
                {
                    CustomLogger.LogWarning($"Entry {i} in glyph set \"{name}\" has no action assigned. "
                        + $"Device type: {DeviceType}.", this);

                    continue;
                }

                lookup[entry.Action.action.id] = entry;
            }

            return lookup;
        }
    }
}