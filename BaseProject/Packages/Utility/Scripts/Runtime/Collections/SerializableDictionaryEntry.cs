using System;
using UnityEngine;

namespace Base.UtilityPackage.Collections
{
    /// <summary>
    /// Serializable key-value pair entry for use in <see cref="SerializableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The key of the entry.</typeparam>
    /// <typeparam name="TValue">The value associated with the key.</typeparam>
    [Serializable]
    public struct SerializableDictionaryEntry<TKey, TValue>
    {
        // Field names are kept lowercase so existing serialized data keeps resolving.
        [SerializeField] private TKey key;
        [SerializeField] private TValue value;

        /// <summary>
        /// The key of the entry.
        /// </summary>
        public TKey Key => key;

        /// <summary>
        /// The value associated with <see cref="Key"/>.
        /// </summary>
        public TValue Value => value;

        /// <summary>
        /// Creates an entry from a key and its value.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="value">The value associated with the key.</param>
        public SerializableDictionaryEntry(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}