using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Base.TextPackage
{
    /// <summary>
    /// Marks every long word of a text with soft hyphens right before TextMeshPro reads it, in the current language, so
    /// the text the rest of the game sets and reads stays untouched. Languages without patterns, like Japanese and
    /// Chinese, break anywhere on their own and pass through as they are. Styled keywords break too, since a long term
    /// kept whole leaves big gaps in a narrow text. An icon right after a word stays glued to its end.
    /// </summary>
    public sealed class TextHyphenator : ITextPreprocessor
    {
        private const string ExceptionsFormat = "Hyphenation/hyph-{0}.hyp";

        private const string PatternsFormat = "Hyphenation/hyph-{0}.pat";
        private const char TagEnd = '>';
        private const char TagStart = '<';

        // The hyph-utf8 package names some patterns by spelling, not by the locale code the game uses.
        private static readonly Dictionary<string, string> PatternNames = new()
        {
            { "en", "en-us" },
            { "de", "de-1996" },
            { "es", "es" },
            { "fr", "fr" },
            { "pt-BR", "pt" },
            { "ru", "ru" }
        };

        private static readonly Dictionary<string, LiangHyphenator> Loaded = new();

        /// <summary>The one preprocessor every hyphenated text shares. It holds no state of its own.</summary>
        public static TextHyphenator Shared { get; } = new();

        /// <inheritdoc/>
        public string PreprocessText(string text)
        {
            Locale locale = TextLocale.Current;

            if (string.IsNullOrEmpty(text)
                || locale == null
                || !PatternNames.TryGetValue(locale.Identifier.Code, out string name))
                return text;

            return Hyphenate(text, GetHyphenator(name));
        }

        /// <summary>
        /// Returns <paramref name="text"/> with soft hyphens in its long words. Rich text tags stay as they are.
        /// </summary>
        /// <param name="text">The text, with or without rich text tags.</param>
        /// <param name="hyphenator">The patterns of the text's language.</param>
        public static string Hyphenate(string text, LiangHyphenator hyphenator)
        {
            StringBuilder builder = new(text.Length * 2);
            int i = 0;

            while (i < text.Length)
            {
                bool isWord = char.IsLetter(text[i]);
                int end = isWord
                    ? FindWordEnd(text, i)
                    : FindTagEnd(text, i);

                string part = text.Substring(i, end - i);
                builder.Append(isWord
                    ? hyphenator.Hyphenate(part)
                    : part);
                i = end;
            }

            return builder.ToString();
        }

        // Statics survive between plays with domain reload off, so a new play reads the lists again.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Loaded.Clear();

        private static LiangHyphenator GetHyphenator(string name)
        {
            if (Loaded.TryGetValue(name, out LiangHyphenator hyphenator))
                return hyphenator;

            hyphenator = new LiangHyphenator(ReadList(PatternsFormat, name), ReadList(ExceptionsFormat, name));
            Loaded[name] = hyphenator;
            return hyphenator;
        }

        // Not every language has exceptions, so a missing list is normal.
        private static string ReadList(string format, string name)
        {
            TextAsset list = Resources.Load<TextAsset>(string.Format(format, name));

            return list == null
                ? string.Empty
                : list.text;
        }

        private static int FindWordEnd(string text, int start)
        {
            int end = start;

            while (end < text.Length
                && char.IsLetter(text[end]))
                end++;

            return end;
        }

        // A rich text tag is not a word or <sprite name="emotional"> would break its name. A lone < is just a
        // character.
        private static int FindTagEnd(string text, int start)
        {
            if (text[start] != TagStart)
                return start + 1;

            int close = text.IndexOf(TagEnd, start);

            return close < 0
                ? start + 1
                : close + 1;
        }
    }
}