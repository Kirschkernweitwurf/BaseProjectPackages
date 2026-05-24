using System.Linq;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;

namespace Base.SaveSystemPackage.Savable
{
    /// <summary>
    /// Default <see cref="ISavableRegistry"/>. Plain instance, no static state, so resetting between
    /// play sessions or tests is just "make a new one".
    /// </summary>
    public sealed class SavableRegistry : ISavableRegistry
    {
        private readonly List<ISavable> _items = new();

        public void Register(ISavable savable)
        {
            if (savable == null || _items.Contains(savable))
                return;

            if (_items.Any(s => s.SaveId == savable.SaveId))
            {
                CustomLogger.LogWarning($"A savable with SaveId '{savable.SaveId}' is already registered. " +
                                        "Each savable must have a unique SaveId. Skipping registration.", null);
                return;
            }

            _items.Add(savable);
        }

        public void Deregister(ISavable savable)
        {
            if (savable != null)
                _items.Remove(savable);
        }

        public IReadOnlyList<ISavable> GetOrdered()
        {
            return _items
                .Select((item, index) => (item, index))
                .OrderByDescending(x => (byte)x.item.Priority)
                .ThenBy(x => x.index)
                .Select(x => x.item)
                .ToList();
        }
    }
}