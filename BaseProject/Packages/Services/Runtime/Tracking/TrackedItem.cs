namespace Base.ServicesPackage.Tracking
{
    /// <summary>
    /// A single tracked item together with its priority and insertion order.
    /// Created and owned by <see cref="PriorityTracker{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the tracked item.</typeparam>
    public sealed class TrackedItem<T>
    {
        /// <summary>The tracked item itself.</summary>
        public T Item { get; }

        /// <summary>The priority of the item. Higher values win.</summary>
        public uint Priority { get; }

        /// <summary>The insertion order, used as a tiebreaker between equal priorities.</summary>
        public ulong Order { get; }

        /// <summary>Creates a tracked item. Only the tracker that owns it may do this.</summary>
        /// <param name="item">The item being tracked.</param>
        /// <param name="priority">The priority it was registered with.</param>
        /// <param name="order">The insertion order, assigned by the tracker.</param>
        internal TrackedItem(T item, uint priority, ulong order)
        {
            Item = item;
            Priority = priority;
            Order = order;
        }
    }
}