#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Utility.Identification.Editor
{
    /// <summary>
    /// Tracks which UniqueIds are already "claimed" during this editor session.
    /// It is also responsible for deciding when an asset needs a new ID.
    /// </summary>
    internal static class UniqueIdRegistry
    {
        private static readonly Dictionary<string, Object> UsedIds = new();

        /// <summary>
        /// Clears all cached ID registrations.
        /// Call this before doing a full project-wide rebuild.
        /// </summary>
        internal static void Reset() => UsedIds.Clear();

        /// <summary>
        /// Ensures that the provided asset has a globally unique ID.
        /// Returns the final ID that should be on the asset (may be unchanged or newly generated).
        /// </summary>
        internal static string EnsureUnique(Object asset, string candidateId)
        {
            bool needsNew = string.IsNullOrEmpty(candidateId) || IsDuplicate(asset, candidateId);
            if (needsNew)
                candidateId = Guid.NewGuid().ToString();

            UsedIds[candidateId] = asset;
            return candidateId;
        }

        /// <summary>
        /// Registers an existing valid ID (used when we know we don't need to mutate it).
        /// </summary>
        internal static void Register(Object asset, string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            UsedIds[id] = asset;
        }

        private static bool IsDuplicate(Object asset, string id)
        {
            if (!UsedIds.TryGetValue(id, out Object existing))
                return false;

            // If the same object already "owns" this id, that's fine.
            return existing != null && existing != asset;
        }
    }
}
#endif