using System;
using System.Collections.Generic;
using Utility.Logging;

namespace Tracking
{
    /// <summary>
    /// Tracks items with associated priorities.
    /// Higher priority items are more important.
    /// If multiple items share the same priority, insertion order is used as a tiebreaker.
    /// </summary>
    public class PriorityTracker<T>
    {
        /// <summary>
        /// The currently active (highest-priority) tracked item.
        /// </summary>
        public TrackedItem<T> CurrentTrackedItem { get; private set; }

        /// <summary>
        /// Invoked whenever the current tracked item changes.
        /// </summary>
        public event Action<TrackedItem<T>> OnCurrentActiveItemChanged;

        public readonly List<TrackedItem<T>> TrackedItems = new();
        private readonly Dictionary<object, TrackedItem<T>> _callerToTracked = new();
        private ulong _orderCounter;

        public void Initialize() => OnCurrentActiveItemChanged?.Invoke(CurrentTrackedItem);

        /// <summary>
        /// Adds an item with the given priority on behalf of a specific caller.
        /// </summary>
        public void Add(T item, uint priority, object caller)
        {
            if (item == null)
            {
                CustomLogger.LogWarning("Tried to add a null item.", null);
                return;
            }

            if (caller == null)
            {
                CustomLogger.LogWarning("Tried to add with a null caller.", null);
                return;
            }

            if (_callerToTracked.ContainsKey(caller))
            {
                CustomLogger.LogWarning("Tried adding an item from the same caller twice.", null);
                return;
            }

            TrackedItem<T> tracked = new(item, priority, _orderCounter++);
            TrackedItems.Add(tracked);
            _callerToTracked[caller] = tracked;
            ReevaluateCurrent();
        }

        /// <summary>
        /// Removes an item associated with the given caller.
        /// </summary>
        public void Remove(object caller)
        {
            if (caller == null)
            {
                CustomLogger.LogWarning("Tried to remove with a null caller.", null);
                return;
            }

            if (!_callerToTracked.TryGetValue(caller, out TrackedItem<T> tracked))
            {
                CustomLogger.LogWarning($"Tried removing an item from an unknown caller: {caller}.", null);
                return;
            }

            TrackedItems.Remove(tracked);
            _callerToTracked.Remove(caller);
            ReevaluateCurrent();
        }

        /// <summary>
        /// Clears all tracked items.
        /// </summary>
        public void Clear()
        {
            TrackedItems.Clear();
            _callerToTracked.Clear();
            _orderCounter = 0;
            ReevaluateCurrent();
        }

        /// <summary>
        /// Checks if an item is currently tracked.
        /// </summary>
        public bool IsTracked(T item)
        {
            if (item == null)
                return false;

            foreach (TrackedItem<T> t in TrackedItems)
            {
                if (Equals(t.Item, item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a caller currently has an active tracked item.
        /// </summary>
        public bool HasCaller(object caller)
        {
            return caller != null && _callerToTracked.ContainsKey(caller);
        }

        private void ReevaluateCurrent()
        {
            if (TrackedItems.Count == 0)
            {
                if (CurrentTrackedItem == null)
                    return;

                CurrentTrackedItem = null;
                OnCurrentActiveItemChanged?.Invoke(null);
                return;
            }

            TrackedItem<T> top = TrackedItems[0];
            for (int i = 1; i < TrackedItems.Count; i++)
            {
                TrackedItem<T> next = TrackedItems[i];
                if (next.Priority > top.Priority ||
                    (next.Priority == top.Priority && next.Order > top.Order))
                {
                    top = next;
                }
            }

            if (Equals(CurrentTrackedItem, top))
                return;

            CurrentTrackedItem = top;
            OnCurrentActiveItemChanged?.Invoke(CurrentTrackedItem);
        }

        /// <summary>
        /// Compares two items for equality, with special handling for UnityEngine.Object types.
        /// </summary>
        private static bool Equals(T a, T b)
        {
            if (a is UnityEngine.Object ao && b is UnityEngine.Object bo)
                return ao == bo;

            return EqualityComparer<T>.Default.Equals(a, b);
        }
    }
}