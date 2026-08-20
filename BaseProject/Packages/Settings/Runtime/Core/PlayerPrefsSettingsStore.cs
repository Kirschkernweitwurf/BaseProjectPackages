using UnityEngine;

namespace Base.SettingsPackage.Core
{
    /// <summary>
    /// Default <see cref="ISettingsStore"/> backed by Unity's <see cref="PlayerPrefs"/>.
    /// Writes are buffered until <see cref="Flush"/> is called, which keeps revert behavior intact.
    /// </summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        /// <inheritdoc/>
        public bool Has(string key) => PlayerPrefs.HasKey(key);

        /// <inheritdoc/>
        public int GetInt(string key, int fallback) => PlayerPrefs.GetInt(key, fallback);

        /// <inheritdoc/>
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        /// <inheritdoc/>
        public float GetFloat(string key, float fallback) => PlayerPrefs.GetFloat(key, fallback);

        /// <inheritdoc/>
        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        /// <inheritdoc/>
        public string GetString(string key, string fallback) => PlayerPrefs.GetString(key, fallback);

        /// <inheritdoc/>
        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        /// <inheritdoc/>
        public void Flush() => PlayerPrefs.Save();

        /// <inheritdoc/>
        public void Delete(string key) => PlayerPrefs.DeleteKey(key);
    }
}