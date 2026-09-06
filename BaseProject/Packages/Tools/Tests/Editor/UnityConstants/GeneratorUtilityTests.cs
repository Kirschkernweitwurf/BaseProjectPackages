using System.Collections.Generic;
using Base.ToolsPackage.Editor.UnityConstants;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the part of code generation that turns a tag or layer name into something a compiler
    /// accepts. A tag can be called anything at all, so anything at all has to come out as a valid,
    /// unique identifier or the generated file does not build.
    /// </summary>
    public sealed class GeneratorUtilityTests
    {
        private HashSet<string> _used;

        /// <summary>Every test starts with no names taken.</summary>
        [SetUp]
        public void Build() => _used = new HashSet<string>();

        /// <summary>A name that is already an identifier comes through untouched.</summary>
        [Test]
        public void AValidNameComesThroughUntouched()
            => Assert.That(GeneratorUtility.MakeUniqueIdentifier("Player", _used), Is.EqualTo("Player"));

        /// <summary>Anything a C# name cannot carry becomes an underscore.</summary>
        [Test]
        public void ForbiddenCharactersBecomeUnderscores()
        {
            Assert.That(GeneratorUtility.MakeUniqueIdentifier("My Tag", _used), Is.EqualTo("My_Tag"));
            Assert.That(GeneratorUtility.MakeUniqueIdentifier("a-b.c", _used), Is.EqualTo("a_b_c"));
        }

        /// <summary>A name that starts with a digit gets a prefix, since an identifier cannot.</summary>
        [Test]
        public void ALeadingDigitGetsAPrefix()
            => Assert.That(GeneratorUtility.MakeUniqueIdentifier("2Fast", _used), Is.EqualTo("_2Fast"));

        /// <summary>A name that reads as a keyword is escaped rather than renamed.</summary>
        [Test]
        public void AKeywordIsEscaped()
        {
            Assert.That(GeneratorUtility.MakeUniqueIdentifier("class", _used), Is.EqualTo("@class"));
            Assert.That(GeneratorUtility.MakeUniqueIdentifier("default", _used), Is.EqualTo("@default"));
        }

        /// <summary>A name that is only a keyword in another casing is left alone.</summary>
        [Test]
        public void ADifferentCasingIsNotAKeyword()
            => Assert.That(GeneratorUtility.MakeUniqueIdentifier("Class", _used), Is.EqualTo("Class"));

        /// <summary>An empty name still produces something a compiler accepts.</summary>
        /// <remarks>
        /// Each call gets its own set, because both of these clean up to the same identifier and the
        /// second would otherwise come back with a collision suffix rather than the name itself.
        /// </remarks>
        [Test]
        public void AnEmptyNameStillProducesAnIdentifier()
        {
            Assert.That(GeneratorUtility.MakeUniqueIdentifier(string.Empty, _used), Is.EqualTo("_"));
            Assert.That(GeneratorUtility.MakeUniqueIdentifier(null, new HashSet<string>()), Is.EqualTo("_"));
        }

        /// <summary>A second nameless entry is still told apart from the first.</summary>
        [Test]
        public void TwoNamelessEntriesAreToldApart()
        {
            Assert.That(GeneratorUtility.MakeUniqueIdentifier(string.Empty, _used), Is.EqualTo("_"));
            Assert.That(GeneratorUtility.MakeUniqueIdentifier(null, _used), Is.EqualTo("__1"));
        }

        /// <summary>
        /// Two names that clean up to the same identifier are told apart, since two constants of one
        /// name would not compile.
        /// </summary>
        [Test]
        public void CollidingNamesAreToldApart()
        {
            Assert.That(GeneratorUtility.MakeUniqueIdentifier("My Tag", _used), Is.EqualTo("My_Tag"));
            Assert.That(GeneratorUtility.MakeUniqueIdentifier("My.Tag", _used), Is.EqualTo("My_Tag_1"));
            Assert.That(GeneratorUtility.MakeUniqueIdentifier("My-Tag", _used), Is.EqualTo("My_Tag_2"));
        }

        /// <summary>A quote or a backslash is escaped, so the generated literal still closes.</summary>
        [Test]
        public void QuotesAndBackslashesAreEscaped()
        {
            Assert.That(GeneratorUtility.Escape("say \"hi\""), Is.EqualTo("say \\\"hi\\\""));
            Assert.That(GeneratorUtility.Escape(@"a\b"), Is.EqualTo(@"a\\b"));
        }

        /// <summary>A plain value passes through the escaping untouched.</summary>
        [Test]
        public void APlainValueIsNotEscaped() => Assert.That(GeneratorUtility.Escape("Player"), Is.EqualTo("Player"));
    }
}