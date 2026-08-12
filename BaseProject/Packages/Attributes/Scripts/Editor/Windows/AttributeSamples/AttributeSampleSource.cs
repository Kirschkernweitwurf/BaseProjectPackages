using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples
{
    /// <summary>
    /// Pulls the few lines that declare one member out of a sample's source file.
    /// </summary>
    /// <remarks>
    /// Showing the whole file would bury the four lines worth reading. The block is found by locating
    /// the declaration and walking back over the attributes and comments stacked above it, which is the
    /// unit a reader would have copied by hand anyway.
    /// <para>
    /// A plain text scan rather than a parser. The samples are written by hand in one house style, and a
    /// parser for the general case would be far more machinery than the job needs.
    /// </para>
    /// </remarks>
    internal static class AttributeSampleSource
    {
        private const char BlockEnd = '}';
        private const char BlockStart = '{';
        private const char StatementEnd = ';';

        /// <summary>Extracts the declaration of one member, with its attributes above it.</summary>
        /// <param name="source">The full source of the sample file.</param>
        /// <param name="memberName">The field or method to find.</param>
        /// <returns>The block of lines, or the whole source when the member cannot be located.</returns>
        public static string Extract(string source, string memberName)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(memberName))
                return source;

            string[] lines = source.Replace("\r\n", "\n").Split('\n');
            int declaration = FindDeclaration(lines, memberName);

            if (declaration < 0)
                return source;

            int first = FindFirstAttributeLine(lines, declaration);
            int last = FindLastLine(lines, declaration);

            StringBuilder builder = new();

            for (int i = first; i <= last && i < lines.Length; i++)
            {
                if (builder.Length > 0)
                    builder.Append('\n');

                builder.Append(Dedent(lines[i]));
            }

            return builder.ToString();
        }

        // The declaration is the first line naming the member outside an attribute argument, so a field
        // referenced by a nameof above it is not mistaken for the declaration itself.
        private static int FindDeclaration(string[] lines, string memberName)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].TrimStart();

                if (trimmed.StartsWith("[") || trimmed.StartsWith("//"))
                    continue;

                if (Names(trimmed, memberName))
                    return i;
            }

            return -1;
        }

        private static bool Names(string line, string memberName)
        {
            int index = line.IndexOf(memberName, StringComparison.Ordinal);
            if (index < 0)
                return false;

            // A whole-word match, so "count" does not match "counted".
            bool startsClean = index == 0 || !char.IsLetterOrDigit(line[index - 1]) && line[index - 1] != '_';
            int after = index + memberName.Length;
            bool endsClean = after >= line.Length
                || !char.IsLetterOrDigit(line[after]) && line[after] != '_';

            return startsClean && endsClean;
        }

        private static int FindFirstAttributeLine(string[] lines, int declaration)
        {
            int first = declaration;

            while (first > 0)
            {
                string previous = lines[first - 1].TrimStart();

                if (!previous.StartsWith("[") && !previous.StartsWith("///") && !previous.StartsWith("//"))
                    break;

                first--;
            }

            return first;
        }

        // A field ends at its semicolon and a method at the brace that closes it, so both are followed
        // to their end rather than assumed to be one line.
        private static int FindLastLine(string[] lines, int declaration)
        {
            int depth = 0;
            bool opened = false;

            for (int i = declaration; i < lines.Length; i++)
            {
                foreach (char character in lines[i])
                {
                    if (character == BlockStart)
                    {
                        depth++;
                        opened = true;
                    }
                    else if (character == BlockEnd)
                    {
                        depth--;
                    }
                    else if (character == StatementEnd && depth == 0)
                    {
                        return i;
                    }
                }

                if (opened && depth == 0)
                    return i;
            }

            return declaration;
        }

        // The block sits two levels deep in the file and reads better flush against the left edge.
        private static string Dedent(string line)
        {
            const int classIndent = 8;

            return line.Length > classIndent && line[..classIndent].Trim().Length == 0
                ? line[classIndent..]
                : line.TrimStart();
        }
    }
}