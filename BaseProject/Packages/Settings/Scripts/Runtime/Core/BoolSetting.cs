using Base.ToolPackage.Identification;

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
            => store.GetInt(Key.Value, ToInt(fallback)) != 0;

        /// <inheritdoc/>
        protected override void Write(ISettingsStore store, bool value) => store.SetInt(Key.Value, ToInt(value));

        private static int ToInt(bool value) => value
            ? 1
            : 0;
    }
}
