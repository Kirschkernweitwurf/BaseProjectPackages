using Base.SettingsPackage.Core;

namespace Base.SettingsPackage.Components
{
    /// <summary>Typed base for components backed by a <see cref="FloatSetting"/>.</summary>
    public abstract class FloatSettingComponent : SettingComponent<float, FloatSetting>
    {
        /// <inheritdoc/>
        protected sealed override FloatSetting CreateSetting(ISettingsStore store, string key, float defaultValue)
            => new(store, key, defaultValue);
    }
}