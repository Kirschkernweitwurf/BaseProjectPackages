using Base.UtilityPackage.Identification;

namespace Base.SettingsPackage.Controls
{
    /// <summary>
    /// The keys the control settings persist under. Owned here rather than by whatever writes them, so
    /// the component that stores a value and the code that reads it cannot drift apart.
    /// </summary>
    public static class ControlSettingKeys
    {
        /// <summary>Key of the horizontal invert toggle.</summary>
        public static readonly PersistentKey InvertHorizontal = new("InvertLookHorizontal");

        /// <summary>Key of the vertical invert toggle.</summary>
        public static readonly PersistentKey InvertVertical = new("InvertLookVertical");

        /// <summary>Key of the normalized look sensitivity slider.</summary>
        public static readonly PersistentKey LookSensitivity = new("LookSensitivity");

        /// <summary>Key of the binding overrides written by the rebind rows.</summary>
        public static readonly PersistentKey Rebinds = new("Rebinds");
    }
}