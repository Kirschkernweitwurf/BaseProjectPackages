using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Default <see cref="ISettingsService"/> implementation. Creates the concrete settings, wires
    /// them to a store and exposes them. This is a plain C# class with no Unity dependency, so it can
    /// be unit tested with an in-memory store. To add a new setting: create the field, build it in the
    /// constructor, and add it to the <c>_all</c> collection.
    /// </summary>
    public sealed class GameSettingsService : ISettingsService
    {
        public FloatSetting MasterVolume { get; }
        public FloatSetting MusicVolume { get; }
        public FloatSetting SfxVolume { get; }
        public BoolSetting Fullscreen { get; }
        public BoolSetting VSync { get; }
        public IntSetting QualityLevel { get; }
        public EnumSetting<ELanguage> Language { get; }
        public IReadOnlyList<ISetting> All { get; }

        private readonly ISettingsStore _store;

        /// <summary>
        /// Creates the service and its settings.
        /// </summary>
        /// <param name="store">The backing store used by every setting.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is null.</exception>
        public GameSettingsService(ISettingsStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));

            MasterVolume = new FloatSetting(store, SettingsKeys.MasterVolume, 1f, 0f, 1f);
            MusicVolume = new FloatSetting(store, SettingsKeys.MusicVolume, 0.8f, 0f, 1f);
            SfxVolume = new FloatSetting(store, SettingsKeys.SfxVolume, 0.8f, 0f, 1f);
            Fullscreen = new BoolSetting(store, SettingsKeys.Fullscreen, true);
            VSync = new BoolSetting(store, SettingsKeys.VSync, true);
            QualityLevel = new IntSetting(store, SettingsKeys.QualityLevel, 2, 0, 5);
            Language = new EnumSetting<ELanguage>(store, SettingsKeys.Language, ELanguage.English);

            List<ISetting> settings = new()
            {
                MasterVolume,
                MusicVolume,
                SfxVolume,
                Fullscreen,
                VSync,
                QualityLevel,
                Language
            };

            All = new ReadOnlyCollection<ISetting>(settings);
        }

        public void ResetAllToDefault()
        {
            foreach (ISetting setting in All)
                setting.ResetToDefault();

            _store.Save();
        }

        public void Save() => _store.Save();
    }
}