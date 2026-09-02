using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Base.ServicesPackage.Tracking
{
    /// <summary>
    /// Tracks items with associated priorities.
    /// Higher priority items are more important.
    /// If multiple items share the same priority, insertion order is used as a tiebreaker.
    /// </summary>
    /// <typeparam name="T">The type of the tracked items.</typeparam>
    public sealed class PriorityTracker<T>
    {
        /// <summary>
        /// Invoked whenever the current tracked item changes. Passes <c>null</c> when nothing is tracked.
        /// </summary>
        public event Action<TrackedItem<T>> OnCurrentActiveItemChanged;

        /// <summary>
        /// The currently active (highest priority) tracked item, or <c>null</c> if nothing is tracked.
        /// </summary>
        public TrackedItem<T> CurrentTrackedItem { get; private set; }

        /// <summary>
        /// All tracked items, in insertion order.
        /// </summary>
        public IReadOnlyList<TrackedItem<T>> TrackedItems => _trackedItems;

        private readonly List<TrackedItem<T>> _trackedItems = new();
        private readonly Dictionary<object, TrackedItem<T>> _callerToTracked = new();

        private ulong _orderCounter;

        /// <summary>
        /// Raises <see cref="OnCurrentActiveItemChanged"/> once, so listeners can sync to the current state.
        /// </summary>
        public void Initialize() => OnCurrentActiveItemChanged?.Invoke(CurrentTrackedItem);

        /// <summary>
        /// Adds an item with the given priority on behalf of a specific caller.
        /// </summary>
        /// <param name="item">The item to track.</param>
        /// <param name="priority">The priority of the item. Higher values take precedence.</param>
        /// <param name="caller">The object requesting the item, used as the key for removal.</param>
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
            _trackedItems.Add(tracked);
            _callerToTracked[caller] = tracked;
            ReevaluateCurrent();
        }

        /// <summary>
        /// Removes the item associated with the given caller.
        /// </summary>
        /// <param name="caller">The object that added the item.</param>
        public void Remove(object caller)
        {
            if (caller == null)
            {
                CustomLogger.LogWarning("Tried to remove with a null caller.", null);
                return;
            }

            if (!_callerToTracked.Remove(caller, out TrackedItem<T> tracked))
            {
                CustomLogger.LogWarning($"Tried removing an item from an unknown caller: {caller}.", null);
                return;
            }

            _trackedItems.Remove(tracked);
            ReevaluateCurrent();
        }

        /// <summary>
        /// Clears all tracked items.
        /// </summary>
        public void Clear()
        {
            _trackedItems.Clear();
            _callerToTracked.Clear();
            _orderCounter = 0;
            ReevaluateCurrent();
        }

        /// <summary>
        /// Checks if an item is currently tracked.
        /// </summary>
        /// <param name="item">The item to look for.</param>
        /// <returns><c>true</c> if the item is tracked; otherwise, <c>false</c>.</returns>
        public bool IsTracked(T item)
        {
            if (item == null)
                return false;

            foreach (TrackedItem<T> tracked in _trackedItems)
            {
                if (AreItemsEqual(tracked.Item, item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a caller currently has an active tracked item.
        /// </summary>
        /// <param name="caller">The object to look for.</param>
        /// <returns><c>true</c> if the caller has a tracked item; otherwise, <c>false</c>.</returns>
        public bool HasCaller(object caller) => caller != null && _callerToTracked.ContainsKey(caller);

        /// <summary>
        /// Compares two items, using Unity's equality operator for <see cref="Object"/> types
        /// so that destroyed objects are treated as null.
        /// </summary>
        private static bool AreItemsEqual(T a, T b)
        {
            if (a is Object unityA
                && b is Object unityB)
                return unityA == unityB;

            return EqualityComparer<T>.Default.Equals(a, b);
        }

        private void ReevaluateCurrent()
        {
            if (_trackedItems.Count == 0)
            {
                if (CurrentTrackedItem == null)
                    return;

                CurrentTrackedItem = null;
                OnCurrentActiveItemChanged?.Invoke(null);
                return;
            }

            TrackedItem<T> top = _trackedItems[0];
            foreach (TrackedItem<T> candidate in _trackedItems)
            {
                if (candidate.Priority > top.Priority
                    || candidate.Priority == top.Priority
                    && candidate.Order > top.Order)
                    top = candidate;
            }

            if (CurrentTrackedItem == top)
                return;

            CurrentTrackedItem = top;
            OnCurrentActiveItemChanged?.Invoke(CurrentTrackedItem);
        }
    }
}