using System;
using System.Collections.Generic;

namespace Base.SaveSystemPackage.Serialization.Wire
{
    /// <summary>
    /// The container written to disk: just the list of (id, state) pairs.
    /// </summary>
    [Serializable]
    internal sealed class SaveBlob
    {
        public List<SaveEntry> entries = new();

        public void Add(string id, string state) => entries.Add(new SaveEntry
        {
            id = id,
            state = state
        });

        /// <summary>
        /// Build an id -> state map once, so loading is O(n) instead of O(n*m) linear scans.
        /// </summary>
        public Dictionary<string, string> ToLookup()
        {
            Dictionary<string, string> map = new(entries.Count);
            foreach (SaveEntry e in entries)
            {
                if (e?.id != null)
                    map[e.id] = e.state;
            }

            return map;
        }
    }
}