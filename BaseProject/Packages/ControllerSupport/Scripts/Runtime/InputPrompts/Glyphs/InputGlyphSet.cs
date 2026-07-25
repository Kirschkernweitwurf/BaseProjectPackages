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

            if (action == null || action.action == null)
            {
                CustomLogger.LogWarning($"Action reference is null or has no action assigned. DeviceType={DeviceType}",
                    this);

                return false;
            }

            if (!TryFind(action, out InputGlyphEntry entry))
                return false;

            sprite = entry?.Sprite;
            return sprite != null;
        }

        /// <summary>Tries to resolve the TextMeshPro sprite name for an action on this device.</summary>
        public bool TryGetTmpSpriteName(InputActionReference action, out string spriteName)
        {
            spriteName = string.Empty;

            if (!TryFind(action, out InputGlyphEntry entry))
                return false;

            spriteName = entry.TmpSpriteName;
            if (string.IsNullOrEmpty(spriteName))
            {
                CustomLogger.LogWarning($"No TMP sprite name found for action '{action.action.name}'"
                    + $" (id={action.action.id}) in device type {DeviceType}.", this);

                return false;
            }

            return true;
        }

        private bool TryFind(InputActionReference action, out InputGlyphEntry entry)
        {
            _lookup ??= BuildLookup();
            entry = _lookup.GetValueOrDefault(action.action.id);
            if (entry == null)
            {
                CustomLogger.LogWarning($"No glyph entry found for action '{action.action.name}'"
                    + $" (id={action.action.id}) in device type {DeviceType}.", this);

                return false;
            }

            return true;
        }

        private Dictionary<Guid, InputGlyphEntry> BuildLookup()
        {
            Dictionary<Guid, InputGlyphEntry> lookup = new();

            for (int i = 0; i < entries.Count; i++)
            {
                InputGlyphEntry entry = entries[i];
                if (entry == null)
                {
                    CustomLogger.LogWarning(
                        $"Null entry found in glyph set '{name}' (DeviceType={DeviceType}) at index {i}.", this);

                    continue;
                }

                if (entry.Action == null || entry.Action.action == null)
                {
                    CustomLogger.LogWarning(
                        $"Glyph entry at index {i} in glyph set '{name}' (DeviceType={DeviceType}) has no action assigned.",
                        this);

                    continue;
                }

                lookup[entry.Action.action.id] = entry;
            }

            return lookup;
        }
    }
}