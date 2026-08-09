using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base.UtilityPackage.Collections
{
    /// <summary>
    /// A set that survives Unity serialization and is editable in the inspector. Items are stored as a
    /// serialized list and projected into a runtime set on first access, so membership tests stay O(1)
    /// while the authored data remains a plain list. Duplicates entered in the inspector keep their
    /// first occurrence; the drawer reports the conflict.
    /// </summary>
    /// <typeparam name="T">The type of the items in the set.</typeparam>
    [Serializable]
    public sealed class SerializableHashSet<T> : ISet<T>, IReadOnlyCollection<T>, ISerializationCallbackReceiver
    {
        /// <summary>Name of the serialized item list. Used by the inspector drawer.</summary>
        public const string ItemsField = nameof(items);

        private static readonly EqualityComparer<T> ItemComparer = EqualityComparer<T>.Default;

        [SerializeField] private List<T> items = new();

        private HashSet<T> _set;

        /// <summary>Gets the number of items contained in the set.</summary>
        public int Count => Resolved.Count;

        /// <summary>Always false. The set is writable at runtime.</summary>
        public bool IsReadOnly => false;

        // Rebuilds from the serialized items on first access after every deserialization.
        private HashSet<T> Resolved
        {
            get
            {
                if (_set == null)
                    Rebuild();

                return _set;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        // Discards the runtime set after Unity writes the items back, for example after an inspector
        // edit, so the next access rebuilds it from the fresh serialized data.
        void ISerializationCallbackReceiver.OnAfterDeserialize() => _set = null;

        /// <summary>Adds an item to the set.</summary>
        /// <param name="item">The item to add.</param>
        /// <returns>True when the item was not already present.</returns>
        public bool Add(T item)
        {
            if (!Resolved.Add(item))
                return false;

            items.Add(item);
            return true;
        }

        void ICollection<T>.Add(T item) => Add(item);

        /// <summary>Removes all items from the set.</summary>
        public void Clear()
        {
            items.Clear();
            Resolved.Clear();
        }

        /// <summary>Determines whether the set contains the given item.</summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>True when the item is present.</returns>
        public bool Contains(T item) => Resolved.Contains(item);

        /// <summary>Copies all items into the given array, starting at the given index.</summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The index in the destination array to start writing at.</param>
        public void CopyTo(T[] array, int arrayIndex) => Resolved.CopyTo(array, arrayIndex);

        /// <summary>Removes every item that is also in the given collection.</summary>
        /// <param name="other">The items to remove.</param>
        public void ExceptWith(IEnumerable<T> other) => Mutate(set => set.ExceptWith(other));

        /// <summary>Returns an enumerator that iterates through the set.</summary>
        /// <returns>An enumerator over all items.</returns>
        public IEnumerator<T> GetEnumerator() => Resolved.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Keeps only the items that are also in the given collection.</summary>
        /// <param name="other">The items to keep.</param>
        public void IntersectWith(IEnumerable<T> other) => Mutate(set => set.IntersectWith(other));

        /// <summary>Determines whether the set is a proper subset of the given collection.</summary>
        /// <param name="other">The collection to compare against.</param>
        /// <returns>True when every item is contained and the collection has more items.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other) => Resolved.IsProperSubsetOf(other);

        /// <summary>Determines whether the set is a proper superset of the given collection.</summary>
        /// <param name="other">The collection to compare against.</param>
        /// <returns>True when every given item is contained and the set has more items.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other) => Resolved.IsProperSupersetOf(other);

        /// <summary>Determines whether the set is a subset of the given collection.</summary>
        /// <param name="other">The collection to compare against.</param>
        /// <returns>True when every item is contained in the collection.</returns>
        public bool IsSubsetOf(IEnumerable<T> other) => Resolved.IsSubsetOf(other);

        /// <summary>Determines whether the set is a superset of the given collection.</summary>
        /// <param name="other">The collection to compare against.</param>
        /// <returns>True when every given item is contained in the set.</returns>
        public bool IsSupersetOf(IEnumerable<T> other) => Resolved.IsSupersetOf(other);

        /// <summary>Determines whether the set shares at least one item with the given collection.</summary>
        /// <param name="other">The collection to compare against.</param>
        /// <returns>True when at least one item is shared.</returns>
        public bool Overlaps(IEnumerable<T> other) => Resolved.Overlaps(other);

        /// <summary>Removes an item from the set.</summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True when the item was present.</returns>
        public bool Remove(T item)
        {
            if (!Resolved.Remove(item))
                return false;

            // Every occurrence goes, not just the first. Duplicates authored in the inspector collapse
            // into a single set entry, so leaving one behind would make the item unaddable afterwards.
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (ItemComparer.Equals(items[i], item))
                    items.RemoveAt(i);
            }

            return true;
        }

        /// <summary>Determines whether the set and the given collection contain the same items.</summary>
        /// <param name="other">The collection to compare against.</param>
        /// <returns>True when both contain exactly the same items.</returns>
        public bool SetEquals(IEnumerable<T> other) => Resolved.SetEquals(other);

        /// <summary>Keeps only the items present in either the set or the collection, but not both.</summary>
        /// <param name="other">The collection to compare against.</param>
        public void SymmetricExceptWith(IEnumerable<T> other) => Mutate(set => set.SymmetricExceptWith(other));

        /// <summary>Adds every item of the given collection that is not already present.</summary>
        /// <param name="other">The items to add.</param>
        public void UnionWith(IEnumerable<T> other) => Mutate(set => set.UnionWith(other));

        // Bulk set operations are applied to the runtime set, then the serialized list is rewritten from
        // it. Mirroring each individual insert and removal by hand would duplicate the set logic.
        private void Mutate(Action<HashSet<T>> operation)
        {
            operation(Resolved);

            items.Clear();
            items.AddRange(Resolved);
        }

        private void Rebuild()
        {
            _set = new HashSet<T>();

            // Duplicates are an authoring mistake the drawer reports. Skipping them here keeps the
            // runtime set usable instead of losing the association between list and set.
            foreach (T item in items)
                _set.Add(item);
        }
    }
}
