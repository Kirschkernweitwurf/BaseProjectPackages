using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Worst-case values for the placeholders of a Smart String. Numbers hit every plural form of every language
    /// (Russian needs one, few and many: 1, 3, 5, 21) plus a wide number. A source can hand in its own values for
    /// placeholders that are not numbers. The placeholders are read from the entry of the language being tested,
    /// never from English.
    /// </summary>
    /// <remarks>
    /// Every combination grows too fast: a tooltip with five numbers would need 7,776 renders per language. So every
    /// placeholder first takes the value that makes the text longest, then each placeholder runs through all of its
    /// values while the others keep theirs. That still shows every plural form of every placeholder.
    /// </remarks>
    public static class SmartTestArguments
    {
        private const string Separator = ", ";

        private static readonly object[] Numbers = { 1, 2, 3, 5, 21, 100 };
        private static readonly Regex Placeholder = new(@"\{(\w+)", RegexOptions.Compiled);

        /// <summary>The test values for the placeholders in <paramref name="entryText"/>, each with its text.</summary>
        /// <param name="entryText">The Smart String of the language being tested.</param>
        /// <param name="format">Formats the entry with a set of values, like the game does.</param>
        /// <param name="valuesFor">
        /// Values for a placeholder by name or null to use numbers. Null itself uses numbers everywhere.
        /// </param>
        public static List<(Dictionary<string, object> Values, string Text)> For(string entryText,
            Func<Dictionary<string, object>, string> format, Func<string, IEnumerable<object>> valuesFor = null)
        {
            Dictionary<string, List<object>> options = Names(entryText)
                .ToDictionary(name => name, name => (valuesFor?.Invoke(name) ?? Numbers).ToList());
            Dictionary<string, object> longest = options.ToDictionary(pair => pair.Key, pair => pair.Value[0]);

            foreach ((string name, List<object> values) in options)
                longest[name] = values.OrderByDescending(value => format(With(longest, name, value)).Length).First();

            List<(Dictionary<string, object>, string)> sets = new() { (longest, format(longest)) };

            foreach ((string name, List<object> values) in options)
            {
                foreach (object value in values.Where(value => !Equals(value, longest[name])))
                {
                    Dictionary<string, object> set = With(longest, name, value);
                    sets.Add((set, format(set)));
                }
            }

            return sets;
        }

        /// <summary>The values of <paramref name="set"/> for the report, like "count=3, name=Sam".</summary>
        public static string Describe(Dictionary<string, object> set)
        {
            List<string> parts = new();

            foreach (KeyValuePair<string, object> pair in set)
                parts.Add(pair.Key + "=" + pair.Value);

            return string.Join(Separator, parts);
        }

        private static Dictionary<string, object> With(Dictionary<string, object> set, string name, object value)
            => new(set) { [name] = value };

        private static IEnumerable<string> Names(string entryText)
        {
            HashSet<string> seen = new();

            foreach (Match match in Placeholder.Matches(entryText ?? string.Empty))
            {
                if (seen.Add(match.Groups[1].Value))
                    yield return match.Groups[1].Value;
            }
        }
    }
}