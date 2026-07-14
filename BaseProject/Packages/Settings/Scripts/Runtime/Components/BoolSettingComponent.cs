using Base.SettingsPackage.Core;
using Base.ToolPackage.Identification;

namespace Base.SettingsPackage.Components
{
    /// <summary>Typed base for components backed by a <see cref="BoolSetting"/>.</summary>
    public abstract class BoolSettingComponent : SettingComponent<bool, BoolSetting>
    {
        /// <inheritdoc/>
        protected sealed override BoolSetting CreateSetting(ISettingsStore store, PersistentKey key, bool defaultValue)
            => new(store, key, defaultValue);
    }
}