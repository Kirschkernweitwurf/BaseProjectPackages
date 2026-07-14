using Base.ToolPackage.Identification;

namespace Base.SettingsPackage.Core
{
    /// <summary>Integer setting.</summary>
    public sealed class IntSetting : Setting<int>
    {
        /// <summary>Creates an integer setting.</summary>
        public IntSetting(ISettingsStore store, PersistentKey key, int defaultValue)
            : base(store, key, defaultValue) { }

        /// <inheritdoc/>
        protected override int Read(ISettingsStore store, int fallback) => store.GetInt(Key.Value, fallback);

        /// <inheritdoc/>
        protected override void Write(ISettingsStore store, int value) => store.SetInt(Key.Value, value);
    }
}