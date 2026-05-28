using System;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// An enum setting. The underlying integer value is stored, so the enum can grow or be reordered
    /// safely as long as the numeric values of existing members stay the same.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    public sealed class EnumSetting<TEnum> : Setting<TEnum> where TEnum : struct, Enum
    {
        /// <summary>Creates an enum setting.</summary>
        /// <param name="store">The backing store.</param>
        /// <param name="key">The unique persistence key.</param>
        /// <param name="defaultValue">The default value.</param>
        public EnumSetting(ISettingsStore store, string key, TEnum defaultValue)
            : base(store, key, defaultValue) { }

        protected override TEnum ReadFromStore(ISettingsStore store, string key, TEnum defaultValue)
        {
            int storedValue = store.GetInt(key, Convert.ToInt32(defaultValue));
            return (TEnum)Enum.ToObject(typeof(TEnum), storedValue);
        }

        protected override void WriteToStore(ISettingsStore store, string key, TEnum value)
        {
            store.SetInt(key, Convert.ToInt32(value));
        }
    }
}