using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Walks cleaned C# text while respecting nesting.
    /// <para>
    /// A declaration is bounded by the first semicolon that is not inside a generic argument list, an
    /// array initializer or a lambda body, and the same holds for the comma between two declarators
    /// and for the equals sign of an initializer. Every method here answers one of those questions by
    /// counting depth rather than by matching a pattern, which is why none of them needs a parser.
    /// </para>
    /// </summary>
    internal static class SourceNavigator
    {
        /// <summary>Finds the character that closes the one at the given index, skipping nested pairs.</summary>
        /// <param name="text">Cleaned source to search.</param>
        /// <param name="openIndex">Index of the opening character.</param>
        /// <param name="open">The opening character.</param>
        /// <param name="close">The closing character.</param>
        /// <returns>The index of the match, or the length of the text when nothing closes it.</returns>
        internal static int MatchPair(string text, int openIndex, char open, char close)
        {
            int depth = 0;
            for (int index = openIndex; index < text.Length; index++)
            {
                if (text[index] == open)
                {
                    depth++;
                }
                else if (text[index] == close)
                {
                    depth--;
                    if (depth == 0)
                        return index + 1;
                }
            }

            return text.Length;
        }

        /// <summary>
        /// Finds the semicolon that ends a declaration. One inside a generic argument list, an
        /// initializer or a lambda body belongs to something else and is stepped over.
        /// </summary>
        /// <param name="text">Cleaned source to search.</param>
        /// <param name="start">Index to start searching from.</param>
        /// <returns>The index of the semicolon, or the last index when the text holds none.</returns>
        internal static int FindTopLevelSemicolon(string text, int start)
        {
            int parenthesis = 0, bracket = 0, brace = 0;
            for (int index = start; index < text.Length; index++)
            {
                char current = text[index];
                switch (current)
                {
                    case '(':
                    {
                        parenthesis++;
                        break;
                    }
                    case ')':
                    {
                        if (parenthesis > 0)
                            parenthesis--;

                        break;
                    }
                    case '[':
                    {
                        bracket++;
                        break;
                    }
                    case ']':
                    {
                        if (bracket > 0)
                            bracket--;

                        break;
                    }
                    case '{':
                    {
                        brace++;
                        break;
                    }
                    case '}':
                    {
                        if (brace > 0)
                            brace--;

                        break;
                    }
                    case ';':
                    {
                        if (parenthesis == 0
                            && bracket == 0
                            && brace == 0)
                            return index;

                        break;
                    }
                }
            }

            return text.Length - 1;
        }

        /// <summary>
        /// Splits on a separator that is not nested, which is how the declarators of
        /// <c>static int a, b = 2;</c> are told apart from the commas inside their initializers.
        /// </summary>
        /// <param name="text">Cleaned source to split.</param>
        /// <param name="separator">The character to split on.</param>
        /// <returns>The parts, with the separators removed.</returns>
        internal static List<string> SplitTopLevel(string text, char separator)
        {
            List<string> parts = new();
            int angle = 0, parenthesis = 0, bracket = 0, brace = 0, segmentStart = 0;
            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                switch (current)
                {
                    case '<':
                        angle++;
                        break;
                    case '>':
                        if (angle > 0)
                            angle--;

                        break;
                    case '(':
                        parenthesis++;
                        break;
                    case ')':
                        if (parenthesis > 0)
                            parenthesis--;

                        break;
                    case '[':
                        bracket++;
                        break;
                    case ']':
                        if (bracket > 0)
                            bracket--;

                        break;
                    case '{':
                        brace++;
                        break;
                    case '}':
                        if (brace > 0)
                            brace--;

                        break;
                }

                if (current != separator
                    || angle != 0
                    || parenthesis != 0
                    || bracket != 0
                    || brace != 0)
                    continue;

                parts.Add(text.Substring(segmentStart, index - segmentStart));
                segmentStart = index + 1;
            }

            parts.Add(text[segmentStart..]);
            return parts;
        }

        /// <summary>
        /// Finds the equals sign that starts an initializer, and not one belonging to a comparison, a
        /// lambda arrow, a compound assignment or anything nested inside the declaration.
        /// </summary>
        /// <param name="text">Cleaned declaration to search.</param>
        /// <returns>The index of the equals sign, or minus one when the declaration has none.</returns>
        internal static int IndexOfTopLevelAssign(string text)
        {
            int angle = 0, parenthesis = 0, bracket = 0, brace = 0;
            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                switch (current)
                {
                    case '<':
                    {
                        angle++;
                        break;
                    }
                    case '>':
                    {
                        if (angle > 0)
                            angle--;

                        break;
                    }
                    case '(':
                    {
                        parenthesis++;
                        break;
                    }
                    case ')':
                    {
                        if (parenthesis > 0)
                            parenthesis--;

                        break;
                    }
                    case '[':
                    {
                        bracket++;
                        break;
                    }
                    case ']':
                    {
                        if (bracket > 0)
                            bracket--;

                        break;
                    }
                    case '{':
                    {
                        brace++;
                        break;
                    }
                    case '}':
                    {
                        if (brace > 0)
                            brace--;

                        break;
                    }
                }

                if (current != '='
                    || angle != 0
                    || parenthesis != 0
                    || bracket != 0
                    || brace != 0)
                    continue;

                char next = index + 1 < text.Length
                    ? text[index + 1]
                    : '\0';

                char previous = index > 0
                    ? text[index - 1]
                    : '\0';

                if (next == '>'
                    || next == '='
                    || previous == '='
                    || previous == '!'
                    || previous == '<'
                    || previous == '>'
                    || previous == '+'
                    || previous == '-'
                    || previous == '*'
                    || previous == '/'
                    || previous == '%'
                    || previous == '&'
                    || previous == '|'
                    || previous == '^')
                    continue;

                return index;
            }

            return -1;
        }

        /// <summary>Reads the first character before the given index that is not whitespace.</summary>
        /// <param name="text">Cleaned source to read.</param>
        /// <param name="index">Index to read backwards from.</param>
        /// <returns>The character, or the null character when only whitespace precedes it.</returns>
        internal static char PrevNonSpace(string text, int index)
        {
            while (index >= 0 && char.IsWhiteSpace(text[index]))
                index--;

            return index >= 0
                ? text[index]
                : '\0';
        }
    }
}