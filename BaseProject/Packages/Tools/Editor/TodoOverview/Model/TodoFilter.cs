using System;
using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// Everything the user narrowed the list down to. Kept in one object so the toolbar writes to the
    /// same place the query reads from, and so the window does not carry a field per control.
    /// </summary>
    internal sealed class TodoFilter
    {
        /// <summary>The value of the owner dropdown that means every owner.</summary>
        internal const string AnyOwner = "";

        /// <summary>The order the list falls back to, which is the order the files are read in.</summary>
        internal const ETodoSort DefaultSort = ETodoSort.Location;

        /// <summary>The text typed into the search field. Every word in it has to match.</summary>
        internal string Search { get; set; } = string.Empty;

        /// <summary>The owner the list is limited to, or <see cref="AnyOwner"/> for all of them.</summary>
        internal string Owner { get; set; } = AnyOwner;

        /// <summary>The order items are listed in.</summary>
        internal ETodoSort Sort { get; set; }

        /// <summary>Whether that order runs backwards.</summary>
        internal bool Descending { get; set; }

        /// <summary>What the list is split into sections by.</summary>
        internal ETodoGrouping Grouping { get; set; }

        /// <summary>Whether only items whose date has passed are shown.</summary>
        internal bool OverdueOnly { get; set; }

        private readonly HashSet<string> _hiddenKeywords = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Applies a click on a column title. A new column sorts by itself, clicking it again turns
        /// the order around, and a third click drops back to the default rather than leaving the
        /// list in an order nobody asked for.
        /// </summary>
        /// <param name="sort">The order the clicked column stands for.</param>
        internal void ApplySortClick(ETodoSort sort)
        {
            if (Sort != sort)
            {
                Sort = sort;
                Descending = false;

                return;
            }

            if (!Descending)
            {
                Descending = true;

                return;
            }

            Sort = DefaultSort;
            Descending = false;
        }

        /// <summary>Sets the order and starts it off forwards again.</summary>
        /// <param name="sort">The order to list the items in.</param>
        internal void SetSort(ETodoSort sort)
        {
            Sort = sort;
            Descending = false;
        }

        /// <summary>Whether items with the given keyword are currently shown.</summary>
        /// <param name="keyword">The keyword to test.</param>
        /// <returns><c>true</c> when the keyword is not filtered out.</returns>
        internal bool IsKeywordVisible(string keyword) => !_hiddenKeywords.Contains(keyword);

        /// <summary>Shows or hides every item with the given keyword.</summary>
        /// <param name="keyword">The keyword to toggle.</param>
        internal void ToggleKeyword(string keyword)
        {
            if (!_hiddenKeywords.Add(keyword))
                _hiddenKeywords.Remove(keyword);
        }
    }
}