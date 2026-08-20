using System.Collections.Generic;
using Base.UtilityPackage.Logging;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace Base.ServicePackage.Tracking
{
    /// <summary>
    /// Generic tracker that maps unique keys to values, which allows registration,
    /// removal, retrieval, and clearing of tracked elements.
    /// </summary>
    /// <typeparam name="TKey">The type used as keys.</typeparam>
    /// <typeparam name="TValue">The type of values to be tracked.</typeparam>
    public sealed class Tracker<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _trackedElements = new();

        /// <summary>
        /// Adds an element with a unique key.
        /// </summary>
        /// <param name="key">The key to register the element under.</param>
        /// <param name="element">The element to track.</param>
        /// <returns><c>true</c> if the element was registered; otherwise, <c>false</c>.</returns>
        public bool Register(TKey key, TValue element)
        {
            if (_trackedElements.TryAdd(key, element))
                return true;

            CustomLogger.LogWarning($"Key '{key}' is already registered.", null);
            return false;
        }

        /// <summary>
        /// Removes an element by its key.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><c>true</c> if an element was removed; otherwise, <c>false</c>.</returns>
        public bool Remove(TKey key) => _trackedElements.Remove(key);

        /// <summary>
        /// Attempts to get an element by key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="element">The tracked element if found; otherwise, the default value.</param>
        /// <returns><c>true</c> if an element was found; otherwise, <c>false</c>.</returns>
        public bool TryGet(TKey key, out TValue element) => _trackedElements.TryGetValue(key, out element);

        /// <summary>
        /// Removes all tracked elements.
        /// </summary>
        public void Clear() => _trackedElements.Clear();
    }
}