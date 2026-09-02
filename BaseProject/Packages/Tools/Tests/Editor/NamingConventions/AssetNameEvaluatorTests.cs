using Base.ToolsPackage.Editor.NamingConventions.Data;
using Base.ToolsPackage.Editor.NamingConventions.Scanning;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the file name check on top of the shared rule: the trailing number and where it belongs.
    /// A name always has to read prefix, base, number, suffix, so a texture numbered before its sub
    /// type marker gets reordered rather than only renumbered.
    /// </summary>
    public sealed class AssetNameEvaluatorTests
    {
        private const int Digits = 2;
        private const string NoSuffix = "";

        private AssetNamingRule _rule;

        /// <summary>Every test starts from a rule demanding a prefix, pascal case and two digits.</summary>
        [SetUp]
        public void Build()
        {
            _rule = new AssetNamingRule("Textures", string.Empty, ENamingStyle.PascalCase)
            {
                EnumerationDigits = Digits
            };

            _rule.Naming.Prefixes.Add("T_");
        }

        /// <summary>A trailing number is split off with or without its underscore.</summary>
        [Test]
        public void ATrailingNumberIsSplitOff()
        {
            Assert.That(AssetNameEvaluator.TrySplitEnumeration("Lamp_01", out string core, out string number),
                Is.True);

            Assert.That(core, Is.EqualTo("Lamp"));
            Assert.That(number, Is.EqualTo("01"));

            Assert.That(AssetNameEvaluator.TrySplitEnumeration("Lamp01", out core, out number), Is.True);
            Assert.That(core, Is.EqualTo("Lamp"));
            Assert.That(number, Is.EqualTo("01"));
        }

        /// <summary>A name without a number keeps itself as the core.</summary>
        [Test]
        public void ANameWithoutANumberIsNotSplit()
        {
            Assert.That(AssetNameEvaluator.TrySplitEnumeration("Lamp", out string core, out string number),
                Is.False);

            Assert.That(core, Is.EqualTo("Lamp"));
            Assert.That(number, Is.Empty);
        }

        /// <summary>A name that is nothing but digits has no core, so it is left alone.</summary>
        [Test]
        public void ANameOfOnlyDigitsIsNotSplit()
            => Assert.That(AssetNameEvaluator.TrySplitEnumeration("01", out string _, out string _), Is.False);

        /// <summary>Nothing to split is not an error.</summary>
        [Test]
        public void NothingToSplitIsNotAnError()
        {
            Assert.That(AssetNameEvaluator.TrySplitEnumeration(null, out string _, out string _), Is.False);
            Assert.That(AssetNameEvaluator.TrySplitEnumeration(string.Empty, out string _, out string _), Is.False);
        }

        /// <summary>A well formed numbered name passes.</summary>
        [Test]
        public void AWellFormedNumberedNamePasses()
            => Assert.That(AssetNameEvaluator.IsValid(_rule, "T_Kitchen_01", NoSuffix), Is.True);

        /// <summary>A name without a number passes too, since a number is never demanded.</summary>
        [Test]
        public void ANameWithoutANumberPasses()
            => Assert.That(AssetNameEvaluator.IsValid(_rule, "T_Kitchen", NoSuffix), Is.True);

        /// <summary>
        /// A number without its underscore is a violation on its own, even when everything else is
        /// already right.
        /// </summary>
        [Test]
        public void ANumberWithoutItsUnderscoreIsReported()
        {
            Assert.That(AssetNameEvaluator.IsValid(_rule, "T_Kitchen01", NoSuffix), Is.False);
            Assert.That(AssetNameEvaluator.Reason(_rule, "T_Kitchen01", NoSuffix), Does.Contain("_"));
            Assert.That(AssetNameEvaluator.Suggest(_rule, "T_Kitchen01", NoSuffix), Is.EqualTo("T_Kitchen_01"));
        }

        /// <summary>A number of the wrong length is reported and padded.</summary>
        [Test]
        public void ANumberOfTheWrongLengthIsPadded()
        {
            Assert.That(AssetNameEvaluator.IsValid(_rule, "T_Kitchen_1", NoSuffix), Is.False);
            Assert.That(AssetNameEvaluator.Reason(_rule, "T_Kitchen_1", NoSuffix), Does.Contain(Digits.ToString()));
            Assert.That(AssetNameEvaluator.Suggest(_rule, "T_Kitchen_1", NoSuffix), Is.EqualTo("T_Kitchen_01"));
        }

        /// <summary>A digit count of zero accepts any length.</summary>
        [Test]
        public void ADigitCountOfZeroAcceptsAnyLength()
        {
            _rule.EnumerationDigits = 0;

            Assert.That(AssetNameEvaluator.IsValid(_rule, "T_Kitchen_1", NoSuffix), Is.True);
            Assert.That(AssetNameEvaluator.IsValid(_rule, "T_Kitchen_0001", NoSuffix), Is.True);
        }

        /// <summary>The rest of the name still goes through the shared rule.</summary>
        [Test]
        public void TheRestOfTheNameStillGoesThroughTheRule()
        {
            Assert.That(AssetNameEvaluator.IsValid(_rule, "Kitchen_01", NoSuffix), Is.False,
                "the prefix is still missing");

            Assert.That(AssetNameEvaluator.Suggest(_rule, "kitchen_01", NoSuffix), Is.EqualTo("T_Kitchen_01"));
        }

        /// <summary>
        /// The number belongs in front of the suffix, so a texture numbered behind its sub type marker
        /// is reordered rather than only renumbered.
        /// </summary>
        [Test]
        public void TheNumberIsMovedInFrontOfTheSuffix()
        {
            _rule.Naming.Suffixes.Add("_N");

            Assert.That(AssetNameEvaluator.Suggest(_rule, "T_PhoneScreen_N_2", NoSuffix),
                Is.EqualTo("T_PhoneScreen_02_N"));
        }

        /// <summary>A name that already reads in the right order is left alone.</summary>
        [Test]
        public void AlreadyOrderedNamesAreLeftAlone()
        {
            _rule.Naming.Suffixes.Add("_N");

            Assert.That(AssetNameEvaluator.IsValid(_rule, "T_PhoneScreen_02_N", NoSuffix), Is.True);
            Assert.That(AssetNameEvaluator.Suggest(_rule, "T_PhoneScreen_02_N", NoSuffix),
                Is.EqualTo("T_PhoneScreen_02_N"));
        }

        /// <summary>The suggested name is one that actually passes the rule.</summary>
        [Test]
        public void TheSuggestionPassesTheRule()
        {
            _rule.Naming.Suffixes.Add("_N");

            string suggestion = AssetNameEvaluator.Suggest(_rule, "phone screen_N_2", NoSuffix);

            Assert.That(AssetNameEvaluator.IsValid(_rule, suggestion, NoSuffix), Is.True, suggestion);
        }
    }
}