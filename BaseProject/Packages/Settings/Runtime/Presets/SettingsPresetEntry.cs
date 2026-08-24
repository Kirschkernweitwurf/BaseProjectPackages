using System;
using Base.AttributePackage;
using Base.SettingsPackage.Core;
using Base.UtilityPackage.Identification;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SettingsPackage.Presets
{
    /// <summary>
    /// One key and value pair inside a <see cref="SettingsPreset"/>. Unity cannot serialize a value whose
    /// type is only known at runtime, so the entry carries one field per supported type and the value type
    /// picks which of them counts.
    /// </summary>
    [Serializable]
    public sealed class SettingsPresetEntry
    {
        [SerializeField] [NotNullOrEmpty] private string settingKey;
        [SerializeField] private ESettingValueType valueType;

        [SerializeField] [ShowIfEnum(nameof(valueType), ESettingValueType.Bool)]
        private bool boolValue;

        [SerializeField] [ShowIfEnum(nameof(valueType), ESettingValueType.Float)]
        private float floatValue;

        [SerializeField] [ShowIfEnum(nameof(valueType), ESettingValueType.Int)]
        private int intValue;

        [SerializeField] [ShowIfEnum(nameof(valueType), ESettingValueType.String)]
        private string stringValue;

        /// <summary>
        /// Writes this entry's value into the matching registered setting, which runs that setting's own
        /// applier. A key that is not registered is reported by the registry and skipped.
        /// </summary>
        /// <param name="registry">The registry holding the settings to write to.</param>
        public void ApplyTo(SettingsRegistry registry)
        {
            if (!PersistentKey.TryCreate(settingKey, out PersistentKey key))
            {
                CustomLogger.LogError($"A preset entry carries the invalid setting key '{settingKey}'.", null);
                return;
            }

            switch (valueType)
            {
                case ESettingValueType.Bool:
                    if (registry.TryGet(key, out BoolSetting boolSetting))
                        boolSetting.Value = boolValue;

                    break;

                case ESettingValueType.Float:
                    if (registry.TryGet(key, out FloatSetting floatSetting))
                        floatSetting.Value = floatValue;

                    break;

                case ESettingValueType.Int:
                    if (registry.TryGet(key, out IntSetting intSetting))
                        intSetting.Value = intValue;

                    break;

                default:
                    if (registry.TryGet(key, out StringSetting stringSetting))
                        stringSetting.Value = stringValue;

                    break;
            }
        }

        /// <summary>Whether the registered setting already holds this entry's value.</summary>
        /// <param name="registry">The registry holding the settings to compare against.</param>
        /// <returns>True when the values match; false when they differ or the key is not registered.</returns>
        public bool Matches(SettingsRegistry registry)
        {
            // This runs on every change of every setting, so it stays silent and leaves the reporting to
            // ApplyTo. A preset covering more settings than the current scene registers is normal, and a
            // key that is wrong is reported the moment someone applies the preset.
            if (!PersistentKey.TryCreate(settingKey, out PersistentKey key)
                || !registry.Contains(key))
                return false;

            return valueType switch
            {
                ESettingValueType.Bool => registry.TryGet(key, out BoolSetting boolSetting)
                    && boolSetting.Value == boolValue,
                ESettingValueType.Float => registry.TryGet(key, out FloatSetting floatSetting)
                    && Mathf.Approximately(floatSetting.Value, floatValue),
                ESettingValueType.Int => registry.TryGet(key, out IntSetting intSetting)
                    && intSetting.Value == intValue,
                _ => registry.TryGet(key, out StringSetting stringSetting)
                    && string.Equals(stringSetting.Value, stringValue, StringComparison.Ordinal)
            };
        }
    }
}