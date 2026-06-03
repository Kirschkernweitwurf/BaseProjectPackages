using Base.UtilityPackage.Identification;

namespace Base.SettingsPackage.Core
{
    /// <summary>Floating point setting.</summary>
    public sealed class FloatSetting : Setting<float>
    {
        /// <summary>Creates a float setting.</summary>
        public FloatSetting(ISettingsStore store, PersistentKey key, float defaultValue)
            : base(store, key, defaultValue) { }

        /// <inheritdoc/>
        protected override float Read(ISettingsStore store, float fallback) => store.GetFloat(Key.Value, fallback);

        /// <inheritdoc/>
        protected override void Write(ISettingsStore store, float value) => store.SetFloat(Key.Value, value);
    }
}