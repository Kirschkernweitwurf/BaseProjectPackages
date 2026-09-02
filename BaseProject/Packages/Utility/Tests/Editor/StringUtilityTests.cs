using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the two promises the helper makes: a raw field name reads as words, and the hash of a
    /// string is the same value every session, so colors and file names derived from it are stable.
    /// </summary>
    public sealed class StringUtilityTests
    {
        private const string CamelCaseName = "overrideEnabled";
        private const string CapitalRunName = "myHTTPServer";
        private const uint OffsetBasis = 2166136261u;
        private const string UnderscoreName = "audio_manager";

        /// <summary>A capital in the middle of a word starts a new word.</summary>
        [Test]
        public void CamelCaseBecomesSeparateWords()
            => Assert.That(StringUtility.NicifyVariableName(CamelCaseName), Is.EqualTo("Override Enabled"));

        /// <summary>An underscore is a word break, not a character to keep.</summary>
        [Test]
        public void UnderscoresBecomeWordBreaks()
            => Assert.That(StringUtility.NicifyVariableName(UnderscoreName), Is.EqualTo("Audio Manager"));

        /// <summary>An acronym must not be torn apart into single letters.</summary>
        [Test]
        public void RunsOfCapitalsStayTogether()
            => Assert.That(StringUtility.NicifyVariableName(CapitalRunName), Is.EqualTo("My HTTPServer"));

        /// <summary>Nothing in means nothing out, not a crash.</summary>
        [Test]
        public void AMissingNameBecomesAnEmptyString()
        {
            Assert.That(StringUtility.NicifyVariableName(null), Is.Empty);
            Assert.That(StringUtility.NicifyVariableName(string.Empty), Is.Empty);
        }

        /// <summary>
        /// Documents the current handling of a leading underscore: the word break is inserted before
        /// there is a word, so the result starts with a space.
        /// </summary>
        [Test]
        public void ALeadingUnderscoreKeepsTheSpaceItInserts()
            => Assert.That(StringUtility.NicifyVariableName("_isDirty"), Is.EqualTo(" Is Dirty"));

        /// <summary>The same text has to hash to the same value every time it is asked.</summary>
        [Test]
        public void TheSameTextAlwaysHashesTheSame()
        {
            uint first = StringUtility.GetStableHash(CamelCaseName);
            uint second = StringUtility.GetStableHash(CamelCaseName);

            Assert.That(second, Is.EqualTo(first));
        }

        /// <summary>No text is not an error, it is the starting value of the hash.</summary>
        [Test]
        public void AMissingTextHashesToTheOffsetBasis()
        {
            Assert.That(StringUtility.GetStableHash(null), Is.EqualTo(OffsetBasis));
            Assert.That(StringUtility.GetStableHash(string.Empty), Is.EqualTo(OffsetBasis));
        }

        /// <summary>Two different strings must not land on the same value.</summary>
        [Test]
        public void DifferentTextHashesDifferently()
            => Assert.That(StringUtility.GetStableHash("Beta"), Is.Not.EqualTo(StringUtility.GetStableHash("Alpha")));

        /// <summary>Case is part of the text, so it has to change the hash.</summary>
        [Test]
        public void CaseChangesTheHash()
            => Assert.That(StringUtility.GetStableHash("alpha"), Is.Not.EqualTo(StringUtility.GetStableHash("Alpha")));
    }
}