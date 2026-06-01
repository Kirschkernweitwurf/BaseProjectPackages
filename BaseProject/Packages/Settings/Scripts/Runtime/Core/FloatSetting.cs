namespace Base.SettingsPackage.Core
{
    /// <summary>Floating point setting.</summary>
    public sealed class FloatSetting : Setting<float>
    {
        /// <summary>Creates a float setting.</summary>
        public FloatSetting(ISettingsStore store, string key, float defaultValue)
            : base(store, key, defaultValue) { }

        /// <inheritdoc/>
        protected override float Read(ISettingsStore store, float fallback) => store.GetFloat(Key, fallback);

        /// <inheritdoc/>
        protected override void Write(ISettingsStore store, float value) => store.SetFloat(Key, value);
    }
}