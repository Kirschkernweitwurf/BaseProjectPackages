using System.Collections.Generic;
using System.Linq;
using Base.UtilityPackage.Logging;

namespace Base.SaveSystemPackage.Savable
{
    /// <summary>
    /// Where savables register themselves. The save system asks this registry for everyone, ordered by priority.
    /// Order is the highest priority first (Critical -> Low). Items with the same
    /// priority keep their registration order, so the result is deterministic.
    /// </summary>
    public static class SaveRegistry
    {
        private static readonly List<ISavable> Items = new();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetStatics() => Items.Clear();
#endif

        /// <summary>
        /// Register a new savable.
        /// </summary>
        /// <param name="savable">
        /// The savable to register. Must not have the same SaveId as an existing savable.
        /// </param>
        public static void Register(ISavable savable)
        {
            if (savable == null || Items.Contains(savable))
                return;

            if (Items.Any(s => s.SaveId == savable.SaveId))
            {
                CustomLogger.LogWarning($"A savable with SaveId '{savable.SaveId}' is already registered. " +
                                        "Each savable must have a unique SaveId. Skipping registration.", null);
                return;
            }

            Items.Add(savable);
        }

        /// <summary>
        /// Deregister a savable, e.g. when it's destroyed. Not strictly necessary but keeps the registry clean.
        /// </summary>
        /// <param name="savable"></param>
        public static void Deregister(ISavable savable)
        {
            if (savable != null)
                Items.Remove(savable);
        }

        /// <summary>
        /// All savables, highest priority first. Returns a fresh copy.
        /// </summary>
        public static IReadOnlyList<ISavable> GetOrdered()
        {
            List<ISavable> ordered = new(Items);

            List<(ISavable item, int index)> indexed = new(ordered.Count);
            indexed.AddRange(ordered.Select((t, i) => (t, i)));
            indexed.Sort((a, b) =>
            {
                int byPriority = ((byte)b.item.Priority).CompareTo((byte)a.item.Priority);
                return byPriority != 0 ? byPriority : a.index.CompareTo(b.index);
            });

            List<ISavable> result = new(indexed.Count);
            foreach ((ISavable item, int index) entry in indexed)
                result.Add(entry.item);

            return result;
        }
    }
}