using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.OrderManagement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the text the Order Manager writes into the project. Whatever comes out of here is
    /// compiled, so a name the generator lets through unchanged or a comment it fails to escape does
    /// not produce a wrong value, it breaks the build.
    /// </summary>
    public sealed class OrderCodeGeneratorTests
    {
        private const string ClassName = "MenuOrders";
        private const string DuplicateWarning = "duplicate constant name";
        private const string Namespace = "Generated.UnityConstants";

        /// <summary>The file declares the namespace and class it was configured with.</summary>
        [Test]
        public void TheNamespaceAndClassAreDeclared()
        {
            string code = Build(out int _, new OrderConstant("First", 0));

            Assert.That(code, Does.Contain($"namespace {Namespace}"));
            Assert.That(code, Does.Contain($"public static class {ClassName}"));
        }

        /// <summary>A constant is emitted as a public int with its value.</summary>
        [Test]
        public void AConstantIsEmittedWithItsValue()
            => Assert.That(Build(out int _, new OrderConstant("First", 42)),
                Does.Contain("public const int First = 42;"));

        /// <summary>
        /// Constants are ordered by value, because the generated file is read by a person deciding
        /// what number to use next and a list sorted by name tells them nothing.
        /// </summary>
        [Test]
        public void ConstantsAreOrderedByValue()
        {
            string code = Build(out int _, new OrderConstant("Later", 20), new OrderConstant("Earlier", 10));

            Assert.That(code.IndexOf("Earlier", StringComparison.Ordinal),
                Is.LessThan(code.IndexOf("Later", StringComparison.Ordinal)));
        }

        /// <summary>Two constants on the same value fall back to their names, so the file is stable.</summary>
        [Test]
        public void ConstantsOnTheSameValueAreOrderedByName()
        {
            string code = Build(out int _, new OrderConstant("Beta", 10), new OrderConstant("Alpha", 10));

            Assert.That(code.IndexOf("Alpha", StringComparison.Ordinal),
                Is.LessThan(code.IndexOf("Beta", StringComparison.Ordinal)));
        }

        /// <summary>A comment becomes the constant's summary, since that is the only place it can go.</summary>
        [Test]
        public void ACommentBecomesAnXmlSummary()
            => Assert.That(Build(out int _, new OrderConstant("First", 0, "Runs before everything else")),
                Does.Contain("/// <summary>Runs before everything else</summary>"));

        /// <summary>No comment means no empty summary left hanging above the constant.</summary>
        [Test]
        public void NoCommentMeansNoSummary()
            => Assert.That(Build(out int _, new OrderConstant("First", 0)).Contains("<summary>"), Is.False);

        /// <summary>A comment over several lines opens and closes the summary on its own lines.</summary>
        [Test]
        public void AMultiLineCommentBecomesABlockSummary()
        {
            string code = Build(out int _, new OrderConstant("First", 0, "Line one\nLine two"));

            // The single line form puts the text straight after the tag, so a bare "/// Line one"
            // can only come from the block form.
            Assert.That(code, Does.Contain("/// Line one"));
            Assert.That(code, Does.Contain("/// Line two"));
        }

        /// <summary>
        /// An angle bracket in a comment would end the summary tag early and stop the file compiling,
        /// so the three XML characters are escaped.
        /// </summary>
        [Test]
        public void XmlCharactersInACommentAreEscaped()
        {
            string code = Build(out int _, new OrderConstant("First", 0, "a < b & c > d"));

            Assert.That(code, Does.Contain("a &lt; b &amp; c &gt; d"));
        }

        /// <summary>
        /// A name is typed by hand into a window, so anything that is not a letter or digit is dropped
        /// rather than emitted into an identifier that cannot compile.
        /// </summary>
        [Test]
        public void AnInvalidCharacterIsStrippedFromTheName()
            => Assert.That(Build(out int _, new OrderConstant("My Order!", 0)),
                Does.Contain("public const int MyOrder = 0;"));

        /// <summary>An identifier cannot start with a digit, so one gets an underscore in front.</summary>
        [Test]
        public void ANameStartingWithADigitGetsAnUnderscore()
            => Assert.That(Build(out int _, new OrderConstant("1st", 0)),
                Does.Contain("public const int _1st = 0;"));

        /// <summary>A name with nothing usable left in it is skipped rather than emitted empty.</summary>
        [Test]
        public void ANameWithNothingUsableIsSkipped()
        {
            Build(out int count, new OrderConstant("!!!", 0));

            Assert.That(count, Is.EqualTo(0));
        }

        /// <summary>
        /// Two names that strip down to the same identifier would declare it twice, so the second is
        /// dropped and said out loud rather than breaking the build silently.
        /// </summary>
        [Test]
        public void ADuplicateIdentifierIsReportedAndSkipped()
        {
            LogAssert.Expect(LogType.Warning, new Regex(DuplicateWarning));

            Build(out int count, new OrderConstant("My Order", 0), new OrderConstant("My-Order", 1));

            Assert.That(count, Is.EqualTo(1));
        }

        /// <summary>The reported count is what was written, not what was handed in.</summary>
        [Test]
        public void TheCountIsWhatWasActuallyEmitted()
        {
            Build(out int count, new OrderConstant("First", 0), new OrderConstant("Second", 1));

            Assert.That(count, Is.EqualTo(2));
        }

        /// <summary>Generates a file from the given constants.</summary>
        private static string Build(out int count, params OrderConstant[] constants)
            => OrderCodeGenerator.BuildCode(Namespace, ClassName, new List<OrderConstant>(constants), out count);
    }
}