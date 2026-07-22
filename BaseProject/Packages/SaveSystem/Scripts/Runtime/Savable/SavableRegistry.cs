using System;
using System.Collections.Generic;
using Base.ToolPackage.Identification;
using Base.UtilityPackage.Logging;

namespace Base.SaveSystemPackage.Savable
{
    /// <summary>
    /// Default <see cref="ISavableRegistry"/>. Plain instance, no static state, so resetting between
    /// play sessions or tests is just "make a new one".
    /// The ordered list is cached and only rebuilt after a register/deregister, so save and load
    /// do not re-sort an unchanged set.
    /// </summary>
    public sealed class SavableRegistry : ISavableRegistry
    {
        private readonly List<ISavable> _items = new();
        private readonly HashSet<PersistentKey> _keys = new();
        private ISavable[] _orderedCache;

        public void Register(ISavable savable)
        {
            if (savable == null || _items.Contains(savable))
                return;

            if (savable.PersistentKey.IsEmpty)
            {
                CustomLogger.LogWarning("A savable was registered with an empty PersistentKey. "
                    + "Each savable must expose a valid PersistentKey. Skipping registration.", null);

                return;
            }

            if (!_keys.Add(savable.PersistentKey))
            {
                CustomLogger.LogWarning($"A savable with {nameof(PersistentKey)} '{savable.PersistentKey}'"
                    + " is already registered. Each savable must have a unique key."
                    + " Skipping registration.", null);

                return;
            }

            _items.Add(savable);
            _orderedCache = null;
        }

        public void Deregister(ISavable savable)
        {
            if (savable == null || !_items.Remove(savable))
                return;

            _keys.Remove(savable.PersistentKey);
            _orderedCache = null;
        }

        public IReadOnlyList<ISavable> GetOrdered() => _orderedCache ??= BuildOrdered();

        private ISavable[] BuildOrdered()
        {
            // Stable sort: highest priority first, ties keep registration order.
            ISavable[] snapshot = _items.ToArray();
            int[] order = new int[snapshot.Length];
            for (int i = 0; i < order.Length; i++)
                order[i] = i;

            Array.Sort(order, (a, b) =>
            {
                int byPriority = ((byte)snapshot[b].Priority).CompareTo((byte)snapshot[a].Priority);
                return byPriority != 0
                    ? byPriority
                    : a.CompareTo(b);
            });

            ISavable[] ordered = new ISavable[snapshot.Length];
            for (int i = 0; i < ordered.Length; i++)
                ordered[i] = snapshot[order[i]];

            return ordered;
        }
    }
}
