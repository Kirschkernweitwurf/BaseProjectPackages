using System;
using Base.UtilityPackage.Identification;

namespace Base.SettingsPackage.Core
{
    /// <summary>
    /// Type-agnostic contract that lets settings of different value types be stored and driven together
    /// by a <see cref="SettingsRegistry"/>.
    /// </summary>
    public interface ISetting
    {
        /// <summary>
        /// Raised whenever the value changes, for listeners that do not know the value type, such as the
        /// registry, preset buttons and per-setting reset buttons.
        /// </summary>
        event Action OnChanged;

        /// <summary>Unique key used to identify and persist the setting.</summary>
        PersistentKey Key { get; }

        /// <summary>True while the current value equals the configured default.</summary>
        bool IsDefault { get; }

        /// <summary>Loads the value from the backing store and notifies listeners.</summary>
        void Load();

        /// <summary>Writes the value to the backing store.</summary>
        void Save();

        /// <summary>Restores the value to the state captured at the last load or save.</summary>
        void Revert();

        /// <summary>Restores the value to its configured default.</summary>
        void ResetToDefault();
    }
}