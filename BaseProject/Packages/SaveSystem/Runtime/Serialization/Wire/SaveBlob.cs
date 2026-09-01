using System;
using System.Collections.Generic;

namespace Base.SaveSystemPackage.Serialization.Wire
{
    /// <summary>
    /// The container written to disk: just the list of id and state pairs.
    /// </summary>
    [Serializable]
    internal sealed class SaveBlob
    {
        /// <summary>
        /// Every savable's state, in the order it was collected. A list rather than a dictionary
        /// because JsonUtility cannot serialize one.
        /// </summary>
        public List<SaveEntry> entries = new();

        /// <summary>
        /// Builds an id to state map once, so loading is O(n) instead of O(n*m) linear scans.
        /// </summary>
        /// <returns>Every entry that carries an id.</returns>
        public Dictionary<string, string> ToLookup()
        {
            Dictionary<string, string> map = new(entries.Count);
            foreach (SaveEntry entry in entries)
            {
                if (entry?.id != null)
                    map[entry.id] = entry.state;
            }

            return map;
        }

        /// <summary>Appends one savable's serialized state.</summary>
        internal void Add(string id, string state) => entries.Add(new SaveEntry
        {
            id = id,
            state = state
        });
    }
}