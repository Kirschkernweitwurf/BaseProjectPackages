using System.Collections.Generic;
using System.Text;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Turns a path into the rich text the result rows draw: the parent segments are dimmed so the
    /// eye lands on the entry name, and every character the search term matched is picked out.
    /// Equal neighbours share one tag pair, so a row stays a handful of tags instead of one per
    /// character.
    /// </summary>
    internal static class CommandHighlighter
    {
        private const int DimStyle = 1;
        private const int MatchStyle = 2;
        private const int NoStyle = -1;
        private const int PlainStyle = 0;

        private static readonly List<int> Matches = new();
        private static readonly StringBuilder Builder = new();

        private static bool[] _flags = new bool[0];

        /// <summary>Builds the rich text label of an entry.</summary>
        /// <param name="entry">The entry to render.</param>
        /// <param name="term">Lowercase search term. An empty term only dims the parent.</param>
        /// <returns>The rich text to draw with a rich text label style.</returns>
        public static string Build(CommandEntry entry, string term)
        {
            string path = entry.Path;

            PrepareFlags(entry, term);
            Builder.Clear();

            int current = NoStyle;

            for (int i = 0; i <= path.Length; i++)
            {
                int next = i < path.Length
                    ? StyleAt(i, entry.LeafStart)
                    : NoStyle;

                if (next != current)
                {
                    AppendClose(current);
                    AppendOpen(next);

                    current = next;
                }

                if (i < path.Length)
                    Builder.Append(path[i]);
            }

            return Builder.ToString();
        }

        private static void AppendClose(int style)
        {
            if (style == MatchStyle)
                Builder.Append(CommandPaletteStyles.MatchClose);
            else if (style == DimStyle)
                Builder.Append(CommandPaletteStyles.DimClose);
        }

        private static void AppendOpen(int style)
        {
            if (style == MatchStyle)
                Builder.Append(CommandPaletteStyles.MatchOpen);
            else if (style == DimStyle)
                Builder.Append(CommandPaletteStyles.DimOpen);
        }

        private static void PrepareFlags(CommandEntry entry, string term)
        {
            if (_flags.Length < entry.Path.Length)
                _flags = new bool[entry.Path.Length];

            for (int i = 0; i < entry.Path.Length; i++)
                _flags[i] = false;

            Matches.Clear();

            if (term.Length == 0)
                return;

            CommandMatcher.TryMatch(entry, term, Matches, out int _);

            foreach (int index in Matches)
                _flags[index] = true;
        }

        private static int StyleAt(int index, int leafStart)
        {
            if (_flags[index])
                return MatchStyle;

            return index < leafStart
                ? DimStyle
                : PlainStyle;
        }
    }
}