namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Abstraction over a persistent key/value store. Decouples settings from the concrete storage
    /// backend, so the same settings can run on top of <see cref="UnityEngine.PlayerPrefs"/>, a file,
    /// a cloud save, or an in-memory store for unit tests without any change to the settings themselves.
    /// </summary>
    public interface ISettingsStore
    {
        /// <summary>Returns true if a value is stored for the given key.</summary>
        bool HasKey(string key);

        /// <summary>Reads a float, returning <paramref name="defaultValue"/> when the key is missing.</summary>
        float GetFloat(string key, float defaultValue);

        /// <summary>Writes a float to the in-memory store. Call <see cref="Save"/> to persist to disk.</summary>
        void SetFloat(string key, float value);

        /// <summary>Reads an int, returning <paramref name="defaultValue"/> when the key is missing.</summary>
        int GetInt(string key, int defaultValue);

        /// <summary>Writes an int to the in-memory store. Call <see cref="Save"/> to persist to disk.</summary>
        void SetInt(string key, int value);

        /// <summary>Reads a string, returning <paramref name="defaultValue"/> when the key is missing.</summary>
        string GetString(string key, string defaultValue);

        /// <summary>Writes a string to the in-memory store. Call <see cref="Save"/> to persist to disk.</summary>
        void SetString(string key, string value);

        /// <summary>Removes the value stored for the given key, if any.</summary>
        void DeleteKey(string key);

        /// <summary>Flushes all pending writes to disk.</summary>
        void Save();
    }
}