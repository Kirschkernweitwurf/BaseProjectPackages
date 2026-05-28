namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>A string setting.</summary>
    public sealed class StringSetting : Setting<string>
    {
        /// <summary>Creates a string setting.</summary>
        /// <param name="store">The backing store.</param>
        /// <param name="key">The unique persistence key.</param>
        /// <param name="defaultValue">The default value.</param>
        public StringSetting(ISettingsStore store, string key, string defaultValue)
            : base(store, key, defaultValue) { }

        protected override string ReadFromStore(ISettingsStore store, string key, string defaultValue)
        {
            return store.GetString(key, defaultValue);
        }

        protected override void WriteToStore(ISettingsStore store, string key, string value)
        {
            store.SetString(key, value);
        }
    }
}