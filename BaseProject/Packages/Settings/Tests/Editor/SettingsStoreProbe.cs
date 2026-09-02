using System.Collections.Generic;
using Base.SettingsPackage.Core;

namespace Base.SettingsPackage.Tests
{
    /// <summary>
    /// A store that keeps everything in memory instead of in player preferences, so the settings tests
    /// can state exactly what was written and how often it was committed, and leave nothing behind on
    /// the machine they ran on.
    /// </summary>
    internal sealed class SettingsStoreProbe : ISettingsStore
    {
        /// <summary>How often the buffered writes were committed.</summary>
        internal int FlushCount { get; private set; }

        private readonly Dictionary<string, object> _values = new();

        /// <inheritdoc/>
        public bool Has(string key) => _values.ContainsKey(key);

        /// <inheritdoc/>
        public int GetInt(string key, int fallback) => _values.TryGetValue(key, out object stored)
            ? (int)stored
            : fallback;

        /// <inheritdoc/>
        public void SetInt(string key, int value) => _values[key] = value;

        /// <inheritdoc/>
        public float GetFloat(string key, float fallback) => _values.TryGetValue(key, out object stored)
            ? (float)stored
            : fallback;

        /// <inheritdoc/>
        public void SetFloat(string key, float value) => _values[key] = value;

        /// <inheritdoc/>
        public string GetString(string key, string fallback) => _values.TryGetValue(key, out object stored)
            ? (string)stored
            : fallback;

        /// <inheritdoc/>
        public void SetString(string key, string value) => _values[key] = value;

        /// <inheritdoc/>
        public void Flush() => FlushCount++;

        /// <inheritdoc/>
        public void Delete(string key) => _values.Remove(key);
    }
}