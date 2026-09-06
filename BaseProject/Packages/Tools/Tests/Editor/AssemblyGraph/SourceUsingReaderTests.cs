using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.AssemblyGraph;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the line parsing behind the source side of the reference check. A using directive is
    /// the only trace left of an assembly whose contribution the compiler folded into a literal, so
    /// a directive this misses is a reference the graph offers to remove out of a compiling project.
    /// </summary>
    public sealed class SourceUsingReaderTests
    {
        /// <summary>A plain directive names its namespace.</summary>
        /// <param name="line">Line of source to parse.</param>
        /// <param name="expected">Namespace the line has to name.</param>
        [TestCase("using Base.UtilityPackage.Editor;", "Base.UtilityPackage.Editor")]
        [TestCase("    using Base.UtilityPackage.Editor;", "Base.UtilityPackage.Editor")]
        [TestCase("using Base.UtilityPackage.Editor; // for the script property name",
            "Base.UtilityPackage.Editor")]
        [TestCase("global using Base.UtilityPackage.Editor;", "Base.UtilityPackage.Editor")]
        [TestCase("using static Base.UtilityPackage.Editor.EditorConstants;", "Base.UtilityPackage.Editor")]
        [TestCase("using Constants = Base.UtilityPackage.Editor.EditorConstants;",
            "Base.UtilityPackage.Editor")]
        [TestCase("using Map = System.Collections.Generic.Dictionary<string, int>;",
            "System.Collections.Generic")]
        public void ReadLine_NamesNamespace(string line, string expected)
        {
            HashSet<string> namespaces = new(StringComparer.Ordinal);

            SourceUsingReader.ReadLine(line, namespaces);

            Assert.That(namespaces, Does.Contain(expected));
        }

        /// <summary>Lines that only look like a directive name nothing.</summary>
        /// <param name="line">Line of source to parse.</param>
        [TestCase("// using Base.UtilityPackage.Editor;")]
        [TestCase("using (StreamReader reader = new(path))")]
        [TestCase("using StreamReader reader = new(path);")]
        [TestCase("namespace Base.UtilityPackage.Editor")]
        [TestCase("            string text = \"using Base.UtilityPackage.Editor;\";")]
        public void ReadLine_IgnoresNonDirective(string line)
        {
            HashSet<string> namespaces = new(StringComparer.Ordinal);

            SourceUsingReader.ReadLine(line, namespaces);

            Assert.That(namespaces, Is.Empty);
        }

        /// <summary>An alias credits the namespace reading as well, since the text says which it is not.</summary>
        [Test]
        public void ReadLine_CreditsBothReadingsOfAnAlias()
        {
            HashSet<string> namespaces = new(StringComparer.Ordinal);

            SourceUsingReader.ReadLine("using Assembly = UnityEditor.Compilation.Assembly;", namespaces);

            Assert.That(namespaces, Does.Contain("UnityEditor.Compilation"));
            Assert.That(namespaces, Does.Contain("UnityEditor.Compilation.Assembly"));
        }
    }
}