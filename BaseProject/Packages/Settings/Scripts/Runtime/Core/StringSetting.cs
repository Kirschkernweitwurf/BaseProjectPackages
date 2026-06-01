namespace Base.SettingsPackage.Core
{
    /// <summary>String setting.</summary>
    public sealed class StringSetting : Setting<string>
    {
        /// <summary>Creates a string setting.</summary>
        public StringSetting(ISettingsStore store, string key, string defaultValue)
            : base(store, key, defaultValue) { }

        /// <inheritdoc/>
        protected override string Read(ISettingsStore store, string fallback) => store.GetString(Key, fallback);

        /// <inheritdoc/>
        protected override void Write(ISettingsStore store, string value) => store.SetString(Key, value);
    }
}