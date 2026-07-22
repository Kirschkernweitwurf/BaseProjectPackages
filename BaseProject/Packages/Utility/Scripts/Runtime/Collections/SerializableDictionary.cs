using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base.UtilityPackage.Collections
{
    /// <summary>
    /// A serializable dictionary that can be displayed and edited in the Unity Inspector.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
        : IEnumerable<KeyValuePair<TKey, TValue>>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SerializableDictionaryEntry<TKey, TValue>> entries = new();

        /// <summary>
        /// Gets the number of key-value pairs contained in the dictionary.
        /// </summary>
        public int Count
        {
            get
            {
                EnsureDictionary();
                return _dict.Count;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the key.</returns>
        public TValue this[TKey key]
        {
            get
            {
                EnsureDictionary();
                return _dict[key];
            }
            set
            {
                EnsureDictionary();

                if (TryGetEntryIndex(key, out int index))
                    entries[index] = new SerializableDictionaryEntry<TKey, TValue>(key, value);
                else
                    entries.Add(new SerializableDictionaryEntry<TKey, TValue>(key, value));

                _dict[key] = value;
            }
        }

        /// <summary>
        /// Gets an enumerable collection of the keys contained in the dictionary.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                EnsureDictionary();
                return _dict.Keys;
            }
        }

        /// <summary>
        /// Gets an enumerable collection of the values contained in the dictionary.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                EnsureDictionary();
                return _dict.Values;
            }
        }

        private static readonly EqualityComparer<TKey> KeyComparer = EqualityComparer<TKey>.Default;

        private Dictionary<TKey, TValue> _dict;

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            EnsureDictionary();
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        /// <summary>
        /// Discards the runtime dictionary after Unity deserializes the entries (e.g. inspector edits),
        /// so the next access rebuilds it from the fresh serialized data.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize() => _dict = null;

        /// <summary>
        /// Adds a new key-value pair to the dictionary.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <exception cref="ArgumentException">Thrown if the key already exists.</exception>
        public void Add(TKey key, TValue value)
        {
            EnsureDictionary();
            _dict.Add(key, value); // Throws on duplicate keys, matching Dictionary semantics.
            entries.Add(new SerializableDictionaryEntry<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Removes the entry with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns>True if the entry was removed; otherwise, false.</returns>
        public bool Remove(TKey key)
        {
            if (!TryGetEntryIndex(key, out int index))
                return false;

            entries.RemoveAt(index);
            _dict?.Remove(key);
            return true;
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the value if found; otherwise, the default value.
        /// </param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            EnsureDictionary();
            return _dict.TryGetValue(key, out value);
        }

        /// <summary>
        /// Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>True if the dictionary contains the key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            EnsureDictionary();
            return _dict.ContainsKey(key);
        }

        /// <summary>
        /// Removes all entries from the dictionary.
        /// </summary>
        public void Clear()
        {
            entries.Clear();
            _dict?.Clear();
        }

        /// <summary>
        /// Builds the runtime dictionary from the serialized entries if it does not exist yet.
        /// Mutations keep both stores in sync afterwards, so no rebuild is needed per access.
        /// </summary>
        private void EnsureDictionary()
        {
            if (_dict != null)
                return;

            _dict = new Dictionary<TKey, TValue>(entries.Count);
            foreach (SerializableDictionaryEntry<TKey, TValue> entry in entries)
            {
                if (entry.key == null
                    || _dict.ContainsKey(entry.key))
                    continue;

                _dict[entry.key] = entry.value;
            }
        }

        private bool TryGetEntryIndex(TKey key, out int index)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (!KeyComparer.Equals(entries[i].key, key))
                    continue;

                index = i;
                return true;
            }

            index = -1;
            return false;
        }
    }
}
