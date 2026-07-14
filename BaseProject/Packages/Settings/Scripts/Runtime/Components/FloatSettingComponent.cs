using Base.SettingsPackage.Core;
using Base.ToolPackage.Identification;

namespace Base.SettingsPackage.Components
{
    /// <summary>Typed base for components backed by a <see cref="FloatSetting"/>.</summary>
    public abstract class FloatSettingComponent : SettingComponent<float, FloatSetting>
    {
        /// <inheritdoc/>
        protected sealed override FloatSetting CreateSetting(ISettingsStore store, PersistentKey key,
            float defaultValue) => new(store, key, defaultValue);
    }
}