using System;
using System.Collections.Generic;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// The container that actually gets written to disk. It is just a list of
    /// (id, state) pairs collected from the savables. Plain [Serializable] types
    /// so JsonUtility can handle them (note: JsonUtility can serialize a List as a
    /// field, but not as the top-level object, which is why SaveBlob wraps it).
    /// </summary>
    [Serializable]
    public sealed class SaveBlob
    {
        public List<SaveEntry> entries = new();

        /// <summary>
        /// Find the state string for the given id, or null if not found.
        /// </summary>
        /// <param name="id">The id to look for.</param>
        /// <returns>The state string for the given id, or <c>null</c> if not found.</returns>
        public string Find(string id)
        {
            foreach (SaveEntry e in entries)
                if (e.id == id)
                    return e.state;

            return null;
        }
    }
}