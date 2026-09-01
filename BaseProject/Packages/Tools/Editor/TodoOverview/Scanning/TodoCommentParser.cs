using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.Shared;
using Base.ToolPackage.Editor.TodoOverview.Model;

namespace Base.ToolPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// Turns the comment lines of one file into items.
    /// <para>
    /// The interesting part is how far an item reaches. A keyword marks the start, but the text can
    /// carry on over the following comment lines, and the rule for that differs per IDE. JetBrains
    /// continues an item while the following lines are indented deeper than the keyword, which is what
    /// <see cref="ETodoContinuation.Indented"/> reproduces, including inside block comments where the
    /// leading star is decoration rather than indentation. A line that carries a keyword of its own
    /// always starts a new item instead of continuing the previous one.
    /// </para>
    /// </summary>
    internal static class TodoCommentParser
    {
        private const char Decoration = '*';
        private const char LineBreak = '\n';
        private const char Slash = '/';

        private static readonly char[] Separators =
        {
            ':',
            '-',
            '>',
            ' ',
            '\t'
        };

        /// <summary>Finds every item in one file.</summary>
        /// <param name="assetPath">Project relative path of the file.</param>
        /// <param name="source">The full text of the file.</param>
        /// <param name="patterns">The compiled patterns of this scan.</param>
        /// <param name="results">The list the found items are added to.</param>
        internal static void Parse(string assetPath, string source, TodoPatterns patterns,
            List<TodoEntry> results)
        {
            List<CommentLine> comments = CommentReader.Read(source);
            string fileName = Path.GetFileName(assetPath);
            EAssetOrigin origin = AssetOriginResolver.Classify(assetPath);

            for (int i = 0; i < comments.Count; i++)
            {
                CommentLine comment = comments[i];
                Match match = patterns.Keywords.Match(comment.Text);

                if (!match.Success)
                    continue;

                int keywordColumn = comment.TextColumn + match.Index;
                string head = comment.Text[(match.Index + match.Length)..].Trim(Separators);

                int continued = CollectContinuation(comments, i, keywordColumn, patterns, out string details);
                TodoMetadata metadata = TodoMetadataParser.Parse(head, patterns);

                results.Add(new TodoEntry(patterns.Resolve(match.Value), metadata.Message, details, metadata,
                    assetPath, fileName, comment.Line, keywordColumn, continued + 1, origin));

                // The lines that were swallowed must not start an item of their own.
                i += continued;
            }
        }

        // Where the text of a comment line really starts. A block comment's inner lines and a
        // documentation comment begin with decoration that says nothing about how deep the line sits.
        private static int ContentColumn(CommentLine comment)
        {
            string text = comment.Text;
            int index = SkipWhitespace(text, 0);

            while (index < text.Length
                   && (text[index] == Decoration || text[index] == Slash))
                index++;

            return comment.TextColumn + SkipWhitespace(text, index);
        }

        private static int SkipWhitespace(string text, int start)
        {
            int index = start;

            while (index < text.Length
                   && char.IsWhiteSpace(text[index]))
                index++;

            return index;
        }

        private static string StripDecoration(CommentLine comment)
        {
            int start = ContentColumn(comment) - comment.TextColumn;

            return start >= comment.Text.Length
                ? string.Empty
                : comment.Text[start..].TrimEnd();
        }

        private static int CollectContinuation(List<CommentLine> comments, int start, int keywordColumn,
            TodoPatterns patterns, out string details)
        {
            details = string.Empty;

            if (patterns.Continuation == ETodoContinuation.SingleLine)
                return 0;

            CommentLine first = comments[start];
            StringBuilder builder = new();
            int taken = 0;

            for (int i = start + 1; i < comments.Count; i++)
            {
                CommentLine next = comments[i];

                if (!Continues(first, next, taken, keywordColumn, patterns))
                    break;

                if (builder.Length > 0)
                    builder.Append(LineBreak);

                builder.Append(StripDecoration(next));
                taken++;
            }

            details = builder.ToString();

            return taken;
        }

        private static bool Continues(CommentLine first, CommentLine next, int taken, int keywordColumn,
            TodoPatterns patterns)
        {
            if (next.BlockId != first.BlockId
                || next.Line != first.Line + taken + 1)
                return false;

            if (StripDecoration(next).Length == 0)
                return false;

            if (patterns.Keywords.IsMatch(next.Text))
                return false;

            return patterns.Continuation == ETodoContinuation.WholeBlock
                || ContentColumn(next) > keywordColumn;
        }
    }
}