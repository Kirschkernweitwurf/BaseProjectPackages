using System.Collections.Generic;
using Base.AttributesPackage;
using Base.SettingsPackage.Core;
using Base.UtilityPackage.Menus;
using UnityEngine;
using UnityEngine.Localization;

namespace Base.SettingsPackage.Presets
{
    /// <summary>
    /// A named bundle of setting values, for example Low, Medium and High. Applying one writes every entry
    /// into the matching registered setting, which runs that setting's own applier, so a preset carries no
    /// code and knows nothing about what its values control.
    /// </summary>
    /// <remarks>
    /// A preset is an action rather than a persisted value. What is saved is the settings it wrote, so a
    /// player who applies High and then turns one thing down keeps that change on the next launch.
    /// </remarks>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Settings/New Settings Preset", "SP_SettingsPreset")]
    public sealed class SettingsPreset : ScriptableObject
    {
        [Title("Preset")]
        [Tooltip("Label shown on the button that applies this preset.")]
        [SerializeField] private LocalizedString displayName;

        [Tooltip("Every setting this preset writes. Keys that are not registered are reported and skipped.")]
        [SerializeField] private List<SettingsPresetEntry> entries = new();

        /// <summary>The label shown on the button that applies this preset.</summary>
        public LocalizedString DisplayName => displayName;

        /// <summary>Writes every entry into the matching registered setting.</summary>
        /// <param name="registry">The registry holding the settings to write to.</param>
        public void Apply(SettingsRegistry registry)
        {
            foreach (SettingsPresetEntry entry in entries)
                entry.ApplyTo(registry);
        }

        /// <summary>
        /// Whether every entry already matches the current values, which is what lets a row of preset
        /// buttons show which one the player is on and show none of them once something was tuned by hand.
        /// </summary>
        /// <param name="registry">The registry holding the settings to compare against.</param>
        /// <returns>True when every entry matches; otherwise false.</returns>
        public bool IsActive(SettingsRegistry registry)
        {
            // An empty preset would otherwise report itself as active against any settings at all.
            if (entries.Count == 0)
                return false;

            foreach (SettingsPresetEntry entry in entries)
            {
                if (!entry.Matches(registry))
                    return false;
            }

            return true;
        }
    }
}