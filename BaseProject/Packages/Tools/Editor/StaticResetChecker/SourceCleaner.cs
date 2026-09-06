using System.Text;

namespace Base.ToolsPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Blanks out everything in a C# file that is not code.
    /// <para>
    /// Every pass over the source afterwards searches for braces, semicolons and keywords, and all
    /// three appear inside comments and string literals as well. Blanking them to spaces rather than
    /// removing them is what keeps every index in the cleaned text pointing at the same character in
    /// the file it came from.
    /// </para>
    /// </summary>
    internal static class SourceCleaner
    {
        /// <summary>
        /// Replaces every comment and every string or character literal with spaces of the same
        /// length. Positions and line numbers stay exactly where they were, so a brace or a
        /// semicolon found in the result points at the same place in the original file.
        /// </summary>
        /// <param name="source">The source to blank out.</param>
        /// <returns>The source with everything that is not code turned into spaces.</returns>
        internal static string Clean(string source)
        {
            StringBuilder builder = new(source.Length);
            int index = 0, length = source.Length;
            while (index < length)
            {
                char current = source[index];

                switch (current)
                {
                    case '/' when index + 1 < length && source[index + 1] == '/':
                    {
                        while (index < length && source[index] != '\n')
                        {
                            builder.Append(' ');
                            index++;
                        }

                        continue;
                    }
                    case '/' when index + 1 < length && source[index + 1] == '*':
                    {
                        builder.Append("  ");
                        index += 2;
                        while (index < length
                               && !(source[index] == '*' && index + 1 < length && source[index + 1] == '/'))
                        {
                            builder.Append(source[index] == '\n'
                                ? '\n'
                                : ' ');

                            index++;
                        }

                        if (index < length)
                        {
                            builder.Append("  ");
                            index += 2;
                        }

                        continue;
                    }
                    case '@':
                    case '$':
                    {
                        int prefixEnd = index;
                        bool verbatim = false;
                        while (prefixEnd < length && (source[prefixEnd] == '@' || source[prefixEnd] == '$'))
                        {
                            if (source[prefixEnd] == '@')
                                verbatim = true;

                            prefixEnd++;
                        }

                        if (prefixEnd < length && source[prefixEnd] == '"')
                        {
                            for (int prefixIndex = index; prefixIndex < prefixEnd; prefixIndex++)
                                builder.Append(' ');

                            index = BlankString(source, prefixEnd, verbatim, builder);
                            continue;
                        }

                        builder.Append(current);
                        index++;
                        continue;
                    }
                    case '"':
                        index = BlankString(source, index, false, builder);
                        continue;
                    case '\'':
                        index = BlankChar(source, index, builder);
                        continue;
                    default:
                        builder.Append(current);
                        index++;
                        break;
                }
            }

            return builder.ToString();
        }

        private static int BlankString(string source, int index, bool verbatim, StringBuilder builder)
        {
            int length = source.Length;
            builder.Append(' ');
            index++;
            while (index < length)
            {
                char current = source[index];
                if (verbatim)
                {
                    if (current == '"')
                    {
                        if (index + 1 < length && source[index + 1] == '"')
                        {
                            builder.Append("  ");
                            index += 2;
                            continue;
                        }

                        builder.Append(' ');
                        index++;
                        return index;
                    }

                    builder.Append(current == '\n'
                        ? '\n'
                        : ' ');

                    index++;
                }
                else
                {
                    switch (current)
                    {
                        case '\\' when index + 1 < length:
                        {
                            builder.Append("  ");
                            index += 2;
                            continue;
                        }
                        case '"':
                        {
                            builder.Append(' ');
                            index++;
                            return index;
                        }
                        case '\n':
                        {
                            builder.Append('\n');
                            index++;
                            return index;
                        }
                        default:
                        {
                            builder.Append(' ');
                            index++;
                            break;
                        }
                    }
                }
            }

            return index;
        }

        private static int BlankChar(string source, int index, StringBuilder builder)
        {
            int length = source.Length;
            builder.Append(' ');
            index++;
            while (index < length)
            {
                char current = source[index];
                switch (current)
                {
                    case '\\' when index + 1 < length:
                    {
                        builder.Append("  ");
                        index += 2;
                        continue;
                    }
                    case '\'':
                    {
                        builder.Append(' ');
                        index++;
                        return index;
                    }
                    case '\n':
                    {
                        builder.Append('\n');
                        index++;
                        return index;
                    }
                    default:
                    {
                        builder.Append(' ');
                        index++;
                        break;
                    }
                }
            }

            return index;
        }
    }
}