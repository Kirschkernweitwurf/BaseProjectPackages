using Base.SettingsPackage.Core;
using Base.UtilityPackage.Identification;

namespace Base.SettingsPackage.Components
{
    /// <summary>Typed base for components backed by an <see cref="IntSetting"/>.</summary>
    public abstract class IntSettingComponent : SettingComponent<int, IntSetting>
    {
        /// <inheritdoc/>
        protected sealed override IntSetting CreateSetting(ISettingsStore store, PersistentKey key, int defaultValue)
            => new(store, key, defaultValue);
    }
}