namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// A boolean setting. PlayerPrefs has no native bool, so the value is stored as an int (0 or 1).
    /// </summary>
    public sealed class BoolSetting : Setting<bool>
    {
        /// <summary>Creates a bool setting.</summary>
        /// <param name="store">The backing store.</param>
        /// <param name="key">The unique persistence key.</param>
        /// <param name="defaultValue">The default value.</param>
        public BoolSetting(ISettingsStore store, string key, bool defaultValue)
            : base(store, key, defaultValue) { }

        protected override bool ReadFromStore(ISettingsStore store, string key, bool defaultValue)
        {
            return store.GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        protected override void WriteToStore(ISettingsStore store, string key, bool value)
        {
            store.SetInt(key, value ? 1 : 0);
        }
    }
}