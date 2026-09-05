using System.Collections.Generic;
using Base.ToolsPackage.Editor.TodoOverview.Scanning;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers what separates a task from text that only looks like one. The reader walks characters
    /// rather than pattern matching, so a keyword inside a string is skipped, comments are followed
    /// across line ends, and neighboring lines are grouped into one run. Get any of that wrong and the
    /// overview either invents items out of log messages or splits one item into several.
    /// </summary>
    public sealed class CommentReaderTests
    {
        private const string LineBreak = "\n";

        /// <summary>A line comment is one entry, pointing at the line it sits on.</summary>
        [Test]
        public void ALineCommentIsReadWithItsLineNumber()
        {
            List<CommentLine> comments = CommentReader.Read(Source("int i = 0;", "// TODO fix this"));

            Assert.That(comments, Has.Count.EqualTo(1));
            Assert.That(comments[0].Line, Is.EqualTo(2));
            Assert.That(comments[0].Text.Trim(), Is.EqualTo("TODO fix this"));
        }

        /// <summary>The column points past the marker, so an editor lands on the text and not the slashes.</summary>
        [Test]
        public void TheColumnSkipsTheCommentMarker()
        {
            List<CommentLine> comments = CommentReader.Read("// TODO");

            Assert.That(comments[0].TextColumn, Is.EqualTo(2));
        }

        /// <summary>
        /// The reason the reader walks characters at all: a log message that mentions a keyword is not
        /// a task, and a string that looks like a comment is still a string.
        /// </summary>
        [Test]
        public void AKeywordInsideAStringIsNotAComment()
        {
            List<CommentLine> comments = CommentReader.Read("Debug.Log(\"// TODO not a task\");");

            Assert.That(comments, Is.Empty);
        }

        /// <summary>A block comment reports one entry per line, all belonging to the same run.</summary>
        [Test]
        public void ABlockCommentIsOneRunAcrossItsLines()
        {
            List<CommentLine> comments = CommentReader.Read(Source("/* TODO first", "second line */"));

            Assert.That(comments, Has.Count.EqualTo(2));
            Assert.That(comments[1].BlockId, Is.EqualTo(comments[0].BlockId));
        }

        /// <summary>
        /// Neighboring line comments are one item with a continuation, which is what lets a task run
        /// onto a second line.
        /// </summary>
        [Test]
        public void NeighboringLineCommentsShareOneRun()
        {
            List<CommentLine> comments = CommentReader.Read(Source("// TODO first", "// still the same"));

            Assert.That(comments, Has.Count.EqualTo(2));
            Assert.That(comments[1].BlockId, Is.EqualTo(comments[0].BlockId));
        }

        /// <summary>A gap between two comments makes them two items rather than one long one.</summary>
        [Test]
        public void ACommentAfterAGapStartsANewRun()
        {
            List<CommentLine> comments = CommentReader.Read(Source("// TODO first", "int i = 0;", "// TODO second"));

            Assert.That(comments, Has.Count.EqualTo(2));
            Assert.That(comments[1].BlockId, Is.Not.EqualTo(comments[0].BlockId));
        }

        /// <summary>
        /// A verbatim string may run past the end of a line, so the reader has to follow it to its
        /// closing quote instead of treating the next line as code.
        /// </summary>
        [Test]
        public void AVerbatimStringIsFollowedAcrossLineEnds()
        {
            List<CommentLine> comments =
                CommentReader.Read(Source("string s = @\"first", "// TODO inside the string\";", "// TODO real"));

            Assert.That(comments, Has.Count.EqualTo(1));
            Assert.That(comments[0].Line, Is.EqualTo(3));
        }

        /// <summary>Nothing in means nothing out, not a crash.</summary>
        [Test]
        public void AnEmptyFileHasNoComments()
        {
            Assert.That(CommentReader.Read(string.Empty), Is.Empty);
            Assert.That(CommentReader.Read(null), Is.Empty);
        }

        /// <summary>Joins lines into one source text, so a test reads as the file it stands for.</summary>
        private static string Source(params string[] lines) => string.Join(LineBreak, lines);
    }
}