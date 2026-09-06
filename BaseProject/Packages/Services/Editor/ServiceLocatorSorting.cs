using System;
using Base.EditorUIPackage.Editor;

namespace Base.ServicesPackage.Editor
{
    /// <summary>
    /// Which column the table is ordered by and in which direction, plus the comparison that carries
    /// that out.
    /// <para>
    /// Held as its own state because the order survives every rebuild. The locator is re-read four
    /// times a second while play mode runs, and a table that reset its own sorting on each of those
    /// would be unusable.
    /// </para>
    /// </summary>
    internal sealed class ServiceLocatorSorting
    {
        /// <summary>The column the table is ordered by.</summary>
        internal EServiceColumn Column { get; private set; }

        /// <summary>The direction the column is ordered in.</summary>
        internal ESortOrder Order { get; private set; }

        /// <summary>Hands the order back to the window's own, which is by service type name.</summary>
        internal void Reset() => Order = ESortOrder.Default;

        /// <summary>
        /// Advances the sort after a header was clicked. First click sorts, second reverses, third
        /// hands the order back to the window. A different column always starts that cycle over rather
        /// than inheriting the previous direction.
        /// </summary>
        /// <param name="column">The column whose header was clicked.</param>
        internal void Cycle(EServiceColumn column)
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

        /// <summary>Orders two registrations against each other.</summary>
        /// <param name="first">The first registration.</param>
        /// <param name="second">The second registration.</param>
        /// <returns>The comparison result, in the current direction.</returns>
        internal int Compare(ServiceRegistrationEntry first, ServiceRegistrationEntry second)
        {
            if (Order == ESortOrder.Default)
                return Ordinal(first.TypeName, second.TypeName);

            int result = Column switch
            {
                EServiceColumn.Instance => Ordinal(first.InstanceTypeName, second.InstanceTypeName),
                EServiceColumn.Location => Ordinal(first.Location, second.Location),
                EServiceColumn.State => first.State.CompareTo(second.State),
                _ => Ordinal(first.TypeName, second.TypeName)
            };

            // Rows that tie fall back to the name, so a column with few distinct values does not let
            // its rows swap places between two reads a quarter of a second apart.
            if (result == 0)
                result = Ordinal(first.TypeName, second.TypeName);

            return Order == ESortOrder.Descending
                ? -result
                : result;
        }

        private static int Ordinal(string first, string second)
            => string.Compare(first, second, StringComparison.Ordinal);
    }
}