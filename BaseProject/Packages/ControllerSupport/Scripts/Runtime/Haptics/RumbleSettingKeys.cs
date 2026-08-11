using Base.ToolPackage.Identification;

namespace Base.ControllerSupport.Haptics
{
    /// <summary>
    /// The keys the rumble settings persist under. Owned by this package rather than by whatever writes
    /// them, so the component that stores a value and the service that consumes it cannot drift apart.
    /// </summary>
    public static class RumbleSettingKeys
    {
        /// <summary>Key of the on/off toggle.</summary>
        public static readonly PersistentKey Enabled = new("RumbleEnabled");

        /// <summary>Key of the strength slider.</summary>
        public static readonly PersistentKey Intensity = new("RumbleIntensity");
    }
}