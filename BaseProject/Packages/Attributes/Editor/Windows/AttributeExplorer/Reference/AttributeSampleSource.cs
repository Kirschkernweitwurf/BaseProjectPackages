using System;
using System.Collections.Generic;
using System.Text;

namespace Base.AttributePackage.Editor.Drawers.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// Lifts the body of a sample class out of its source file.
    /// </summary>
    /// <remarks>
    /// The whole body rather than one member. A sample demonstrates exactly one attribute, so everything
    /// in it is part of the answer: the bool a condition watches, the property a dropdown reads its
    /// options from, the fields an ordering attribute is reordered against. Cutting a single declaration
    /// out was what produced snippets that could not be pasted anywhere and compile.
    /// <para>
    /// The usings, the namespace and the class declaration are dropped, since none of them are what the
    /// reader came for. A sample whose attribute is written on the class itself is the exception: there
    /// the declaration is the answer, so the class attributes and the header come with it and the
    /// closing brace is put back. The marker this window reads is dropped either way, being plumbing
    /// rather than part of the sample.
    /// </para>
    /// </remarks>
    internal static class AttributeSampleSource
    {
        private const char AttributeEnd = ']';
        private const char AttributeStart = '[';
        private const char BlockEnd = '}';
        private const char BlockStart = '{';
        private const string ClassKeyword = "class ";
        private const string DocComment = "///";
        private const char LineFeed = '\n';
        private const string SampleMarker = "[AttributeSample(";
        private const char Space = ' ';
        private const string WindowsLineBreak = "\r\n";

        // Reused between draws so switching pages does not allocate a list per selection.
        private static readonly List<int> Emitted = new();

        /// <summary>Extracts the body of the named class, with its class attributes when it has any.</summary>
        /// <param name="source">The full source of the sample file.</param>
        /// <param name="typeName">The name of the sample class.</param>
        /// <returns>The snippet, or the whole source when the class cannot be located.</returns>
        internal static string Extract(string source, string typeName)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(typeName))
                return source ?? string.Empty;

            string[] lines = source.Replace(WindowsLineBreak, LineFeed.ToString()).Split(LineFeed);
            int declaration = FindDeclaration(lines, typeName);

            if (declaration < 0)
                return source;

            int first = FindBodyStart(lines, declaration);

            if (first < 0)
                return source;

            int last = FindBodyEnd(lines, first);

            if (last < first)
                return source;

            Emitted.Clear();

            CollectClassAttributes(lines, declaration);

            // Only a class carrying an attribute of its own needs the header. Without one the declaration
            // is scaffolding, and a reader pasting the body into their own type wants it gone.
            if (Emitted.Count > 0)
            {
                Emitted.Add(declaration);
                Add(first - 1, last + 1);
            }
            else
            {
                Add(first, last);
            }

            return Join(lines);
        }

        private static int FindDeclaration(string[] lines, string typeName)
        {
            string marker = ClassKeyword + typeName;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(marker, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        // The brace may sit on the declaration line or on the next one, so it is searched for rather than
        // assumed.
        private static int FindBodyStart(string[] lines, int declaration)
        {
            for (int i = declaration; i < lines.Length; i++)
            {
                if (lines[i].IndexOf(BlockStart) >= 0)
                    return i + 1;
            }

            return -1;
        }

        private static int FindBodyEnd(string[] lines, int first)
        {
            int depth = 1;

            for (int i = first; i < lines.Length; i++)
            {
                foreach (char character in lines[i])
                {
                    if (character == BlockStart)
                        depth++;
                    else if (character == BlockEnd)
                        depth--;

                    if (depth == 0)
                        return i - 1;
                }
            }

            return lines.Length - 1;
        }

        // Walked forwards from the top of the block, one attribute at a time, because an attribute
        // wrapped over several lines cannot be recognized from its last line: that line ends in a bracket
        // like every other one, so walking backwards mistakes the tail of the marker for an attribute of
        // its own.
        private static void CollectClassAttributes(string[] lines, int declaration)
        {
            int start = FirstAttributeLine(lines, declaration);
            int index = start;

            while (index < declaration)
            {
                string trimmed = lines[index].TrimStart();

                if (trimmed.Length == 0 || trimmed.StartsWith(DocComment, StringComparison.Ordinal))
                {
                    index++;
                    continue;
                }

                if (trimmed.Length == 0 || trimmed[0] != AttributeStart)
                {
                    index++;
                    continue;
                }

                int end = AttributeEndLine(lines, index, declaration);

                if (!trimmed.StartsWith(SampleMarker, StringComparison.Ordinal))
                    Add(index, end);

                index = end + 1;
            }
        }

        // The first line of the contiguous run of attributes and comments above the declaration.
        private static int FirstAttributeLine(string[] lines, int declaration)
        {
            int first = declaration;

            while (first > 0 && lines[first - 1].Trim().Length > 0)
                first--;

            return first;
        }

        // An attribute ends where its brackets balance again, which is what makes a wrapped one a single
        // unit rather than a run of lines that each look like something.
        private static int AttributeEndLine(string[] lines, int index, int declaration)
        {
            int depth = 0;

            for (int i = index; i < declaration; i++)
            {
                foreach (char character in lines[i])
                {
                    if (character == AttributeStart)
                        depth++;
                    else if (character == AttributeEnd)
                        depth--;
                }

                if (depth <= 0)
                    return i;
            }

            return declaration - 1;
        }

        private static void Add(int first, int last)
        {
            for (int i = first; i <= last; i++)
                Emitted.Add(i);
        }

        private static string Join(string[] lines)
        {
            int indent = CommonIndent(lines);

            StringBuilder builder = new();

            foreach (int index in Emitted)
            {
                if (index < 0 || index >= lines.Length)
                    continue;

                string line = Dedent(lines[index], indent);

                // A block that opens with a blank line reads as a gap the reader has to explain to
                // themselves, so it is trimmed to its first real line.
                if (builder.Length == 0 && line.Length == 0)
                    continue;

                if (builder.Length > 0)
                    builder.Append(LineFeed);

                builder.Append(line);
            }

            return builder.ToString().TrimEnd();
        }

        // The shallowest line sets the amount, so the relative indentation inside a method body survives
        // instead of every line being trimmed to nothing.
        private static int CommonIndent(string[] lines)
        {
            int indent = int.MaxValue;

            foreach (int index in Emitted)
            {
                if (index < 0 || index >= lines.Length || lines[index].Trim().Length == 0)
                    continue;

                indent = Math.Min(indent, lines[index].Length - lines[index].TrimStart().Length);
            }

            return indent == int.MaxValue
                ? 0
                : indent;
        }

        private static string Dedent(string line, int indent)
        {
            int removable = 0;

            while (removable < indent && removable < line.Length && line[removable] == Space)
                removable++;

            return line[removable..].TrimEnd();
        }
    }
}