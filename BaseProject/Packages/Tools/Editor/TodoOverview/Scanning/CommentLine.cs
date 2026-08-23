namespace Base.ToolPackage.Editor.TodoOverview.Scanning
{
    /// <summary>
    /// One line of comment text lifted out of a source file, with enough position information to point
    /// an editor at the exact character the keyword starts on.
    /// </summary>
    internal readonly struct CommentLine
    {
        /// <summary>
        /// Identifies the run of comment lines this one belongs to. Consecutive line comments share an
        /// id, and so do all the lines of one block comment, which is what tells a continuation line
        /// apart from an unrelated comment further down.
        /// </summary>
        internal int BlockId { get; }

        /// <summary>One based line number in the source file.</summary>
        internal int Line { get; }

        /// <summary>Zero based column the comment text starts at, so the marker itself is not counted.</summary>
        internal int TextColumn { get; }

        /// <summary>The comment text of this line, without its marker and untrimmed.</summary>
        internal string Text { get; }

        /// <summary>Creates one line of comment text.</summary>
        /// <param name="blockId">The run of comment lines this one belongs to.</param>
        /// <param name="line">One based line number.</param>
        /// <param name="textColumn">Zero based column the text starts at.</param>
        /// <param name="text">The comment text of this line.</param>
        internal CommentLine(int blockId, int line, int textColumn, string text)
        {
            BlockId = blockId;
            Line = line;
            TextColumn = textColumn;
            Text = text;
        }
    }
}