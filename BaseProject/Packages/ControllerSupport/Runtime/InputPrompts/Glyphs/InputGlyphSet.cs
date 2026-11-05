using System;
using System.Collections.Generic;
using Base.AttributesPackage;
using Base.ControllerSupportPackage.InputPrompts.Devices;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.ControllerSupportPackage.InputPrompts.Glyphs
{
    /// <summary>
    /// A set of action-to-glyph mappings for one device family. Author one asset per device type
    /// (mouse/keyboard, gamepad) and one per gamepad family (Xbox, PlayStation, ...) that should show its own
    /// buttons, and assign them to the <see cref="InputGlyphProvider"/>.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Input/New Glyph Set", "IGS_GlyphSet")]
    public sealed class InputGlyphSet : ScriptableObject
    {
        private const string GamepadLayout = "Gamepad";
        private const string KeyboardLayout = "Keyboard";
        private const string KeyboardPathStart = "<Keyboard>/";
        private const string MouseLayout = "Mouse";

        [field: Tooltip("The device family these glyphs represent.")]
        [field: EnumToggleButtons]
        [field: SerializeField] public EInputDeviceType DeviceType { get; private set; }

        [field: Tooltip("The kind of gamepad these glyphs show. Only read for gamepad sets. The Generic set stands in "
            + "for every gamepad without a set of its own.")]
        [field: SerializeField] public EGamepadFamily GamepadFamily { get; private set; }

        [Tooltip("Action to glyph mappings for this device.")]
        [NotNullOrEmpty]
        [SerializeField] private List<InputGlyphEntry> entries = new();

        [Tooltip("Glyphs by control path. An action the player has rebound shows the glyph of its new control instead "
            + "of its entry above. Leave empty in a game without rebinding.")]
        [SerializeField] private List<InputControlGlyph> controlGlyphs = new();

        private Dictionary<Guid, InputGlyphEntry> _lookup;
        private Dictionary<string, Sprite> _controlLookup;

#region Unity Callbacks
        private void OnValidate()
        {
            _lookup = null;
            _controlLookup = null;
        }
#endregion

        /// <summary>Tries to resolve the sprite for an action on this device.</summary>
        public bool TryGetSprite(InputActionReference action, out Sprite sprite)
        {
            if (TryGetReboundSprite(action, out sprite))
                return true;

            if (!TryFind(action, out InputGlyphEntry entry))
                return false;

            sprite = entry.Sprite;

            if (sprite != null)
                return true;

            CustomLogger.LogWarning($"The glyph entry for action \"{action.action.name}\" has no sprite assigned. "
                + $"Device type: {DeviceType}.", this);

            return false;
        }

        /// <summary>Tries to resolve the sprite of one control on this device, by its path.</summary>
        /// <param name="controlPath">Path of the control, like &lt;Keyboard&gt;/g.</param>
        /// <param name="sprite">The sprite, or null if this set has none for the control.</param>
        /// <returns><c>true</c> if this set has a sprite for the control.</returns>
        public bool TryGetControlSprite(string controlPath, out Sprite sprite)
        {
            _controlLookup ??= BuildControlLookup();

            sprite = null;

            if (string.IsNullOrEmpty(controlPath))
                return false;

            return _controlLookup.TryGetValue(GetLabeledPath(controlPath), out sprite)
                || _controlLookup.TryGetValue(controlPath, out sprite);
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

        // Keys bind by position, so on a German keyboard <Keyboard>/z is the key labeled Y, and that key's glyph has to
        // say Y. Only for letters and digits: a key like the German Ü has no glyph of its own and keeps its position's.
        private static string GetLabeledPath(string controlPath)
        {
            if (Keyboard.current == null
                || InputControlPath.TryGetDeviceLayout(controlPath) != KeyboardLayout)
                return controlPath;

            string label = InputControlPath.TryFindControl(Keyboard.current, controlPath)?.displayName;
            if (string.IsNullOrEmpty(label)
                || label.Length != 1
                || !char.IsLetterOrDigit(label[0]))
                return controlPath;

            return $"{KeyboardPathStart}{char.ToLowerInvariant(label[0])}";
        }

        // Parts of a composite are skipped: a prompt for one shows the whole group, like the d-pad, not one direction.
        private bool TryGetReboundSprite(InputActionReference action, out Sprite sprite)
        {
            sprite = null;

            if (action == null
                || action.action == null)
                return false;

            foreach (InputBinding binding in action.action.bindings)
            {
                if (binding.isComposite
                    || binding.isPartOfComposite
                    || string.IsNullOrEmpty(binding.overridePath)
                    || !IsOnThisDevice(binding.overridePath))
                    continue;

                if (TryGetControlSprite(binding.overridePath, out sprite))
                    return true;

                CustomLogger.LogWarning($"No control glyph for \"{binding.overridePath}\", so \"{action.action.name}\" "
                    + $"shows its default button. Device type: {DeviceType}.", this);

                return false;
            }

            return false;
        }

        private bool IsOnThisDevice(string controlPath)
        {
            string layout = InputControlPath.TryGetDeviceLayout(controlPath);

            return DeviceType == EInputDeviceType.Gamepad
                ? layout == GamepadLayout
                : layout == KeyboardLayout || layout == MouseLayout;
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

        private Dictionary<string, Sprite> BuildControlLookup()
        {
            Dictionary<string, Sprite> lookup = new();

            foreach (InputControlGlyph glyph in controlGlyphs)
            {
                if (glyph == null
                    || string.IsNullOrEmpty(glyph.Path)
                    || glyph.Sprite == null)
                {
                    CustomLogger.LogWarning($"Glyph set \"{name}\" has an incomplete control glyph.", this);
                    continue;
                }

                lookup[glyph.Path] = glyph.Sprite;
            }

            return lookup;
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