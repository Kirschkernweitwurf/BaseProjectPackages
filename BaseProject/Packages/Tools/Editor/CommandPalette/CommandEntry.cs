using System;
using System.Collections.Generic;
using System.Text;
using Base.ToolPackage.Editor.Shared;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// One executable entry of the command palette. Built once per index pass and never changed
    /// afterwards, so scoring a keystroke never touches the editor or the asset database.
    /// </summary>

    // Load bearing and concrete on purpose. Built once per index pass and read on every keystroke, so
    // it is a row of data rather than a service. There is only ever one kind of command entry, and an
    // interface in front of it would add a call through a reference in the scoring hot path.
    internal sealed class CommandEntry
    {
        // Keywords are matched as a single string, and this is what stops a term from bridging two
        // of them. A term can never contain it, because the search box splits on spaces.
        private const char KeywordSeparator = '\n';

        private const char PathSeparator = '/';

        /// <summary>Stable id used to store tags and usage, independent of the current path.</summary>
        internal string Id { get; }

        /// <summary>Full menu path the entry is indexed by, root segment included.</summary>
        internal string Path { get; }

        /// <summary>Lowercase copy of <see cref="Path"/>, cached because every keystroke reads it.</summary>
        internal string LowerPath { get; }

        /// <summary>
        /// The entry's own search terms, lowercase, whitespace removed and separated by a
        /// character no term can contain. Empty when the entry brought none.
        /// </summary>
        /// <remarks>
        /// Deliberately kept out of <see cref="Path"/>: the matcher scores a subsequence, and over
        /// a long run of keywords almost any few letters appear in order somewhere, so every entry
        /// carrying them would match everything. They are matched as a plain substring instead.
        /// </remarks>
        internal string LowerKeywords { get; }

        /// <summary>Index of the first character of the last path segment.</summary>
        internal int LeafStart { get; }

        /// <summary>Type that declares the command, or null when it could not be resolved.</summary>
        internal Type Owner { get; }

        /// <summary>Short name of <see cref="Owner"/>, shown as the secondary label.</summary>
        internal string Detail { get; }

        /// <summary>What executing the entry does.</summary>
        internal ECommandKind Kind { get; }

        /// <summary>Where the declaring code lives.</summary>
        internal EAssetOrigin Origin { get; }

        private readonly Action _execute;

        /// <summary>Creates an entry.</summary>
        /// <param name="id">Stable id used for tags and usage.</param>
        /// <param name="path">Full menu path, root segment included.</param>
        /// <param name="owner">Type that declares the command.</param>
        /// <param name="kind">What executing the entry does.</param>
        /// <param name="origin">Where the declaring code lives.</param>
        /// <param name="execute">The action the palette runs.</param>
        /// <param name="keywords">Extra terms the entry can be found by, or null when it has none.</param>
        public CommandEntry(string id, string path, Type owner, ECommandKind kind, EAssetOrigin origin,
            Action execute, IEnumerable<string> keywords = null)
        {
            Id = id;
            Path = path;
            LowerPath = path.ToLowerInvariant();
            LowerKeywords = BuildKeywords(keywords);

            int separator = path.LastIndexOf(PathSeparator);
            LeafStart = separator >= 0
                ? separator + 1
                : 0;

            Owner = owner;
            Detail = owner != null
                ? owner.Name
                : string.Empty;

            Kind = kind;
            Origin = origin;
            _execute = execute;
        }

        /// <summary>Runs the command.</summary>
        internal void Execute() => _execute();

        // Whitespace is dropped because the search box concatenates its tokens without any, so a
        // typed "contact offset" arrives as one word and would never match "Contact Offset".
        private static string BuildKeywords(IEnumerable<string> keywords)
        {
            if (keywords == null)
                return string.Empty;

            StringBuilder builder = new();

            foreach (string keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    continue;

                if (builder.Length > 0)
                    builder.Append(KeywordSeparator);

                foreach (char character in keyword)
                {
                    if (!char.IsWhiteSpace(character))
                        builder.Append(char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }
    }
}