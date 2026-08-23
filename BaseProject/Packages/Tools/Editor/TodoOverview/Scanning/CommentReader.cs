using System;
using System.Collections.Generic;

namespace Base.ToolPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// Lifts the comments out of a source file and leaves everything else behind.
    /// <para>
    /// Walking the characters rather than running a regular expression over the file is what makes the
    /// difference between a comment and a string: a log message that mentions a keyword is not a task,
    /// and neither is a keyword inside a string that happens to look like a comment. Strings, verbatim
    /// strings and character literals are therefore skipped, and both comment forms are followed across
    /// line ends.
    /// </para>
    /// </summary>
    internal sealed class CommentReader
    {
        private const string BlockClose = "*/";
        private const char CarriageReturn = '\r';
        private const char CharQuote = '\'';
        private const char CommentStar = '*';
        private const char Escape = '\\';
        private const char Interpolation = '$';
        private const char LineBreak = '\n';
        private const int MarkerLength = 2;
        private const char Quote = '"';
        private const char Slash = '/';
        private const char Verbatim = '@';

        private readonly List<CommentLine> _comments = new();

        private int _blockId;
        private int _previousLineComment = -1;
        private bool _inBlock;
        private bool _inVerbatim;

        /// <summary>Reads every comment line of a source file, in the order they appear.</summary>
        /// <param name="source">The full text of the file.</param>
        /// <returns>The comment lines the file contains.</returns>
        internal static List<CommentLine> Read(string source)
        {
            CommentReader reader = new();

            return reader.ReadAll(source);
        }

        // Both spellings of an interpolated verbatim string have to be recognized, because only the
        // verbatim ones may run past the end of the line.
        private static bool IsVerbatim(string line, int index)
        {
            if (index > 0
                && line[index - 1] == Verbatim)
                return true;

            return index > 1
                && line[index - 2] == Verbatim
                && line[index - 1] == Interpolation;
        }

        private static int SkipCharLiteral(string line, int index)
        {
            for (int i = index + 1; i < line.Length; i++)
            {
                if (line[i] == Escape)
                {
                    i++;
                    continue;
                }

                if (line[i] == CharQuote)
                    return i + 1;
            }

            return line.Length;
        }

        private static int SkipString(string line, int index)
        {
            for (int i = index + 1; i < line.Length; i++)
            {
                if (line[i] == Escape)
                {
                    i++;
                    continue;
                }

                if (line[i] == Quote)
                    return i + 1;
            }

            return line.Length;
        }

        private List<CommentLine> ReadAll(string source)
        {
            if (string.IsNullOrEmpty(source))
                return _comments;

            string[] lines = source.Split(LineBreak);

            for (int i = 0; i < lines.Length; i++)
                ReadLine(lines[i].TrimEnd(CarriageReturn), i + 1);

            return _comments;
        }

        private void ReadLine(string line, int number)
        {
            int index = 0;

            if (_inVerbatim)
            {
                index = ContinueVerbatim(line, 0);

                if (_inVerbatim)
                    return;
            }

            if (_inBlock)
            {
                index = ContinueBlock(line, number);

                if (_inBlock)
                    return;
            }

            ScanCode(line, index, number);
        }

        private void ScanCode(string line, int start, int number)
        {
            int index = start;

            while (index < line.Length)
            {
                char current = line[index];

                if (current == Slash
                    && index + 1 < line.Length)
                {
                    if (line[index + 1] == Slash)
                    {
                        EmitLineComment(number, index + MarkerLength, line[(index + MarkerLength)..]);
                        return;
                    }

                    if (line[index + 1] == CommentStar)
                    {
                        index = OpenBlock(line, index, number);
                        continue;
                    }
                }

                if (current == Quote)
                {
                    index = IsVerbatim(line, index)
                        ? ContinueVerbatim(line, index + 1)
                        : SkipString(line, index);

                    continue;
                }

                if (current == CharQuote)
                {
                    index = SkipCharLiteral(line, index);
                    continue;
                }

                index++;
            }
        }

        // A verbatim string ends at the first quote that is not doubled, and may run over line ends.
        private int ContinueVerbatim(string line, int start)
        {
            for (int i = start; i < line.Length; i++)
            {
                if (line[i] != Quote)
                    continue;

                if (i + 1 < line.Length
                    && line[i + 1] == Quote)
                {
                    i++;
                    continue;
                }

                _inVerbatim = false;

                return i + 1;
            }

            _inVerbatim = true;

            return line.Length;
        }

        private int OpenBlock(string line, int index, int number)
        {
            int textStart = index + MarkerLength;
            int close = line.IndexOf(BlockClose, textStart, StringComparison.Ordinal);

            string text = close < 0
                ? line[textStart..]
                : line[textStart..close];

            _blockId++;
            _previousLineComment = -1;
            _comments.Add(new CommentLine(_blockId, number, textStart, text));

            if (close >= 0)
                return close + MarkerLength;

            _inBlock = true;

            return line.Length;
        }

        private int ContinueBlock(string line, int number)
        {
            int close = line.IndexOf(BlockClose, StringComparison.Ordinal);

            string text = close < 0
                ? line
                : line[..close];

            _comments.Add(new CommentLine(_blockId, number, 0, text));

            if (close < 0)
                return line.Length;

            _inBlock = false;

            return close + MarkerLength;
        }

        // Line comments on neighboring lines form one run, so a continuation line can be told apart
        // from a comment that only happens to sit further down in the file.
        private void EmitLineComment(int number, int column, string text)
        {
            if (_previousLineComment != number - 1)
                _blockId++;

            _previousLineComment = number;
            _comments.Add(new CommentLine(_blockId, number, column, text));
        }
    }
}