namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Central registry of persistence keys. Keeps the stored keys consistent in one place and
    /// avoids scattering magic strings across the codebase. Do not rename existing keys once shipped,
    /// or previously saved values will no longer be found.
    /// </summary>
    public static class SettingsKeys
    {
        public const string MasterVolume = "settings.audio.master_volume";
        public const string MusicVolume = "settings.audio.music_volume";
        public const string SfxVolume = "settings.audio.sfx_volume";

        public const string Fullscreen = "settings.display.fullscreen";
        public const string VSync = "settings.display.vsync";
        public const string QualityLevel = "settings.display.quality_level";

        public const string Language = "settings.general.language";
    }
}