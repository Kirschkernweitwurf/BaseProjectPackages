using System;
using Base.SettingsPackage.Core;
using Base.UtilityPackage.Identification;

namespace Base.SettingsPackage.Components
{
    /// <summary>Typed base for components backed by an <see cref="EnumSetting{TEnum}"/>.</summary>
    /// <typeparam name="TEnum">The enum type held by the setting.</typeparam>
    public abstract class EnumSettingComponent<TEnum> : SettingComponent<TEnum, EnumSetting<TEnum>>
        where TEnum : struct, Enum
    {
        /// <inheritdoc/>
        protected sealed override EnumSetting<TEnum> CreateSetting(ISettingsStore store, PersistentKey key,
            TEnum defaultValue) => new(store, key, defaultValue);
    }
}