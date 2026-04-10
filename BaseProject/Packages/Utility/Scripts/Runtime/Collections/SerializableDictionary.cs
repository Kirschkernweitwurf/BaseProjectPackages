using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Collections
{
    /// <summary>
    /// A serializable dictionary that can be displayed and edited in the Unity Inspector.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        [SerializeField] private List<SerializableDictionaryEntry<TKey, TValue>> entries = new();

        private Dictionary<TKey, TValue> _dict;

        private bool _isDirty = true;

        /// <summary>
        /// Adds a new key-value pair to the dictionary.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value associated with the key.</param>
        public void Add(TKey key, TValue value)
        {
            entries.Add(new SerializableDictionaryEntry<TKey, TValue>(key, value));
            SetDirty();
        }

        /// <summary>
        /// Removes the entry with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns>True if the entry was removed; otherwise, false.</returns>
        public bool Remove(TKey key)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (!EqualityComparer<TKey>.Default.Equals(entries[i].key, key))
                    continue;

                entries.RemoveAt(i);
                SetDirty();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value if found; otherwise, the default value.</param>
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
        /// Removes all entries from the dictionary.
        /// </summary>
        public void Clear()
        {
            entries.Clear();
            SetDirty();
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
                if (TryGetEntryIndex(key, out int index))
                    entries[index] = new SerializableDictionaryEntry<TKey, TValue>(key, value);
                else
                    entries.Add(new SerializableDictionaryEntry<TKey, TValue>(key, value));

                SetDirty();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            EnsureDictionary();
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Ensures the runtime dictionary is initialized and synchronized with the serialized entries.
        /// </summary>
        private void EnsureDictionary()
        {
            if (!_isDirty && _dict != null)
                return;

            _dict = new Dictionary<TKey, TValue>();
            foreach (SerializableDictionaryEntry<TKey, TValue> entry in entries)
            {
                if (entry.key == null || _dict.ContainsKey(entry.key))
                    continue;

                _dict[entry.key] = entry.value;
            }

            _isDirty = false;
        }

        private bool TryGetEntryIndex(TKey key, out int index)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (!EqualityComparer<TKey>.Default.Equals(entries[i].key, key))
                    continue;

                index = i;
                return true;
            }
            index = -1;
            return false;
        }

        private void SetDirty() => _isDirty = true;
    }
}