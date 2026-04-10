using System;

namespace Utility.Collections
{
    /// <summary>
    /// Serializable key-value pair entry for use in <see cref="SerializableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The key of the entry.</typeparam>
    /// <typeparam name="TValue">The value associated with the key.</typeparam>
    [Serializable]
    public struct SerializableDictionaryEntry<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public SerializableDictionaryEntry(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}