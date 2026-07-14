using Base.ToolPackage.Identification;

namespace Base.SettingsPackage.Core
{
    /// <summary>
    /// Type-agnostic contract that lets settings of different value types be stored and driven together
    /// by a <see cref="SettingsRegistry"/>.
    /// </summary>
    public interface ISetting
    {
        /// <summary>Unique key used to identify and persist the setting.</summary>
        PersistentKey Key { get; }

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