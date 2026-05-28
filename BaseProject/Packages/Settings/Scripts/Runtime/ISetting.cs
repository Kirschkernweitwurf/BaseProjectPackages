namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Non-generic view over a setting. Lets the service treat all settings uniformly for bulk
    /// operations such as "reset all to default" or reloading from disk.
    /// </summary>
    public interface ISetting
    {
        /// <summary>The unique persistence key for this setting.</summary>
        string Key { get; }

        /// <summary>Resets the value back to its default.</summary>
        void ResetToDefault();

        /// <summary>Reloads the value from the backing store, discarding the cached value.</summary>
        void Reload();
    }
}