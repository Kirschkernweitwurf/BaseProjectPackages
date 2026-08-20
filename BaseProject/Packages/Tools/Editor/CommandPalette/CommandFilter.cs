using System;
using System.Collections.Generic;
using System.Text;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// The parsed content of the search box. Plain words become the fuzzy term, "#tag" narrows to
    /// a tag, "&gt;" narrows to menu items and "+" to asset creation.
    /// </summary>
    internal readonly struct CommandFilter
    {
        /// <summary>Marks a token that narrows the result to asset creation entries.</summary>
        public const char CreateAssetMarker = '+';

        /// <summary>Marks a token that narrows the result to menu items.</summary>
        public const char MenuItemMarker = '>';

        /// <summary>Marks a token that narrows the result to a tag.</summary>
        public const char TagMarker = '#';

        private const char TokenSeparator = ' ';

        private static readonly string[] NoTags = Array.Empty<string>();

        private static readonly char[] Separators =
        {
            TokenSeparator
        };

        /// <summary>Lowercase search term with every space removed.</summary>
        public string Term { get; }

        /// <summary>Lowercase tags every result has to carry.</summary>
        public IReadOnlyList<string> Tags { get; }

        /// <summary>Kind every result has to be, or null when both are allowed.</summary>
        public ECommandKind? Kind { get; }

        private CommandFilter(string term, IReadOnlyList<string> tags, ECommandKind? kind)
        {
            Term = term;
            Tags = tags;
            Kind = kind;
        }

        /// <summary>Parses the raw search box content.</summary>
        /// <param name="raw">The text the user typed.</param>
        /// <returns>The filter the query layer works with.</returns>
        public static CommandFilter Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new CommandFilter(string.Empty, NoTags, null);

            StringBuilder term = new();
            List<string> tags = null;
            ECommandKind? kind = null;

            foreach (string token in raw.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
            {
                string rest = token[1..];

                switch (token[0])
                {
                    case MenuItemMarker:
                        kind = ECommandKind.MenuItem;
                        term.Append(rest.ToLowerInvariant());
                        break;

                    case CreateAssetMarker:
                        kind = ECommandKind.CreateAsset;
                        term.Append(rest.ToLowerInvariant());
                        break;

                    case TagMarker:
                        if (rest.Length > 0)
                            (tags ??= new List<string>()).Add(rest.ToLowerInvariant());

                        break;

                    default:
                        term.Append(token.ToLowerInvariant());
                        break;
                }
            }

            IReadOnlyList<string> resolved = tags;

            return new CommandFilter(term.ToString(), resolved ?? NoTags, kind);
        }
    }
}