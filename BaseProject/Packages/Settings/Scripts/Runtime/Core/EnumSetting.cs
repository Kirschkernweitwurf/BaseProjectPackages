using System;

namespace Base.SettingsPackage.Core
{
    /// <summary>Enum setting persisted as its underlying integer value.</summary>
    /// <typeparam name="TEnum">The enum type held by the setting.</typeparam>
    public sealed class EnumSetting<TEnum> : Setting<TEnum> where TEnum : struct, Enum
    {
        /// <summary>Creates an enum setting.</summary>
        public EnumSetting(ISettingsStore store, string key, TEnum defaultValue)
            : base(store, key, defaultValue) { }

        /// <inheritdoc/>
        protected override TEnum Read(ISettingsStore store, TEnum fallback)
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), store.GetInt(Key, Convert.ToInt32(fallback)));
        }

        /// <inheritdoc/>
        protected override void Write(ISettingsStore store, TEnum value) => store.SetInt(Key, Convert.ToInt32(value));
    }
}