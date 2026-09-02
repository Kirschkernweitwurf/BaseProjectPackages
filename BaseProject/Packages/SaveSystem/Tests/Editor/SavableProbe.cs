using Base.SaveSystemPackage.Savable;
using Base.ServicesPackage.Tracking;
using Base.UtilityPackage.Identification;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// A savable the registry tests can register. Its key and priority are handed in, so a test can
    /// build the exact set it wants to see ordered.
    /// </summary>
    internal sealed class SavableProbe : ISavable
    {
        /// <inheritdoc/>
        public PersistentKey PersistentKey { get; }

        /// <inheritdoc/>
        public EPriority Priority { get; }

        /// <summary>The state handed back on the last load, or null when there was none.</summary>
        internal string Restored { get; private set; }

        private readonly string _state;

        /// <summary>Creates a savable under a key.</summary>
        /// <param name="key">The key the registry files it under. Empty for the invalid case.</param>
        /// <param name="priority">The priority that decides when it runs.</param>
        /// <param name="state">The state it reports when asked to serialize.</param>
        internal SavableProbe(PersistentKey key, EPriority priority = EPriority.Medium, string state = null)
        {
            PersistentKey = key;
            Priority = priority;
            _state = state;
        }

        /// <inheritdoc/>
        public string Serialize() => _state;

        /// <inheritdoc/>
        public void Deserialize(string state) => Restored = state;
    }
}