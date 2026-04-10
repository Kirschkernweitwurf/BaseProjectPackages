namespace Tracking
{
    /// <summary>
    /// Internal class to track an item along with its priority and insertion order.
    /// </summary>
    public class TrackedItem<T>
    {
        public readonly T Item;
        public readonly uint Priority;
        public readonly ulong Order;

        public TrackedItem(T item, uint priority, ulong order)
        {
            Item = item;
            Priority = priority;
            Order = order;
        }
    }
}