using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// Which column the table is ordered by and in which direction. Clicking a header sorts, a second
    /// click reverses, a third goes back to the order by name.
    /// </summary>
    internal sealed class DataTableSorting
    {
        private const int NoColumn = -1;

        /// <summary>The column the table is ordered by, or -1 for the order by name.</summary>
        internal int ColumnIndex { get; private set; } = NoColumn;

        /// <summary>The direction the column is ordered in.</summary>
        internal ESortOrder Order { get; private set; } = ESortOrder.Default;

        /// <summary>Advances the sort after a header click. A different column always starts ascending.</summary>
        /// <param name="columnIndex">The column whose header was clicked.</param>
        internal void Cycle(int columnIndex)
        {
            if (ColumnIndex != columnIndex)
            {
                ColumnIndex = columnIndex;
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

        /// <summary>Goes back to the order by name.</summary>
        internal void Reset()
        {
            ColumnIndex = NoColumn;
            Order = ESortOrder.Default;
        }

        /// <summary>Orders two rows against each other.</summary>
        /// <param name="first">The first row.</param>
        /// <param name="second">The second row.</param>
        /// <param name="columns">The columns of the table.</param>
        /// <returns>The comparison result, in the current direction.</returns>
        internal int Compare(DataRow first, DataRow second, IReadOnlyList<DataColumn> columns)
        {
            if (Order == ESortOrder.Default
                || ColumnIndex < 0
                || ColumnIndex >= columns.Count)
                return ByName(first, second);

            DataColumn column = columns[ColumnIndex];
            int result = DataCell.Compare(first.CellText(column), second.CellText(column));

            // Ties fall back to the name, so rows with the same value keep a stable order.
            if (result == 0)
                result = ByName(first, second);

            return Order == ESortOrder.Descending
                ? -result
                : result;
        }

        private static int ByName(DataRow first, DataRow second)
            => string.Compare(first.Asset.name, second.Asset.name, StringComparison.OrdinalIgnoreCase);
    }
}