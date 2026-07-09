using Base.SettingsPackage.Core;
using Base.UtilityPackage.Identification;

namespace Base.SettingsPackage.Components
{
    /// <summary>Typed base for components backed by a <see cref="StringSetting"/>.</summary>
    public abstract class StringSettingComponent : SettingComponent<string, StringSetting>
    {
        /// <inheritdoc/>
        protected sealed override StringSetting CreateSetting(ISettingsStore store, PersistentKey key,
            string defaultValue) => new(store, key, defaultValue);
    }
}