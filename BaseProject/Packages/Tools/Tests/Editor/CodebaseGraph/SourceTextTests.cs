using System.Collections.Generic;
using Base.ToolsPackage.Editor.CodebaseGraph.Scanning;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Checks that the identifier count reads code and only code. It is the entire evidence for whether
    /// an inlined const is used, so a name appearing once inside a log message anywhere in the project
    /// would otherwise keep that const alive forever.
    /// <br/><br/>
    /// These are pure string functions with no editor and no project behind them, which makes them the
    /// cheapest thing in the package to be certain about.
    /// </summary>
    public sealed class SourceTextTests
    {
        private const string Name = "SharedLabel";

        /// <summary>Ordinary code is counted.</summary>
        [Test]
        public void CodeIsCounted() => AssertCount("int SharedLabel = 1;", 1);

        /// <summary>A plain string literal is text, not code.</summary>
        [Test]
        public void StringLiteralIsNotCounted() => AssertCount("Log(\"SharedLabel\");", 0);

        /// <summary>A line comment is not code either.</summary>
        [Test]
        public void LineCommentIsNotCounted() => AssertCount("int x = 1; // SharedLabel", 0);

        /// <summary>Neither is a block comment.</summary>
        [Test]
        public void BlockCommentIsNotCounted() => AssertCount("int x = 1; /* SharedLabel */", 0);

        /// <summary>A verbatim string carries no code, including where it escapes its own quotes.</summary>
        [Test]
        public void VerbatimStringIsNotCounted() => AssertCount("string s = @\"raw \"\"quoted\"\" SharedLabel\";", 0);

        /// <summary>An escaped quote does not end the literal early and let the rest read as code.</summary>
        [Test]
        public void EscapedQuoteDoesNotEndTheLiteral() => AssertCount("string s = \"he said \\\" SharedLabel\";", 0);

        /// <summary>The code inside an interpolation hole is code.</summary>
        [Test]
        public void InterpolationHoleIsCounted() => AssertCount("string s = $\"value {SharedLabel}\";", 1);

        /// <summary>A doubled brace is a literal brace rather than the start of a hole.</summary>
        [Test]
        public void EscapedBraceIsNotAHole() => AssertCount("string s = $\"{{SharedLabel}} and {SharedLabel}\";", 1);

        /// <summary>A character literal holding a quote does not open a string.</summary>
        [Test]
        public void CharLiteralQuoteDoesNotOpenAString() => AssertCount("char c = \'\"\'; int SharedLabel = 1;", 1);

        private static void AssertCount(string source, int expected)
        {
            Dictionary<string, int> counts = SourceTextScanner.CountIdentifiers(source);
            counts.TryGetValue(Name, out int actual);

            Assert.That(actual,
                Is.EqualTo(expected),
                $"counting identifiers in: {source}");
        }
    }
}