using System;
using System.Collections.Generic;
using System.Linq;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>
    /// Pure filtering and sorting for menu item entries. The input collection is never
    /// modified; a new ordered list is always returned.
    /// </summary>
    public static class MenuItemQuery
    {
        /// <summary>
        /// Optionally restricts to a single top-level menu, hides package and built-in
        /// items, hides validation functions and applies a path/member search term, then
        /// sorts by priority with the menu path as a tie-breaker.
        /// </summary>
        public static IReadOnlyList<MenuItemEntry> Apply(IReadOnlyList<MenuItemEntry> entries,
            string search, string root, bool includeExternal, bool hideValidation, bool ascending)
        {
            IEnumerable<MenuItemEntry> query = entries;

            if (!string.IsNullOrEmpty(root))
                query = query.Where(entry => entry.Root == root);

            if (!includeExternal)
                query = query.Where(entry => entry.Origin == EMenuItemOrigin.Project);

            if (hideValidation)
                query = query.Where(entry => !entry.IsValidation);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(entry => Matches(entry, term));
            }

            query = ascending
                ? query.OrderBy(entry => entry.Priority).ThenBy(entry => entry.MenuPath)
                : query.OrderByDescending(entry => entry.Priority).ThenBy(entry => entry.MenuPath);

            return query.ToList();
        }

        private static bool Matches(MenuItemEntry entry, string term)
        {
            return entry.MenuPath.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                   || entry.Member.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}