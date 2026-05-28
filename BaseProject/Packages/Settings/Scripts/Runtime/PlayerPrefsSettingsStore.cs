using UnityEngine;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// <see cref="ISettingsStore"/> implementation backed by Unity's <see cref="PlayerPrefs"/>.
    /// Writes are buffered by PlayerPrefs and only flushed to disk on <see cref="Save"/>.
    /// </summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public float GetFloat(string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);

        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);

        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public string GetString(string key, string defaultValue) => PlayerPrefs.GetString(key, defaultValue);

        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);

        public void Save() => PlayerPrefs.Save();
    }
}