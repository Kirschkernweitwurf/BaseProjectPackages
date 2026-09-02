using System;
using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.CommandPalette
{
    /// <summary>
    /// Fuzzy subsequence matching over the whole menu path. Pure and free of any UI, so the
    /// ranking rules live in one place and stay testable.
    /// </summary>
    internal static class CommandMatcher
    {
        private const int ConsecutiveBonus = 8;
        private const int ExactLeafBonus = 200;
        private const int LeafBonus = 6;
        private const int LeafSubstringBonus = 120;
        private const int LengthDivisor = 8;
        private const int MatchScore = 4;
        private const int MaxGapPenalty = 30;
        private const char PathSeparator = '/';
        private const int SegmentBonus = 12;
        private const char SegmentSpace = ' ';
        private const int SubstringBonus = 60;

        /// <summary>
        /// Scores an entry against a term. Every character of the term has to appear in the path
        /// in order; matches that sit next to each other, at the start of a segment or inside the
        /// last segment score higher, and long paths are pushed down slightly.
        /// </summary>
        /// <param name="entry">The entry to score.</param>
        /// <param name="term">Lowercase search term. An empty term matches everything.</param>
        /// <param name="matches">Receives the index of every matched character.</param>
        /// <param name="score">The resulting score, higher is better.</param>
        /// <returns><c>true</c> when the term matches the entry.</returns>
        internal static bool TryMatch(CommandEntry entry, string term, List<int> matches, out int score)
        {
            matches.Clear();
            score = 0;

            if (term.Length == 0)
                return true;

            string path = entry.LowerPath;
            int cursor = 0;
            int previous = -2;
            int gaps = 0;

            foreach (char character in term)
            {
                int found = path.IndexOf(character, cursor);

                if (found < 0)
                {
                    matches.Clear();
                    score = 0;

                    return false;
                }

                matches.Add(found);
                score += MatchScore + BonusAt(path, found, previous, entry.LeafStart);
                gaps += found - cursor;

                previous = found;
                cursor = found + 1;
            }

            score -= Math.Min(MaxGapPenalty, gaps);
            score += ContiguousBonus(entry, path, term);
            score -= path.Length / LengthDivisor;

            return true;
        }

        private static int BonusAt(string path, int index, int previous, int leafStart)
        {
            int bonus = 0;

            if (index == previous + 1)
                bonus += ConsecutiveBonus;

            if (index == 0 || path[index - 1] == PathSeparator || path[index - 1] == SegmentSpace)
                bonus += SegmentBonus;

            if (index >= leafStart)
                bonus += LeafBonus;

            return bonus;
        }

        private static int ContiguousBonus(CommandEntry entry, string path, string term)
        {
            int leafLength = path.Length - entry.LeafStart;

            if (leafLength == term.Length
                && string.CompareOrdinal(path, entry.LeafStart, term, 0, term.Length) == 0)
                return ExactLeafBonus + LeafSubstringBonus;

            int contiguous = path.IndexOf(term, StringComparison.Ordinal);

            if (contiguous < 0)
                return 0;

            return contiguous >= entry.LeafStart
                ? LeafSubstringBonus
                : SubstringBonus;
        }
    }
}