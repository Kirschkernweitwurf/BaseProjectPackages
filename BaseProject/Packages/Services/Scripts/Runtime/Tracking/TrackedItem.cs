namespace Base.ServicePackage.Tracking
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

        internal TrackedItem(T item, uint priority, ulong order)
        {
            Item = item;
            Priority = priority;
            Order = order;
        }
    }
}