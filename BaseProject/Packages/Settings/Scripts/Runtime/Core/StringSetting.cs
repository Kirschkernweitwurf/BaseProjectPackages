using Base.ToolPackage.Identification;

namespace Base.SettingsPackage.Core
{
    /// <summary>String setting.</summary>
    public sealed class StringSetting : Setting<string>
    {
        /// <summary>Creates a string setting.</summary>
        public StringSetting(ISettingsStore store, PersistentKey key, string defaultValue)
            : base(store, key, defaultValue) { }

        /// <inheritdoc/>
        protected override string Read(ISettingsStore store, string fallback) => store.GetString(Key.Value, fallback);

        /// <inheritdoc/>
        protected override void Write(ISettingsStore store, string value) => store.SetString(Key.Value, value);
    }
}