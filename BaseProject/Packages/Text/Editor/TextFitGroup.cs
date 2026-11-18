using System;
using System.Collections.Generic;
using System.Linq;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Problems that belong on one card. Grouped by text, a text that fails in five languages is one entry to fix
    /// instead of five. Grouped by box, the many names that are tight in the same box are one card, too.
    /// </summary>
    internal sealed class TextFitGroup
    {
        private const string KeySeparator = "\n";

        private TextFitGroup(TextSample sample, bool isByBox)
        {
            Sample = sample;
            IsByBox = isByBox;
        }

        /// <summary>True for a group of every text in one box, false for one text in every language.</summary>
        internal bool IsByBox { get; }

        /// <summary>The problems, in the order they were found.</summary>
        internal List<TextFitIssue> Issues { get; } = new();

        /// <summary>The sample of the first problem, for the box, the source and the entry to edit.</summary>
        internal TextSample Sample { get; }

        /// <summary>
        /// Groups <paramref name="issues"/> by text and box, keeping the order they came in and one problem per
        /// language.
        /// </summary>
        internal static List<TextFitGroup> Of(IEnumerable<TextFitIssue> issues) => Group(issues, false,
            issue => issue.Sample.Source + KeySeparator + issue.Sample.Box.Location.Key, issue => issue.Locale);

        /// <summary>
        /// Groups <paramref name="issues"/> by box, keeping the order they came in and one problem per text. For a
        /// page with one language, where a row per language would be a single row.
        /// </summary>
        internal static List<TextFitGroup> ByBox(IEnumerable<TextFitIssue> issues) => Group(issues, true,
            issue => issue.Sample.Box.Location.Key, issue => issue.Sample.Source);

        /// <summary>
        /// The problems to show for <paramref name="locale"/> and <paramref name="search"/>: all of them or the one
        /// language picked, narrowed to those whose text or source has the search in it, unless the box path has.
        /// </summary>
        /// <param name="locale">The language code to show or null for every language.</param>
        /// <param name="search">Text to look for or empty to show everything.</param>
        internal List<TextFitIssue> Visible(string locale, string search)
        {
            List<TextFitIssue> visible = locale == null
                ? Issues
                : Issues.Where(issue => issue.Locale == locale).ToList();

            if (string.IsNullOrEmpty(search)
                || Contains(Sample.Box.Location.Path, search))
                return visible;

            return visible.Where(issue => Contains(issue.Sample.Source, search)
                    || Contains(issue.Sample.Text, search))
                .ToList();
        }

        // Two boxes with the same name under one parent share a path, so a card could not tell their rows apart
        // anyway. The first one is the one Show Box selects.
        private static List<TextFitGroup> Group(IEnumerable<TextFitIssue> issues, bool isByBox,
            Func<TextFitIssue, string> groupKey, Func<TextFitIssue, string> rowKey)
        {
            Dictionary<string, TextFitGroup> byKey = new();
            List<TextFitGroup> groups = new();

            foreach (TextFitIssue issue in issues)
            {
                string key = groupKey(issue);

                if (!byKey.TryGetValue(key, out TextFitGroup group))
                {
                    group = new TextFitGroup(issue.Sample, isByBox);
                    byKey[key] = group;
                    groups.Add(group);
                }

                if (group.Issues.Exists(known => rowKey(known) == rowKey(issue)))
                    continue;

                group.Issues.Add(issue);
            }

            return groups;
        }

        private static bool Contains(string text, string search) => text != null
            && text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}