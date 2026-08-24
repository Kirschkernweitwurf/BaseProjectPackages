using Base.UtilityPackage.Identification;

namespace Base.SaveSystemPackage.Unity.Autosave
{
    /// <summary>
    /// The keys the autosave settings persist under. Owned by this package rather than by whatever
    /// writes them, so the component that stores a value and the service that consumes it cannot drift
    /// apart.
    /// </summary>
    public static class AutosaveSettingKeys
    {
        /// <summary>Key of the shortest gap between two autosaves.</summary>
        public static readonly PersistentKey Cooldown = new("AutosaveCooldown");

        /// <summary>Key of the on/off toggle.</summary>
        public static readonly PersistentKey Enabled = new("AutosaveEnabled");

        /// <summary>Key of the time between timed autosaves.</summary>
        public static readonly PersistentKey Interval = new("AutosaveInterval");
    }
}