using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Base.ToolsPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Reads the vocabulary of a C# declaration: its modifiers, the identifiers in it, and whether a
    /// word is a keyword rather than a name.
    /// <para>
    /// The scanner works on text, so it has to decide what a word is before it can decide what a
    /// declaration means. A field named <c>value</c> and the contextual keyword <c>value</c> look
    /// identical until something asks, and that is what the keyword set is for.
    /// </para>
    /// </summary>
    internal static class DeclarationReader
    {
        private static readonly HashSet<string> Modifiers = new()
        {
            "readonly",
            "volatile",
            "unsafe",
            "extern",
            "event"
        };

        private static readonly HashSet<string> Keywords = new()
        {
            "static",
            "public",
            "private",
            "protected",
            "internal",
            "readonly",
            "volatile",
            "unsafe",
            "extern",
            "event",
            "new",
            "abstract",
            "virtual",
            "override",
            "sealed",
            "async",
            "partial",
            "const",
            "ref",
            "out",
            "in",
            "params",
            "this",
            "base",
            "return",
            "void",
            "var",
            "dynamic",
            "int",
            "uint",
            "long",
            "ulong",
            "short",
            "ushort",
            "byte",
            "sbyte",
            "float",
            "double",
            "decimal",
            "bool",
            "char",
            "string",
            "object",
            "nint",
            "nuint",
            "delegate",
            "enum",
            "struct",
            "class",
            "interface",
            "record",
            "namespace",
            "using",
            "if",
            "else",
            "for",
            "foreach",
            "while",
            "do",
            "switch",
            "case",
            "default",
            "break",
            "continue",
            "throw",
            "try",
            "catch",
            "finally",
            "lock",
            "fixed",
            "checked",
            "unchecked",
            "typeof",
            "sizeof",
            "nameof",
            "true",
            "false",
            "null",
            "operator",
            "implicit",
            "explicit",
            "where",
            "get",
            "set",
            "init",
            "value",
            "yield",
            "await",
            "global",
            "is",
            "as",
            "when",
            "stackalloc",
            "goto",
            "add",
            "remove"
        };

        /// <summary>
        /// Removes the modifiers in front of a declaration and reports the two that decide whether the
        /// static it declares can hold state that outlives play mode.
        /// </summary>
        /// <param name="text">Cleaned declaration, starting at its modifiers.</param>
        /// <param name="isEvent">True when the declaration is an event.</param>
        /// <param name="isReadonly">True when the declaration is readonly.</param>
        /// <returns>The declaration with its leading modifiers removed.</returns>
        internal static string StripLeadingModifiers(string text, out bool isEvent, out bool isReadonly)
        {
            isEvent = false;
            isReadonly = false;
            while (true)
            {
                int index = 0;
                while (index < text.Length && char.IsWhiteSpace(text[index]))
                    index++;

                int start = index;
                while (index < text.Length && (char.IsLetterOrDigit(text[index]) || text[index] == '_'))
                    index++;

                if (index == start)
                    break;

                string token = text.Substring(start, index - start);
                if (!Modifiers.Contains(token))
                    break;

                if (token == "event")
                    isEvent = true;

                if (token == "readonly")
                    isReadonly = true;

                text = text[index..];
            }

            return text;
        }

        /// <summary>Reads the last identifier in a piece of text, which is the name in a declaration.</summary>
        /// <param name="text">Cleaned text to read.</param>
        /// <returns>The identifier, or an empty string when the text ends in none.</returns>
        internal static string LastIdentifier(string text)
        {
            text = text.TrimEnd();
            int end = text.Length - 1;
            if (end < 0 || !(char.IsLetterOrDigit(text[end]) || text[end] == '_'))
                return null;

            int start = end;
            while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_'))
                start--;

            return text.Substring(start + 1, end - start);
        }

        /// <summary>Reads the first identifier in a piece of text.</summary>
        /// <param name="text">Cleaned text to read.</param>
        /// <returns>The identifier, or null when the text starts with none.</returns>
        internal static string FirstIdentifier(string text)
        {
            int index = 0;
            while (index < text.Length && !(char.IsLetterOrDigit(text[index]) || text[index] == '_'))
                index++;

            int start = index;
            while (index < text.Length && (char.IsLetterOrDigit(text[index]) || text[index] == '_'))
                index++;

            return index > start
                ? text.Substring(start, index - start)
                : null;
        }

        /// <summary>
        /// Reads the identifier that ends just before the given index, stepping over the whitespace and
        /// the generic argument list that can sit between a method name and its parenthesis.
        /// </summary>
        /// <param name="text">Cleaned source to read.</param>
        /// <param name="index">Index to read backwards from.</param>
        /// <returns>The identifier, or null when none ends there.</returns>
        internal static string ReadIdentifierBefore(string text, int index)
        {
            int cursor = index - 1;
            while (cursor >= 0 && char.IsWhiteSpace(text[cursor]))
                cursor--;

            if (cursor >= 0 && text[cursor] == '>')
            {
                int depth = 0;
                while (cursor >= 0)
                {
                    if (text[cursor] == '>')
                    {
                        depth++;
                    }
                    else if (text[cursor] == '<')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            cursor--;
                            break;
                        }
                    }

                    cursor--;
                }

                while (cursor >= 0 && char.IsWhiteSpace(text[cursor]))
                    cursor--;
            }

            int end = cursor + 1, start = cursor;
            while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_'))
                start--;

            return end - (start + 1) > 0
                ? text.Substring(start + 1, end - (start + 1))
                : null;
        }

        /// <summary>Whether the word immediately before the given position is the given one.</summary>
        /// <param name="text">Cleaned source to read.</param>
        /// <param name="position">Index to read backwards from.</param>
        /// <param name="word">The word to look for.</param>
        /// <returns>True when that word sits directly before the position.</returns>
        internal static bool PrecededByWord(string text, int position, string word)
        {
            int cursor = position - 1;
            while (cursor >= 0 && char.IsWhiteSpace(text[cursor]))
                cursor--;

            int end = cursor + 1, start = cursor;
            while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_'))
                start--;

            return end - (start + 1) > 0 && text.Substring(start + 1, end - (start + 1)) == word;
        }

        /// <summary>
        /// Maps a name found inside a declaration back to its index in the file, so a finding points at
        /// the name rather than at the start of the line it sits on.
        /// </summary>
        /// <param name="position">Index the declaration starts at in the file.</param>
        /// <param name="text">The declaration the name was read from.</param>
        /// <param name="name">The name to locate.</param>
        /// <returns>The index of the name, or the declaration's own index when it appears more than once.</returns>
        internal static int AbsoluteNameIndex(int position, string text, string name)
        {
            MatchCollection matches = Regex.Matches(text, $@"\b{Regex.Escape(name)}\b");
            return matches.Count > 0
                ? position + matches[^1].Index
                : position;
        }

        /// <summary>Whether a word is a C# keyword rather than something a declaration named.</summary>
        /// <param name="word">The word to test.</param>
        /// <returns>True when the word is a keyword.</returns>
        internal static bool IsKeyword(string word) => Keywords.Contains(word);
    }
}