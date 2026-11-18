using System;
using System.Collections.Generic;
using System.Text;

namespace Base.TextPackage
{
    /// <summary>
    /// Finds where a word may break, with the Knuth-Liang patterns TeX uses and marks those spots with soft hyphens.
    /// Text only shows a soft hyphen when a line really ends there, so marking a word costs nothing on screen. Pure
    /// C#, so the rules can be tested without a scene or a font.
    /// </summary>
    public sealed class LiangHyphenator
    {
        private const char Boundary = '.';
        private const char CommentStart = '%';
        private const char ExceptionHyphen = '-';
        private const int MinAfterBreak = 3;
        private const int MinBeforeBreak = 3;
        private const int MinWordLength = 7;
        private const char SoftHyphen = '\u00AD';

        private static readonly char[] LineEnds = { '\r', '\n' };

        private readonly Dictionary<string, string> _exceptions = new();
        private readonly Dictionary<string, byte[]> _patterns = new();
        private readonly int _longestPattern;

        /// <summary>Reads the pattern and exception lists, in the plain text format of the hyph-utf8 package.</summary>
        /// <param name="patterns">One pattern per line, like <c>.ab3ol</c>.</param>
        /// <param name="exceptions">Words whose breaks are spelled out, like <c>as-so-ciate</c>. Can be empty.</param>
        public LiangHyphenator(string patterns, string exceptions)
        {
            foreach (string line in ReadLines(patterns))
                _longestPattern = Math.Max(_longestPattern, AddPattern(line));

            foreach (string line in ReadLines(exceptions))
                AddException(line);
        }

        /// <summary>
        /// Returns <paramref name="word"/> with a soft hyphen at every spot it may break. Short words and the first and
        /// last letters stay whole, since "a-" or "-ly" at a line end reads worse than a gap.
        /// </summary>
        /// <param name="word">Letters only, in any case.</param>
        public string Hyphenate(string word)
        {
            if (word.Length < MinWordLength)
                return word;

            string lower = word.ToLowerInvariant();

            // A letter whose lower case has another length has no matching position in the original word.
            if (lower.Length != word.Length)
                return word;

            bool[] breaks = _exceptions.TryGetValue(lower, out string spelled)
                ? FindSpelledBreaks(spelled, word.Length)
                : FindPatternBreaks(lower);

            return InsertHyphens(word, breaks);
        }

        // A list can start with a comment, like the notice the license of the Russian lists asks for.
        private static IEnumerable<string> ReadLines(string list)
        {
            foreach (string line in list.Split(LineEnds, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = line.Trim();

                if (trimmed.Length > 0
                    && trimmed[0] != CommentStart)
                    yield return trimmed;
            }
        }

        private static bool[] FindSpelledBreaks(string spelled, int length)
        {
            bool[] breaks = new bool[length];
            int letter = 0;

            foreach (char character in spelled)
            {
                if (character == ExceptionHyphen)
                    breaks[letter] = true;
                else
                    letter++;
            }

            return breaks;
        }

        private static string InsertHyphens(string word, bool[] breaks)
        {
            StringBuilder builder = new(word.Length * 2);

            for (int i = 0; i < word.Length; i++)
            {
                if (breaks[i]
                    && i >= MinBeforeBreak
                    && word.Length - i >= MinAfterBreak)
                    builder.Append(SoftHyphen);

                builder.Append(word[i]);
            }

            return builder.ToString();
        }

        private int AddPattern(string line)
        {
            StringBuilder letters = new(line.Length);
            List<byte> values = new(line.Length + 1) { 0 };

            foreach (char character in line)
            {
                if (char.IsDigit(character))
                {
                    values[values.Count - 1] = (byte)(character - '0');
                    continue;
                }

                letters.Append(character);
                values.Add(0);
            }

            _patterns[letters.ToString()] = values.ToArray();
            return letters.Length;
        }

        private void AddException(string line) =>
            _exceptions[line.Replace(ExceptionHyphen.ToString(), string.Empty)] = line;

        // Patterns raise the values between letters, the highest wins: odd allows a break, even does not.
        private bool[] FindPatternBreaks(string lower)
        {
            string marked = Boundary + lower + Boundary;
            byte[] levels = new byte[marked.Length + 1];

            for (int start = 0; start < marked.Length; start++)
            {
                int longest = Math.Min(_longestPattern, marked.Length - start);

                for (int length = 1; length <= longest; length++)
                    RaiseLevels(marked.Substring(start, length), start, levels);
            }

            bool[] breaks = new bool[lower.Length];

            // Level i sits in front of marked[i] and marked has the boundary as its first letter.
            for (int i = 0; i < lower.Length; i++)
                breaks[i] = levels[i + 1] % 2 == 1;

            return breaks;
        }

        private void RaiseLevels(string part, int start, byte[] levels)
        {
            if (!_patterns.TryGetValue(part, out byte[] values))
                return;

            for (int i = 0; i < values.Length; i++)
                levels[start + i] = Math.Max(levels[start + i], values[i]);
        }
    }
}