using System.Collections.Generic;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Public access point for the game's settings. Consumers subscribe to the individual settings
    /// they care about and never touch persistence directly, which keeps every system decoupled from
    /// both each other and the storage backend.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>Overall output volume, range 0..1.</summary>
        FloatSetting MasterVolume { get; }

        /// <summary>Music volume, range 0..1.</summary>
        FloatSetting MusicVolume { get; }

        /// <summary>Sound effects volume, range 0..1.</summary>
        FloatSetting SfxVolume { get; }

        /// <summary>Whether the game runs in fullscreen.</summary>
        BoolSetting Fullscreen { get; }

        /// <summary>Whether vertical sync is enabled.</summary>
        BoolSetting VSync { get; }

        /// <summary>The selected quality level index.</summary>
        IntSetting QualityLevel { get; }

        /// <summary>The selected language.</summary>
        EnumSetting<ELanguage> Language { get; }

        /// <summary>All settings, useful for bulk operations such as a "reset to defaults" button.</summary>
        IReadOnlyList<ISetting> All { get; }

        /// <summary>Flushes any pending changes to the backing store.</summary>
        void Save();

        /// <summary>Resets every setting to its default value and saves.</summary>
        void ResetAllToDefault();
    }
}