using System;
using Base.EditorUIPackage.Editor;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// Which column the table is ordered by and in which direction, plus the comparisons that carry
    /// that out.
    /// <para>
    /// Held as its own state because the order survives every rebuild. The bus is re-read four times
    /// a second while play mode runs, and a table that reset its own sorting on each of those would be
    /// unusable.
    /// </para>
    /// </summary>
    internal sealed class EventBusSorting
    {
        /// <summary>The column the table is ordered by.</summary>
        internal EEventColumn Column { get; private set; }

        /// <summary>The direction the column is ordered in.</summary>
        internal ESortOrder Order { get; private set; }

        /// <summary>Hands the order back to the window's own, which is by event type name.</summary>
        internal void Reset() => Order = ESortOrder.Default;

        /// <summary>
        /// Advances the sort after a header was clicked. First click sorts, second reverses, third
        /// hands the order back to the window. A different column always starts that cycle over rather
        /// than inheriting the previous direction.
        /// </summary>
        /// <param name="column">The column whose header was clicked.</param>
        internal void Cycle(EEventColumn column)
        {
            if (Column != column)
            {
                Column = column;
                Order = ESortOrder.Ascending;

                return;
            }

            Order = Order switch
            {
                ESortOrder.Ascending => ESortOrder.Descending,
                ESortOrder.Descending => ESortOrder.Default,
                _ => ESortOrder.Ascending
            };
        }

        /// <summary>Orders two events against each other.</summary>
        /// <param name="first">The first event.</param>
        /// <param name="second">The second event.</param>
        /// <returns>The comparison result, in the current direction.</returns>
        internal int CompareEvents(EventTypeEntry first, EventTypeEntry second)
        {
            if (Order == ESortOrder.Default)
                return Ordinal(first.TypeName, second.TypeName);

            int result = Column switch
            {
                EEventColumn.State => first.Handlers.Count.CompareTo(second.Handlers.Count),
                _ => Ordinal(first.TypeName, second.TypeName)
            };

            if (result == 0)
                result = Ordinal(first.TypeName, second.TypeName);

            return Direct(result);
        }

        /// <summary>Orders two subscribers of the same event against each other.</summary>
        /// <param name="first">The first subscriber.</param>
        /// <param name="second">The second subscriber.</param>
        /// <returns>The comparison result, in the current direction.</returns>
        internal int CompareHandlers(HandlerEntry first, HandlerEntry second)
        {
            if (Order == ESortOrder.Default)
                return 0;

            int result = Column switch
            {
                EEventColumn.Handler => Ordinal(first.MethodName, second.MethodName),
                EEventColumn.State => first.State.CompareTo(second.State),
                EEventColumn.Target => Ordinal(first.TargetName, second.TargetName),
                _ => Ordinal(first.SubscriberName, second.SubscriberName)
            };

            // Rows that tie fall back to the subscribing type, so a column with few distinct values
            // does not let its rows swap places between two reads a quarter of a second apart.
            if (result == 0)
                result = Ordinal(first.SubscriberName, second.SubscriberName);

            return Direct(result);
        }

        private static int Ordinal(string first, string second)
            => string.Compare(first, second, StringComparison.Ordinal);

        private int Direct(int result) => Order == ESortOrder.Descending
            ? -result
            : result;
    }
}