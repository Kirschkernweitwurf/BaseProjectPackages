namespace Base.SettingsPackage.Core
{
    /// <summary>
    /// Abstraction over the key/value backend used to persist settings.
    /// Implement this to swap <see cref="PlayerPrefsSettingsStore"/> for a file, cloud, or in-memory store.
    /// </summary>
    public interface ISettingsStore
    {
        /// <summary>Returns whether a value has been persisted for the given key.</summary>
        bool Has(string key);

        /// <summary>Reads an integer, returning <paramref name="fallback"/> when the key is absent.</summary>
        int GetInt(string key, int fallback);

        /// <summary>Writes an integer without flushing it to permanent storage.</summary>
        void SetInt(string key, int value);

        /// <summary>Reads a float, returning <paramref name="fallback"/> when the key is absent.</summary>
        float GetFloat(string key, float fallback);

        /// <summary>Writes a float without flushing it to permanent storage.</summary>
        void SetFloat(string key, float value);

        /// <summary>Reads a string, returning <paramref name="fallback"/> when the key is absent.</summary>
        string GetString(string key, string fallback);

        /// <summary>Writes a string without flushing it to permanent storage.</summary>
        void SetString(string key, string value);

        /// <summary>Commits all buffered writes to permanent storage.</summary>
        void Flush();

        /// <summary>Removes the persisted value for the given key, if present.</summary>
        void Delete(string key);
    }
}