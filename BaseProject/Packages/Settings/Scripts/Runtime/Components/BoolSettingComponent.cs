using Base.SettingsPackage.Core;

namespace Base.SettingsPackage.Components
{
    /// <summary>Typed base for components backed by a <see cref="BoolSetting"/>.</summary>
    public abstract class BoolSettingComponent : SettingComponent<bool, BoolSetting>
    {
        /// <inheritdoc/>
        protected sealed override BoolSetting CreateSetting(ISettingsStore store, string key, bool defaultValue)
            => new(store, key, defaultValue);
    }
}