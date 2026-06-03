using Base.UtilityPackage.Identification;

namespace Base.SettingsPackage.Core
{
    /// <summary>Boolean setting persisted as an integer (0 or 1).</summary>
    public sealed class BoolSetting : Setting<bool>
    {
        /// <summary>Creates a boolean setting.</summary>
        public BoolSetting(ISettingsStore store, PersistentKey key, bool defaultValue)
            : base(store, key, defaultValue) { }

        /// <inheritdoc/>
        protected override bool Read(ISettingsStore store, bool fallback)
        {
            return store.Has(Key.Value) ? store.GetInt(Key.Value, fallback ? 1 : 0) != 0 : fallback;
        }

        /// <inheritdoc/>
        protected override void Write(ISettingsStore store, bool value) => store.SetInt(Key.Value, value ? 1 : 0);
    }
}