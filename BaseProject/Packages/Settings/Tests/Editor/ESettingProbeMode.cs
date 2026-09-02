namespace Base.SettingsPackage.Tests
{
    /// <summary>
    /// The enum an enum setting is tested with. The values are spread out on purpose, so a test that
    /// passes by coincidence when the underlying number happens to line up with the position stands
    /// out instead.
    /// </summary>
    public enum ESettingProbeMode : byte
    {
        Off = 0,
        Low = 3,
        High = 7
    }
}