using System;
using System.Collections.Generic;
using Base.ControllerSupport.InputPrompts.Devices;
using Base.ToolPackage.MenuManagerWindow;
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
        [field: SerializeField] public EInputDeviceType DeviceType { get; private set; }

        [Tooltip("Action to glyph mappings for this device.")]
        [SerializeField] private List<InputGlyphEntry> entries = new();

        private Dictionary<Guid, InputGlyphEntry> _lookup;

#region Unity Callbacks
#if UNITY_EDITOR
        private void OnValidate() => _lookup = null;
#endif
#endregion

        /// <summary>Tries to resolve the sprite for an action on this device.</summary>
        public bool TryGetSprite(InputActionReference action, out Sprite sprite)
        {
            InputGlyphEntry entry = Find(action);
            sprite = entry?.Sprite;
            return sprite != null;
        }

        /// <summary>Tries to resolve the TextMeshPro sprite name for an action on this device.</summary>
        public bool TryGetTmpSpriteName(InputActionReference action, out string spriteName)
        {
            InputGlyphEntry entry = Find(action);
            spriteName = entry?.TmpSpriteName;
            return !string.IsNullOrEmpty(spriteName);
        }

        private InputGlyphEntry Find(InputActionReference action)
        {
            if (action == null || action.action == null)
                return null;

            _lookup ??= BuildLookup();
            return _lookup.GetValueOrDefault(action.action.id);
        }

        private Dictionary<Guid, InputGlyphEntry> BuildLookup()
        {
            Dictionary<Guid, InputGlyphEntry> lookup = new();

            foreach (InputGlyphEntry entry in entries)
            {
                if (entry.Action != null && entry.Action.action != null)
                    lookup[entry.Action.action.id] = entry;
            }

            return lookup;
        }
    }
}