using System;
using System.Collections.Generic;
using System.Linq;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Pure filtering and sorting for execution-order entries. The input collection is
    /// never modified; a new ordered list is always returned.
    /// </summary>
    public static class ExecutionOrderQuery
    {
        /// <summary>
        /// Filters by package visibility and a name/namespace search term, then sorts by
        /// effective order with the type name as a tie-breaker.
        /// </summary>
        public static IReadOnlyList<ExecutionOrderEntry> Apply(IReadOnlyList<ExecutionOrderEntry> entries,
            string search, bool includePackages, bool ascending)
        {
            IEnumerable<ExecutionOrderEntry> query = entries;

            if (!includePackages)
                query = query.Where(entry => !entry.IsPackage);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(entry => Matches(entry, term));
            }

            query = ascending
                ? query.OrderBy(entry => entry.EffectiveOrder).ThenBy(entry => entry.Name)
                : query.OrderByDescending(entry => entry.EffectiveOrder).ThenBy(entry => entry.Name);

            return query.ToList();
        }

        private static bool Matches(ExecutionOrderEntry entry, string term)
        {
            return entry.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                   || entry.Namespace.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}