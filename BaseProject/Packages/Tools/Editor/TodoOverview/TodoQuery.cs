using System;
using System.Collections.Generic;
using Base.EditorUiPackage;
using Base.ToolPackage.Editor.TodoOverview.Model;
using Base.ToolPackage.Editor.TodoOverview.Scanning;
using UnityEngine;

namespace Base.ToolPackage.Editor.TodoOverview
{
    /// <summary>
    /// Narrows the scanned items down to what the user asked for and lays them out in sections. Pure
    /// and free of any UI, so what the list shows can be reasoned about without a window around it.
    /// </summary>
    internal static class TodoQuery
    {
        /// <summary>The section and dropdown entry items without a responsible person land in.</summary>
        internal const string Unassigned = "Unassigned";

        private static readonly char[] SearchSeparators =
        {
            ' '
        };

        /// <summary>Filters, sorts and groups the items.</summary>
        /// <param name="entries">Everything the last scan found.</param>
        /// <param name="filter">What the user narrowed the list down to.</param>
        /// <param name="palette">The keyword colors and their configured order.</param>
        /// <returns>The sections to draw, already sorted.</returns>
        internal static List<TodoGroup> Build(IReadOnlyList<TodoEntry> entries, TodoFilter filter,
            TodoPalette palette)
        {
            Dictionary<string, List<TodoEntry>> buckets = new(StringComparer.Ordinal);
            List<string> labels = new();

            foreach (TodoEntry entry in entries)
            {
                if (!Matches(entry, filter))
                    continue;

                string label = GroupLabel(entry, filter.Grouping);

                if (!buckets.TryGetValue(label, out List<TodoEntry> bucket))
                {
                    bucket = new List<TodoEntry>();
                    buckets.Add(label, bucket);
                    labels.Add(label);
                }

                bucket.Add(entry);
            }

            labels.Sort((left, right) => CompareLabels(left, right, filter.Grouping, palette));

            List<TodoGroup> groups = new();

            foreach (string label in labels)
            {
                List<TodoEntry> bucket = buckets[label];
                bucket.Sort((left, right) => Compare(left, right, filter, palette));

                groups.Add(new TodoGroup(label, Accent(label, filter.Grouping, palette), bucket));
            }

            return groups;
        }

        /// <summary>Collects every responsible person that appears in the items.</summary>
        /// <param name="entries">Everything the last scan found.</param>
        /// <returns>The owners in alphabetical order, with the unassigned bucket last.</returns>
        internal static List<string> CollectOwners(IReadOnlyList<TodoEntry> entries)
        {
            SortedSet<string> owners = new(StringComparer.OrdinalIgnoreCase);
            bool hasUnassigned = false;

            foreach (TodoEntry entry in entries)
            {
                if (entry.Owner.Length == 0)
                {
                    hasUnassigned = true;
                    continue;
                }

                owners.Add(entry.Owner);
            }

            List<string> result = new(owners);

            if (hasUnassigned)
                result.Add(Unassigned);

            return result;
        }

        /// <summary>Counts how many items carry each keyword.</summary>
        /// <param name="entries">Everything the last scan found.</param>
        /// <returns>The item count per keyword.</returns>
        internal static Dictionary<string, int> CountKeywords(IReadOnlyList<TodoEntry> entries)
        {
            Dictionary<string, int> counts = new(StringComparer.OrdinalIgnoreCase);

            foreach (TodoEntry entry in entries)
            {
                counts.TryGetValue(entry.Keyword, out int count);
                counts[entry.Keyword] = count + 1;
            }

            return counts;
        }

        /// <summary>Counts the items whose date has passed.</summary>
        /// <param name="entries">Everything the last scan found.</param>
        /// <returns>The number of overdue items.</returns>
        internal static int CountOverdue(IReadOnlyList<TodoEntry> entries)
        {
            int count = 0;

            foreach (TodoEntry entry in entries)
            {
                if (TodoDateParser.Resolve(entry.Date) == ETodoDateState.Overdue)
                    count++;
            }

            return count;
        }

        private static Color Accent(string label, ETodoGrouping grouping, TodoPalette palette)
            => grouping == ETodoGrouping.Keyword
                ? palette.Of(label)
                : EditorPalette.Accent;

        private static string GroupLabel(TodoEntry entry, ETodoGrouping grouping) => grouping switch
        {
            ETodoGrouping.File => entry.AssetPath,
            ETodoGrouping.Keyword => entry.Keyword,
            ETodoGrouping.Owner => OwnerOf(entry),
            _ => string.Empty
        };

        private static string OwnerOf(TodoEntry entry) => entry.Owner.Length == 0
            ? Unassigned
            : entry.Owner;

        private static bool Matches(TodoEntry entry, TodoFilter filter)
        {
            if (!filter.IsKeywordVisible(entry.Keyword))
                return false;

            if (filter.OverdueOnly
                && TodoDateParser.Resolve(entry.Date) != ETodoDateState.Overdue)
                return false;

            if (filter.Owner != TodoFilter.AnyOwner
                && !string.Equals(OwnerOf(entry), filter.Owner, StringComparison.OrdinalIgnoreCase))
                return false;

            return MatchesSearch(entry, filter.Search);
        }

        // Every word of the query has to appear somewhere in the item, which is what lets a search
        // like "jonny input" find one person's items in one file.
        private static bool MatchesSearch(TodoEntry entry, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return true;

            string[] words = search.ToLowerInvariant()
                .Split(SearchSeparators,
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                if (!entry.SearchText.Contains(word, StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        private static int CompareLabels(string left, string right, ETodoGrouping grouping, TodoPalette palette)
        {
            if (grouping != ETodoGrouping.Keyword)
                return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);

            int order = palette.OrderOf(left).CompareTo(palette.OrderOf(right));

            return order != 0
                ? order
                : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        // The location is the tiebreaker for every other order, so two items that compare equal
        // still come out in the order they sit in their file.
        private static int Compare(TodoEntry left, TodoEntry right, TodoFilter filter, TodoPalette palette)
        {
            int result = filter.Sort switch
            {
                ETodoSort.Keyword => palette.OrderOf(left.Keyword).CompareTo(palette.OrderOf(right.Keyword)),
                ETodoSort.Message => string.Compare(left.Message, right.Message, StringComparison.OrdinalIgnoreCase),
                ETodoSort.Owner => CompareOwners(left, right),
                ETodoSort.Date => CompareDates(left, right),
                _ => 0
            };

            if (result == 0)
                result = CompareLocations(left, right);

            return filter.Descending
                ? -result
                : result;
        }

        private static int CompareOwners(TodoEntry left, TodoEntry right)
        {
            bool leftHas = left.Owner.Length > 0;
            bool rightHas = right.Owner.Length > 0;

            if (leftHas != rightHas)
                return leftHas
                    ? -1
                    : 1;

            return string.Compare(left.Owner, right.Owner, StringComparison.OrdinalIgnoreCase);
        }

        // An item without a date says nothing about when it is due, so it sorts behind every item
        // that does, rather than in front of the oldest one.
        private static int CompareDates(TodoEntry left, TodoEntry right)
        {
            if (left.Date.HasValue && right.Date.HasValue)
                return left.Date.Value.CompareTo(right.Date.Value);

            if (left.Date.HasValue == right.Date.HasValue)
                return 0;

            return left.Date.HasValue
                ? -1
                : 1;
        }

        private static int CompareLocations(TodoEntry left, TodoEntry right)
        {
            int path = string.Compare(left.AssetPath, right.AssetPath, StringComparison.Ordinal);

            return path != 0
                ? path
                : left.Line.CompareTo(right.Line);
        }
    }
}