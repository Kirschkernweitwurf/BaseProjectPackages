using System.Collections.Generic;
using Base.UtilityPackage.Identification;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SettingsPackage.Core
{
    /// <summary>
    /// Holds every registered setting and drives load, save, revert, and reset across all of them.
    /// Settings are registered by the consuming project, so the package carries no game-specific keys.
    /// </summary>
    public sealed class SettingsRegistry
    {
        /// <summary>All registered settings, in registration order.</summary>
        public IReadOnlyCollection<ISetting> Settings => _orderedSettings;

        private readonly Dictionary<PersistentKey, ISetting> _settingsByKey = new();
        private readonly List<ISetting> _orderedSettings = new();
        private readonly ISettingsStore _store;
        private readonly Object _context;

        /// <summary>Creates a registry over a store. The optional context is used as the logging context.</summary>
        public SettingsRegistry(ISettingsStore store, Object context = null)
        {
            _store = store;
            _context = context;
        }

        /// <summary>
        /// Registers a setting and returns it, so it can be captured and have appliers attached in one statement.
        /// Registration order is preserved, which matters when one setting must be applied before another.
        /// </summary>
        public TSetting Register<TSetting>(TSetting setting) where TSetting : class, ISetting
        {
            if (setting == null)
            {
                CustomLogger.LogError("Cannot register a null setting.", _context);
                return null;
            }

            if (_settingsByKey.TryGetValue(setting.Key, out ISetting value))
            {
                CustomLogger.LogError($"A setting with key '{setting.Key}' is already registered.", _context);
                return (TSetting)value;
            }

            _settingsByKey.Add(setting.Key, setting);
            _orderedSettings.Add(setting);
            return setting;
        }

        /// <summary>Returns whether a setting with the given key is registered.</summary>
        public bool Contains(PersistentKey key) => _settingsByKey.ContainsKey(key);

        /// <summary>
        /// Tries to resolve a setting by key as the requested type.
        /// Logs an error and returns false on a missing key or a type mismatch.
        /// </summary>
        public bool TryGet<TSetting>(PersistentKey key, out TSetting setting) where TSetting : class, ISetting
        {
            setting = null;

            if (!_settingsByKey.TryGetValue(key, out ISetting found))
            {
                CustomLogger.LogError($"Setting with key '{key}' not found.", _context);
                return false;
            }

            setting = found as TSetting;
            if (setting != null)
                return true;

            CustomLogger.LogError($"Setting '{key}' is not of the requested type {typeof(TSetting).Name}.", _context);
            return false;
        }

        /// <summary>Resolves a setting by key as the requested type, or null if missing or mismatched.</summary>
        public TSetting Get<TSetting>(PersistentKey key) where TSetting : class, ISetting
        {
            TryGet(key, out TSetting setting);
            return setting;
        }

        /// <summary>Loads every registered setting from the store, applying their loaded values.</summary>
        public void LoadAll()
        {
            foreach (ISetting setting in _orderedSettings)
                setting.Load();
        }

        /// <summary>Writes every registered setting to the store and flushes once.</summary>
        public void SaveAll()
        {
            foreach (ISetting setting in _orderedSettings)
                setting.Save();

            _store.Flush();
        }

        /// <summary>Restores every registered setting to its last persisted value.</summary>
        public void RevertAll()
        {
            foreach (ISetting setting in _orderedSettings)
                setting.Revert();
        }

        /// <summary>Restores every registered setting to its default value.</summary>
        public void ResetAllToDefault()
        {
            foreach (ISetting setting in _orderedSettings)
                setting.ResetToDefault();
        }
    }
}