using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Turns a character index in a file into a line number and the text of that line.
    /// <para>
    /// The scan works in indexes because that is what searching gives it, and a finding has to name a
    /// line because that is what a person opens. The offsets are built once per file and searched
    /// rather than counted, so this stays cheap on a file with thousands of lines.
    /// </para>
    /// </summary>
    internal static class SourceLines
    {
        // Longest declaration line kept for the report. A generated or minified line can run for
        // thousands of characters, which the list cannot show and nobody would read anyway.
        private const int MaxSnippetLength = 200;

        /// <summary>Records where every line of a file begins, once, so lookups can search rather than count.</summary>
        /// <param name="source">The file to index.</param>
        /// <returns>The index each line starts at, in order.</returns>
        internal static int[] BuildLineStarts(string source)
        {
            List<int> starts = new()
            {
                0
            };

            for (int index = 0; index < source.Length; index++)
            {
                if (source[index] == '\n')
                    starts.Add(index + 1);
            }

            return starts.ToArray();
        }

        /// <summary>Finds the line a character index falls on.</summary>
        /// <param name="lineStarts">The line offsets of the file.</param>
        /// <param name="index">The character index to locate.</param>
        /// <returns>The line number, counted from one.</returns>
        internal static int LineFromIndex(int[] lineStarts, int index)
        {
            int low = 0, high = lineStarts.Length - 1, found = 0;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (lineStarts[mid] <= index)
                {
                    found = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return found + 1;
        }

        /// <summary>
        /// Reads one line of a file for the report, trimmed and cut back to a length a list can show.
        /// </summary>
        /// <param name="source">The file to read.</param>
        /// <param name="lineStarts">The line offsets of the file.</param>
        /// <param name="lineNumber">The line to read, counted from one.</param>
        /// <returns>The line text, shortened when it runs long.</returns>
        internal static string GetLineText(string source, int[] lineStarts, int lineNumber)
        {
            int index = lineNumber - 1;
            if (index < 0 || index >= lineStarts.Length)
                return string.Empty;

            int start = lineStarts[index];
            int end = index + 1 < lineStarts.Length
                ? lineStarts[index + 1]
                : source.Length;

            string text = source.Substring(start, end - start).TrimEnd('\r', '\n');
            return text.Length > MaxSnippetLength
                ? text[..MaxSnippetLength]
                : text;
        }
    }
}