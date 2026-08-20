using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.NamingConventions.Data;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Detects the casing of a name and rewrites it into another casing.
    /// <see cref="ENamingStyle.PascalSnakeCase"/> keeps the underscores between words, so
    /// "Kitchen_Lamp" stays a category plus an asset instead of collapsing into one word.
    /// </summary>
    public static class NameStyleUtility
    {
        private const char Underscore = '_';

        private static readonly Regex CamelPattern = new("^[a-z][A-Za-z0-9]*$", RegexOptions.Compiled);
        private static readonly Regex LowerSnakePattern = new("^[a-z0-9]+(_[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Regex PascalPattern = new("^[A-Z][A-Za-z0-9]*$", RegexOptions.Compiled);

        // A segment is a pascal case word or a plain number, so a variant like "Counter_01_MS"
        // stays valid instead of being reported with a fix that changes nothing.
        private static readonly Regex PascalSnakePattern =
            new("^([A-Z][A-Za-z0-9]*|[0-9]+)(_([A-Z][A-Za-z0-9]*|[0-9]+))*$", RegexOptions.Compiled);

        private static readonly Regex UpperSnakePattern = new("^[A-Z0-9]+(_[A-Z0-9]+)*$", RegexOptions.Compiled);

        /// <summary>True when the name follows the style. <see cref="ENamingStyle.Any"/> always passes.</summary>
        public static bool Matches(string name, ENamingStyle style)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return style switch
            {
                ENamingStyle.PascalCase => PascalPattern.IsMatch(name),
                ENamingStyle.CamelCase => CamelPattern.IsMatch(name),
                ENamingStyle.UpperSnakeCase => UpperSnakePattern.IsMatch(name),
                ENamingStyle.LowerSnakeCase => LowerSnakePattern.IsMatch(name),
                ENamingStyle.PascalSnakeCase => PascalSnakePattern.IsMatch(name),
                _ => true
            };
        }

        /// <summary>
        /// Best matching style of a name, or <see cref="ENamingStyle.Any"/> when nothing fits. The
        /// all upper and all lower styles win over the mixed one, so "MY_NAME" stays upper snake.
        /// </summary>
        public static ENamingStyle Detect(string name)
        {
            if (string.IsNullOrEmpty(name))
                return ENamingStyle.Any;

            if (name.IndexOf(Underscore) >= 0)
                return DetectSnake(name);

            if (PascalPattern.IsMatch(name))
                return ENamingStyle.PascalCase;

            if (CamelPattern.IsMatch(name))
                return ENamingStyle.CamelCase;

            return ENamingStyle.Any;
        }

        /// <summary>Rewrites a name into the given style. Returns the input when nothing can be split.</summary>
        public static string Convert(string name, ENamingStyle style)
        {
            if (style == ENamingStyle.PascalSnakeCase)
                return ConvertSegments(name);

            List<string> words = SplitWords(name);

            if (words.Count == 0)
                return name;

            return style switch
            {
                ENamingStyle.PascalCase => JoinCased(words, false),
                ENamingStyle.CamelCase => JoinCased(words, true),
                ENamingStyle.UpperSnakeCase => JoinSnake(words, true),
                ENamingStyle.LowerSnakeCase => JoinSnake(words, false),
                _ => name
            };
        }

        /// <summary>Rewrites every underscore segment on its own, so the segments survive the fix.</summary>
        private static string ConvertSegments(string name)
        {
            StringBuilder builder = new();

            foreach (string segment in name.Split(Underscore))
            {
                if (segment.Length == 0)
                    continue;

                List<string> words = SplitWords(segment);

                if (words.Count == 0)
                    continue;

                if (builder.Length > 0)
                    builder.Append(Underscore);

                builder.Append(JoinCased(words, false));
            }

            return builder.Length == 0
                ? name
                : builder.ToString();
        }

        private static ENamingStyle DetectSnake(string name)
        {
            if (UpperSnakePattern.IsMatch(name))
                return ENamingStyle.UpperSnakeCase;

            if (LowerSnakePattern.IsMatch(name))
                return ENamingStyle.LowerSnakeCase;

            if (PascalSnakePattern.IsMatch(name))
                return ENamingStyle.PascalSnakeCase;

            return ENamingStyle.Any;
        }

        private static List<string> SplitWords(string name)
        {
            List<string> words = new();
            StringBuilder current = new();

            for (int index = 0; index < name.Length; index++)
            {
                char symbol = name[index];

                if (symbol == Underscore
                    || symbol == ' '
                    || symbol == '-')
                {
                    Flush(words, current);
                    continue;
                }

                if (char.IsUpper(symbol)
                    && current.Length > 0
                    && IsWordStart(name, index))
                    Flush(words, current);

                current.Append(char.ToLowerInvariant(symbol));
            }

            Flush(words, current);

            return words;
        }

        private static bool IsWordStart(string name, int index)
        {
            char previous = name[index - 1];

            if (char.IsLower(previous)
                || char.IsDigit(previous))
                return true;

            // An upper case run only ends when the next character is lower case, so "HTTPServer"
            // splits into "http" and "server" instead of one word per letter.
            return index + 1 < name.Length
                && char.IsLower(name[index + 1]);
        }

        private static void Flush(List<string> words, StringBuilder current)
        {
            if (current.Length == 0)
                return;

            words.Add(current.ToString());
            current.Clear();
        }

        private static string JoinCased(List<string> words, bool lowerFirst)
        {
            StringBuilder builder = new();

            for (int index = 0; index < words.Count; index++)
            {
                string word = words[index];
                bool keepLower = lowerFirst && index == 0;

                builder.Append(keepLower
                    ? word[0]
                    : char.ToUpperInvariant(word[0]));

                if (word.Length > 1)
                    builder.Append(word[1..]);
            }

            return builder.ToString();
        }

        private static string JoinSnake(List<string> words, bool upper)
        {
            StringBuilder builder = new();

            for (int index = 0; index < words.Count; index++)
            {
                if (index > 0)
                    builder.Append(Underscore);

                builder.Append(upper
                    ? words[index].ToUpperInvariant()
                    : words[index]);
            }

            return builder.ToString();
        }
    }
}