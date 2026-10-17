using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// The widths of a table's columns, where each one starts, and the draggable lines between them in
    /// the header. Widths are remembered per table and column between sessions. The first columns are
    /// frozen: they stay put while the rest scroll sideways, so a row never loses its name.
    /// </summary>
    internal sealed class DataTableColumns
    {
        /// <summary>How many columns at the left stay put while the others scroll: the name and the check.</summary>
        internal const int FrozenCount = 2;

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
                column.HasSavedWidth = EditorPrefs.HasKey(WidthKey(column));
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

        /// <summary>The width of the frozen columns together.</summary>
        internal float FrozenWidth => Left(Mathf.Min(FrozenCount, _columns.Count));

        /// <summary>True when the column at <paramref name="index"/> stays put while the others scroll.</summary>
        /// <param name="index">The column index.</param>
        /// <returns>True for a frozen column.</returns>
        internal static bool IsFrozen(int index) => index < FrozenCount;

        /// <summary>Where a column starts on screen, from the left edge of the table, after scrolling.</summary>
        /// <param name="index">The column index.</param>
        /// <param name="scrollX">How far the table is scrolled sideways.</param>
        /// <returns>The x offset.</returns>
        internal float ScreenLeft(int index, float scrollX) => IsFrozen(index)
            ? Left(index)
            : Left(index) - scrollX;

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
        /// <param name="scrollX">How far the table is scrolled sideways.</param>
        /// <returns>True when a press there would start a resize.</returns>
        internal bool IsOverDivider(float x, float scrollX)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                if (IsDividerShown(i, scrollX)
                    && EditorColumnDividers.IsOver(x, ScreenLeft(i, scrollX) + _columns[i].Width))
                    return true;
            }

            return false;
        }

        /// <summary>Draws the divider after every column and resizes the column when one is dragged.</summary>
        /// <param name="header">The header area, in its own coordinates.</param>
        /// <param name="scrollX">How far the table is scrolled sideways.</param>
        internal void HandleDividers(Rect header, float scrollX)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                if (!IsDividerShown(i, scrollX))
                    continue;

                DataColumn column = _columns[i];
                float left = ScreenLeft(i, scrollX);
                Rect area = IsInsideGroup(i)
                    ? new Rect(header.x, header.y + EditorTableStyles.HeaderHeight, header.width,
                        Mathf.Max(0f, header.height - EditorTableStyles.HeaderHeight))
                    : header;

                EEditorDividerAction action = _dividers.Handle(i, left + column.Width, area, out float mouseX);

                if (action != EEditorDividerAction.None)
                    column.Width = Mathf.Max(MinColumnWidth, mouseX - left);

                if (action == EEditorDividerAction.Released)
                {
                    column.HasSavedWidth = true;
                    EditorPrefs.SetFloat(WidthKey(column), column.Width);
                }
            }
        }

        /// <summary>
        /// Widens every column the user never resized until its title fits, sort arrow included. A type
        /// based default width cannot know that a number column is called "Standard Card Count".
        /// </summary>
        /// <param name="headerStyle">The style the titles are drawn with.</param>
        /// <param name="padding">The space left and right of a title inside its cell.</param>
        internal void FitTitles(GUIStyle headerStyle, float padding)
        {
            foreach (DataColumn column in _columns)
            {
                if (column.HasSavedWidth)
                    continue;

                float title = headerStyle.CalcSize(new GUIContent(column.Header)).x
                    + padding * 2f
                    + EditorTableStyles.HeaderArrowGap
                    + EditorMetrics.SortArrowWidth;

                column.Width = Mathf.Max(column.Width, title);
            }
        }

        /// <summary>Drops a resize in progress, for a mouse released outside the window.</summary>
        internal void Cancel() => _dividers.Cancel();

        // A scrolled column whose edge slid under the frozen ones cannot be grabbed there.
        private bool IsDividerShown(int index, float scrollX) => IsFrozen(index)
            || ScreenLeft(index, scrollX) + _columns[index].Width > FrozenWidth;

        // A divider between two columns of one struct stops below the group title, which it would cut in half.
        private bool IsInsideGroup(int index) => index + 1 < _columns.Count
            && _columns[index].Group != null
            && _columns[index].Group == _columns[index + 1].Group;

        private string WidthKey(DataColumn column) => string.Format(WidthKeyFormat, _tableKey, column.Key);
    }
}