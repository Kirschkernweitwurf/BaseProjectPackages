using System.Collections.Generic;
using Base.UtilityPackage.Logging;

namespace Base.SystemsCorePackage.Tracking
{
    /// <summary>
    /// Generic tracker that maps unique keys to values, which allows registration,
    /// removal, retrieval, and clearing of tracked elements.
    /// </summary>
    /// <typeparam name="TKey">The type used as keys.</typeparam>
    /// <typeparam name="TValue">The type of values to be tracked.</typeparam>
    public class Tracker<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _trackedElements = new();

        /// <summary>
        /// Adds an element with a unique key.
        /// </summary>
        public bool Register(TKey key, TValue element)
        {
            if (_trackedElements.TryAdd(key, element))
                return true;

            CustomLogger.LogWarning($"Tracker: Key '{key}' is already registered.", null);
            return false;
        }

        /// <summary>
        /// Removes an element by its key.
        /// </summary>
        public bool Remove(TKey key) => _trackedElements.Remove(key);

        /// <summary>
        /// Attempts to get an element by key.
        /// </summary>
        public bool TryGet(TKey key, out TValue element) => _trackedElements.TryGetValue(key, out element);

        /// <summary>
        /// Removes all tracked elements.
        /// </summary>
        public void Clear() => _trackedElements.Clear();
    }
}