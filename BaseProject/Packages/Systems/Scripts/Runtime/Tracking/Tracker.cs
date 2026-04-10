using System;
using System.Collections.Generic;
using Utility.Logging;

namespace Tracking
{
    /// <summary>
    /// Generic tracker that maps unique enum keys to values, which allows registration,
    /// removal, retrieval, and clearing of tracked elements.
    /// </summary>
    /// <typeparam name="TKey">The enum type used as keys.</typeparam>
    /// <typeparam name="TValue">The type of values to be tracked.</typeparam>
    public class Tracker<TKey, TValue> where TKey : struct, Enum
    {
        private readonly Dictionary<TKey, TValue> _trackedElements = new();

        /// <summary>
        /// Adds an element with a unique enum key.
        /// </summary>
        public bool Register(TKey key, TValue element)
        {
            if (_trackedElements.TryAdd(key, element))
                return true;

            CustomLogger.LogWarning($"Tracker: Key '{key}' is already registered.", null);
            return false;
        }

        /// <summary>
        /// Removes an element by its enum key.
        /// </summary>
        public bool Remove(TKey key) => _trackedElements.Remove(key);

        /// <summary>
        /// Attempts to get an element by enum key.
        /// </summary>
        public bool TryGet(TKey key, out TValue element) => _trackedElements.TryGetValue(key, out element);

        /// <summary>
        /// Removes all tracked elements.
        /// </summary>
        public void Clear() => _trackedElements.Clear();
    }
}