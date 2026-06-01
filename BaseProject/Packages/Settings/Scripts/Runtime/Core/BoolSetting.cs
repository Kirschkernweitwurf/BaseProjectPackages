namespace Base.SettingsPackage.Core
{
    /// <summary>Boolean setting persisted as an integer (0 or 1).</summary>
    public sealed class BoolSetting : Setting<bool>
    {
        /// <summary>Creates a boolean setting.</summary>
        public BoolSetting(ISettingsStore store, string key, bool defaultValue)
            : base(store, key, defaultValue) { }

        /// <inheritdoc/>
        protected override bool Read(ISettingsStore store, bool fallback)
        {
            return store.Has(Key) ? store.GetInt(Key, fallback ? 1 : 0) != 0 : fallback;
        }

        /// <inheritdoc/>
        protected override void Write(ISettingsStore store, bool value) => store.SetInt(Key, value ? 1 : 0);
    }
}