using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// The widths of a table's columns, where each one starts, and the draggable lines between them in
    /// the header. Widths are remembered per table and column between sessions.
    /// </summary>
    internal sealed class DataTableColumns
    {
        private const float MinColumnWidth = 40f;
        private const string WidthKeyFormat = "Base.DataHubPackage.Columns.{0}.{1}";

        private readonly List<DataColumn> _columns;
        private readonly EditorColumnDividers _dividers = new();
        private readonly string _tableKey;

        /// <summary>Takes over the columns of a table and restores their saved widths.</summary>
        /// <param name="tableKey">Identifies the table across sessions.</param>
        /// <param name="columns">The columns, in display order.</param>
        internal DataTableColumns(string tableKey, List<DataColumn> columns)
        {
            _tableKey = tableKey;
            _columns = columns;

            foreach (DataColumn column in columns)
            {
                column.Width = EditorPrefs.GetFloat(WidthKey(column), column.DefaultWidth);

                if (column.Group != null)
                    HasGroups = true;
            }
        }

        /// <summary>The columns, in display order.</summary>
        internal IReadOnlyList<DataColumn> All => _columns;

        /// <summary>True when a column sits inside a struct, so the header needs a second line for the group.</summary>
        internal bool HasGroups { get; }

        /// <summary>The width of every column together.</summary>
        internal float TotalWidth
        {
            get
            {
                float total = 0f;

                foreach (DataColumn column in _columns)
                    total += column.Width;

                return total;
            }
        }

        /// <summary>Where a column starts, from the left edge of the table.</summary>
        /// <param name="index">The column index.</param>
        /// <returns>The x offset.</returns>
        internal float Left(int index)
        {
            float left = 0f;

            for (int i = 0; i < index; i++)
                left += _columns[i].Width;

            return left;
        }

        /// <summary>
        /// Whether a point in the header is on a divider. A click there belongs to the resize, not to the
        /// title under it.
        /// </summary>
        /// <param name="x">The x to test, in header coordinates.</param>
        /// <param name="offset">How far the table is scrolled to the left, as a negative number.</param>
        /// <returns>True when a press there would start a resize.</returns>
        internal bool IsOverDivider(float x, float offset)
        {
            float right = offset;

            foreach (DataColumn column in _columns)
            {
                right += column.Width;

                if (EditorColumnDividers.IsOver(x, right))
                    return true;
            }

            return false;
        }

        /// <summary>Draws the divider after every column and resizes the column when one is dragged.</summary>
        /// <param name="header">The header area, in its own coordinates.</param>
        /// <param name="offset">How far the table is scrolled to the left, as a negative number.</param>
        internal void HandleDividers(Rect header, float offset)
        {
            float left = offset;

            for (int i = 0; i < _columns.Count; i++)
            {
                DataColumn column = _columns[i];
                EEditorDividerAction action = _dividers.Handle(i, left + column.Width, header, out float mouseX);

                if (action != EEditorDividerAction.None)
                    column.Width = Mathf.Max(MinColumnWidth, mouseX - left);

                if (action == EEditorDividerAction.Released)
                    EditorPrefs.SetFloat(WidthKey(column), column.Width);

                left += column.Width;
            }
        }

        /// <summary>Drops a resize in progress, for a mouse released outside the window.</summary>
        internal void Cancel() => _dividers.Cancel();

        private string WidthKey(DataColumn column) => string.Format(WidthKeyFormat, _tableKey, column.Key);
    }
}