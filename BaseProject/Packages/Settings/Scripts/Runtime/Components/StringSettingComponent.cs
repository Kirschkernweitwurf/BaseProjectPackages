using Base.SettingsPackage.Core;

namespace Base.SettingsPackage.Components
{
    /// <summary>Typed base for components backed by a <see cref="StringSetting"/>.</summary>
    public abstract class StringSettingComponent : SettingComponent<string, StringSetting>
    {
        /// <inheritdoc/>
        protected sealed override StringSetting CreateSetting(ISettingsStore store, string key, string defaultValue)
            => new(store, key, defaultValue);
    }
}