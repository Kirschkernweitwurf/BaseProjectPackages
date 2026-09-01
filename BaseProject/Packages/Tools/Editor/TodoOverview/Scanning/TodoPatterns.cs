using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.TodoOverview.Model;
using Base.ToolPackage.Editor.TodoOverview.Settings;
using UnityEngine;

namespace Base.ToolPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// The compiled form of everything the settings declare: one regular expression for the keywords,
    /// one per metadata notation, and the date formats. Built once per scan and handed to the parser,
    /// so a pattern is compiled once instead of once per file.
    /// <para>
    /// A pattern the user typed can be invalid or slow. Invalid ones are reported once and dropped, and
    /// every match runs against a timeout, so a bad expression cannot take the editor down with it.
    /// </para>
    /// </summary>
    internal sealed class TodoPatterns
    {
        /// <summary>The name of the group a metadata pattern reports the date in.</summary>
        internal const string DateGroup = "date";

        /// <summary>The name of the group a metadata pattern reports the responsible person in.</summary>
        internal const string OwnerGroup = "owner";

        private const string KeywordPrefix = @"(?<![\w])(";
        private const string KeywordSeparator = "|";
        private const string KeywordSuffix = @")(?![\w])";
        private const string PatternWarning = "Todo Overview: ignoring the invalid pattern \"{0}\". {1}";
        private const int TimeoutSeconds = 2;

        /// <summary>How far an item reaches past the line its keyword sits on.</summary>
        internal ETodoContinuation Continuation { get; }

        /// <summary>Matches any enabled keyword as a whole word, or null when none is enabled.</summary>
        internal Regex Keywords { get; }

        /// <summary>The date formats a date in an item is read with.</summary>
        internal string[] DateFormats { get; }

        /// <summary>The patterns that read the owner and the date, in the order they are tried.</summary>
        internal IReadOnlyList<Regex> Metadata => _metadata;

        /// <summary>Whether there is at least one keyword to look for.</summary>
        internal bool HasKeywords => Keywords != null;

        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

        private readonly Dictionary<string, string> _canonical = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<Regex> _metadata = new();
        private readonly List<string> _words = new();

        private TodoPatterns(TodoSettings settings)
        {
            Continuation = settings.Continuation;
            DateFormats = BuildFormats(settings.DateFormats);

            foreach (TodoTag tag in settings.Tags)
            {
                if (!tag.Enabled || string.IsNullOrWhiteSpace(tag.Keyword))
                    continue;

                string keyword = tag.Keyword.Trim();

                if (!_canonical.TryAdd(keyword, keyword))
                    continue;

                _words.Add(keyword);
            }

            Keywords = BuildKeywords(_words, settings.CaseSensitive);

            foreach (string pattern in settings.MetadataPatterns)
                AddMetadata(pattern);
        }

        /// <summary>Builds the compiled patterns from the project settings.</summary>
        /// <param name="settings">The settings to compile.</param>
        /// <returns>The compiled patterns.</returns>
        internal static TodoPatterns Create(TodoSettings settings) => new(settings);

        /// <summary>
        /// Whether a file mentions any keyword at all. Reading a file is cheap next to lexing it, so
        /// this plain text check is what keeps a scan over a whole project quick.
        /// </summary>
        /// <param name="source">The full text of the file.</param>
        /// <returns><c>true</c> when at least one keyword appears somewhere in the file.</returns>
        internal bool ContainsKeyword(string source)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            foreach (string word in _words)
            {
                if (source.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the keyword in the casing its tag is configured with, so a lower case todo and a
        /// shouted TODO end up in the same section.
        /// </summary>
        /// <param name="matched">The text the keyword pattern matched.</param>
        /// <returns>The configured spelling, or the matched text when the tag is gone.</returns>
        internal string Resolve(string matched) => _canonical.TryGetValue(matched, out string keyword)
            ? keyword
            : matched;

        private static string[] BuildFormats(IReadOnlyList<string> formats)
        {
            List<string> valid = new();

            foreach (string format in formats)
            {
                if (!string.IsNullOrWhiteSpace(format))
                    valid.Add(format.Trim());
            }

            return valid.ToArray();
        }

        private static Regex BuildKeywords(List<string> words, bool caseSensitive)
        {
            if (words.Count == 0)
                return null;

            string[] escaped = new string[words.Count];

            for (int i = 0; i < words.Count; i++)
                escaped[i] = Regex.Escape(words[i]);

            RegexOptions options = RegexOptions.CultureInvariant;

            if (!caseSensitive)
                options |= RegexOptions.IgnoreCase;

            return new Regex(KeywordPrefix + string.Join(KeywordSeparator, escaped) + KeywordSuffix, options,
                Timeout);
        }

        private void AddMetadata(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return;

            try
            {
                _metadata.Add(new Regex(pattern, RegexOptions.CultureInvariant, Timeout));
            }
            catch (ArgumentException exception)
            {
                Debug.LogWarning(string.Format(PatternWarning, pattern, exception.Message));
            }
        }
    }
}