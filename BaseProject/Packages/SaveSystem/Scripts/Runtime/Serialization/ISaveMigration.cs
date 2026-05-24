using System.Collections.Generic;

namespace Base.SaveSystemPackage.Serialization
{
    /// <summary>
    /// Upgrades a save one version forward. Register a chain of these so an old save can be
    /// stepped up to the current <c>SaveVersion</c> on load. Each migration works on the raw
    /// id -> state map (the serialized strings each savable owns) so the system stays agnostic
    /// about data shapes.
    /// </summary>
    public interface ISaveMigration
    {
        /// <summary>Migrates data written at this version up to <see cref="FromVersion"/> + 1.</summary>
        int FromVersion { get; }

        /// <summary>Mutate the map in place: add, remove, or rewrite entries as needed.</summary>
        void Migrate(IDictionary<string, string> states);
    }
}