using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.Shared;

namespace Base.ToolsPackage.Editor.CommandPalette
{
    /// <summary>
    /// Filters and ranks the index for one keystroke. The fuzzy score is the base, on top of it
    /// come pinned entries, matching tags and how often and how recently a command was used.
    /// <para>
    /// An entry whose path does not match at all can still get in on its own search terms, at one
    /// fixed score that lands below what a real path match earns.
    /// </para>
    /// </summary>
    internal static class CommandQuery
    {
        private const int DayBonus = 40;
        private const int DaysPerWeek = 7;
        private const int HourBonus = 90;
        private const int KeywordScore = 40;
        private const int MaxResults = 250;
        private const int MaxUsageBonus = 250;
        private const int PinnedBonus = 1000;
        private const int TagTermBonus = 80;
        private const int UsageBonus = 25;
        private const int WeekBonus = 15;

        private static readonly List<int> MatchBuffer = new();

        /// <summary>Rebuilds the result list for the given filter.</summary>
        /// <param name="entries">Every known command.</param>
        /// <param name="filter">The parsed search box content.</param>
        /// <param name="projectOnly">Whether package and built-in commands are hidden.</param>
        /// <param name="results">The list that receives the ranked matches.</param>
        internal static void Run(IReadOnlyList<CommandEntry> entries, CommandFilter filter, bool projectOnly,
            List<CommandMatch> results)
        {
            results.Clear();

            CommandTagStore tags = CommandTagStore.instance;
            CommandUsageStore usage = CommandUsageStore.instance;
            long now = DateTime.UtcNow.Ticks;

            foreach (CommandEntry entry in entries)
            {
                if (filter.Kind.HasValue && entry.Kind != filter.Kind.Value)
                    continue;

                if (projectOnly && entry.Origin != EAssetOrigin.Project)
                    continue;

                IReadOnlyList<string> assigned = tags.TagsFor(entry.Id);

                if (!HasAllTags(assigned, filter.Tags))
                    continue;

                if (!CommandMatcher.TryMatch(entry, filter.Term, MatchBuffer, out int score))
                {
                    if (!MatchesKeywords(entry, filter.Term))
                        continue;

                    // One flat score for every keyword hit. Where inside a keyword the term landed
                    // says nothing worth ranking by, which is the whole of what the fuzzy score
                    // measures, and none of it survives a match the row cannot even highlight.
                    score = KeywordScore;
                }

                bool pinned = tags.IsPinned(entry.Id);

                if (pinned)
                    score += PinnedBonus;

                if (filter.Term.Length > 0 && ContainsTerm(assigned, filter.Term))
                    score += TagTermBonus;

                score += UsageScore(usage, entry.Id, now);

                results.Add(new CommandMatch(entry, score, pinned));
            }

            results.Sort(Compare);

            if (results.Count > MaxResults)
                results.RemoveRange(MaxResults, results.Count - MaxResults);
        }

        private static int Compare(CommandMatch left, CommandMatch right)
        {
            int byScore = right.Score.CompareTo(left.Score);

            return byScore != 0
                ? byScore
                : string.CompareOrdinal(left.Entry.Path, right.Entry.Path);
        }

        private static bool ContainsTerm(IReadOnlyList<string> assigned, string term)
        {
            foreach (string tag in assigned)
            {
                if (tag.IndexOf(term, StringComparison.Ordinal) >= 0)
                    return true;
            }

            return false;
        }

        private static bool HasAllTags(IReadOnlyList<string> assigned, IReadOnlyList<string> required)
        {
            if (required.Count == 0)
                return true;

            foreach (string tag in required)
            {
                if (!StartsWithAny(assigned, tag))
                    return false;
            }

            return true;
        }

        private static bool MatchesKeywords(CommandEntry entry, string term)
        {
            if (term.Length == 0
                || entry.LowerKeywords.Length == 0)
                return false;

            return entry.LowerKeywords.IndexOf(term, StringComparison.Ordinal) >= 0;
        }

        private static bool StartsWithAny(IReadOnlyList<string> assigned, string prefix)
        {
            foreach (string tag in assigned)
            {
                if (tag.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static int UsageScore(CommandUsageStore usage, string id, long now)
        {
            int count = usage.CountFor(id);

            if (count == 0)
                return 0;

            int score = Math.Min(MaxUsageBonus, count * UsageBonus);
            long elapsed = now - usage.LastUsedFor(id);

            if (elapsed < TimeSpan.TicksPerHour)
                return score + HourBonus;

            if (elapsed < TimeSpan.TicksPerDay)
                return score + DayBonus;

            return elapsed < TimeSpan.TicksPerDay * DaysPerWeek
                ? score + WeekBonus
                : score;
        }
    }
}