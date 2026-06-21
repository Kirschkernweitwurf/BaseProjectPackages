#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

namespace Base.ToolPackage.Editor.CreateAssetMenuOverview
{
    /// <summary>
    /// Pure filtering and sorting for CreateAssetMenu entries. The input collection is never
    /// modified; a new ordered list is always returned.
    /// </summary>
    public static class CreateAssetQuery
    {
        /// <summary>
        /// Optionally restricts to a single top-level menu, hides package and built-in types
        /// and applies a name/type search term, then sorts by order with the menu name as a
        /// tie-breaker.
        /// </summary>
        public static IReadOnlyList<CreateAssetEntry> Apply(IReadOnlyList<CreateAssetEntry> entries,
            string search, string root, bool includeExternal, bool ascending)
        {
            IEnumerable<CreateAssetEntry> query = entries;

            if (!string.IsNullOrEmpty(root))
                query = query.Where(entry => entry.Root == root);

            if (!includeExternal)
                query = query.Where(entry => entry.Origin == ECreateAssetOrigin.Project);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(entry => Matches(entry, term));
            }

            query = ascending
                ? query.OrderBy(entry => entry.Order).ThenBy(entry => entry.MenuName)
                : query.OrderByDescending(entry => entry.Order).ThenBy(entry => entry.MenuName);

            return query.ToList();
        }

        private static bool Matches(CreateAssetEntry entry, string term)
            => entry.MenuName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                || entry.TypeName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
#endif
