using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base.UtilityPackage.Collections
{
    /// <summary>
    /// A dictionary that survives Unity serialization and is editable in the inspector. Entries are
    /// stored as a serialized list and projected into a runtime dictionary on first access, so lookups
    /// stay O(1) while the authored data remains a plain list. When the inspector contains duplicate
    /// keys, the first occurrence wins and the drawer reports the conflict.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    [Serializable]
    public sealed class SerializableDictionary<TKey, TValue>
        : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        /// <summary>Name of the serialized entry list. Used by the inspector drawer.</summary>
        public const string EntriesField = nameof(entries);

        private static readonly EqualityComparer<TKey> KeyComparer = EqualityComparer<TKey>.Default;

        private static readonly EqualityComparer<TValue> ValueComparer = EqualityComparer<TValue>.Default;

        // Field name is kept lowercase and unchanged so existing serialized data keeps resolving.
        [SerializeField] private List<SerializableDictionaryEntry<TKey, TValue>> entries = new();

        private Dictionary<TKey, TValue> _dictionary;

        /// <summary>Gets the number of key-value pairs contained in the dictionary.</summary>
        public int Count => Resolved.Count;

        /// <summary>Always false. The dictionary is writable at runtime.</summary>
        public bool IsReadOnly => false;

        /// <summary>Gets a collection of the keys contained in the dictionary.</summary>
        public ICollection<TKey> Keys => Resolved.Keys;

        /// <summary>Gets a collection of the values contained in the dictionary.</summary>
        public ICollection<TValue> Values => Resolved.Values;

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the key.</returns>
        public TValue this[TKey key]
        {
            get => Resolved[key];
            set
            {
                if (TryGetEntryIndex(key, out int index))
                    entries[index] = new SerializableDictionaryEntry<TKey, TValue>(key, value);
                else
                    entries.Add(new SerializableDictionaryEntry<TKey, TValue>(key, value));

                Resolved[key] = value;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Resolved.Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Resolved.Values;

        // Rebuilds from the serialized entries on first access after every deserialization.
        private Dictionary<TKey, TValue> Resolved
        {
            get
            {
                if (_dictionary == null)
                    Rebuild();

                return _dictionary;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        // Discards the runtime dictionary after Unity writes the entries back, for example after an
        // inspector edit, so the next access rebuilds it from the fresh serialized data.
        void ISerializationCallbackReceiver.OnAfterDeserialize() => _dictionary = null;

        /// <summary>Adds a new key-value pair to the dictionary.</summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <exception cref="ArgumentException">Thrown when the key already exists.</exception>
        public void Add(TKey key, TValue value)
        {
            Resolved.Add(key, value); // Throws on duplicate keys before the entry list is touched.
            entries.Add(new SerializableDictionaryEntry<TKey, TValue>(key, value));
        }

        /// <summary>Adds an existing pair to the dictionary.</summary>
        /// <param name="item">The pair to add.</param>
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <summary>Removes all entries from the dictionary.</summary>
        public void Clear()
        {
            entries.Clear();
            Resolved.Clear();
        }

        /// <summary>Determines whether the dictionary contains the given pair.</summary>
        /// <param name="item">The pair to locate.</param>
        /// <returns>True when both the key and the value match.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
            => TryGetValue(item.Key, out TValue value) && ValueComparer.Equals(value, item.Value);

        /// <summary>Determines whether the dictionary contains the specified key.</summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>True when the dictionary contains the key.</returns>
        public bool ContainsKey(TKey key) => Resolved.ContainsKey(key);

        /// <summary>Copies all pairs into the given array, starting at the given index.</summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The index in the destination array to start writing at.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Resolved).CopyTo(array, arrayIndex);

        /// <summary>Returns an enumerator that iterates through the dictionary.</summary>
        /// <returns>An enumerator over all key-value pairs.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Resolved.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Removes the entry with the specified key.</summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns>True when an entry was removed.</returns>
        public bool Remove(TKey key)
        {
            if (!TryGetEntryIndex(key, out int index))
                return false;

            entries.RemoveAt(index);
            Resolved.Remove(key);
            return true;
        }

        /// <summary>Removes the given pair when both the key and the value match.</summary>
        /// <param name="item">The pair to remove.</param>
        /// <returns>True when an entry was removed.</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item) => Contains(item) && Remove(item.Key);

        /// <summary>Attempts to get the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The value when found, otherwise the default value.</param>
        /// <returns>True when the key exists.</returns>
        public bool TryGetValue(TKey key, out TValue value) => Resolved.TryGetValue(key, out value);

        private void Rebuild()
        {
            _dictionary = new Dictionary<TKey, TValue>(entries.Count);

            foreach (SerializableDictionaryEntry<TKey, TValue> entry in entries)
            {
                // Null and duplicate keys are authoring mistakes the drawer reports. Skipping them here
                // keeps the runtime dictionary usable instead of throwing during deserialization.
                if (entry.Key == null
                    || _dictionary.ContainsKey(entry.Key))
                    continue;

                _dictionary[entry.Key] = entry.Value;
            }
        }

        private bool TryGetEntryIndex(TKey key, out int index)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (!KeyComparer.Equals(entries[i].Key, key))
                    continue;

                index = i;
                return true;
            }

            index = -1;
            return false;
        }
    }
}
