using Base.ToolsPackage.Editor.NamingConventions.Data;
using Base.ToolsPackage.Editor.NamingConventions.Scanning;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the shared name check behind every rule: prefixes, suffixes, stripped text, the ignore
    /// list, an optional pattern and the casing underneath all of it. The suggestion has to be a name
    /// that would actually pass, otherwise a fix leaves the asset reported again right after.
    /// </summary>
    public sealed class NamingRuleEvaluatorTests
    {
        private const string NoSuffix = "";

        private NamingRule _rule;

        /// <summary>Every test starts from a rule demanding a prefix and pascal case.</summary>
        [SetUp]
        public void Build()
        {
            _rule = new NamingRule
            {
                Style = ENamingStyle.PascalCase
            };

            _rule.Prefixes.Add("T_");
        }

        /// <summary>A name carrying the prefix and the right casing passes.</summary>
        [Test]
        public void AWellFormedNamePasses()
            => Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_Rock", NoSuffix), Is.True);

        /// <summary>A missing prefix is reported and named.</summary>
        [Test]
        public void AMissingPrefixIsReported()
        {
            Assert.That(NamingRuleEvaluator.IsValid(_rule, "Rock", NoSuffix), Is.False);
            Assert.That(NamingRuleEvaluator.Reason(_rule, "Rock", NoSuffix), Does.Contain("T_"));
        }

        /// <summary>The suggestion adds the prefix the rule asks for.</summary>
        [Test]
        public void TheSuggestionAddsTheMissingPrefix()
            => Assert.That(NamingRuleEvaluator.Suggest(_rule, "Rock", NoSuffix), Is.EqualTo("T_Rock"));

        /// <summary>The suggested name is one that actually passes the rule.</summary>
        [Test]
        public void TheSuggestionPassesTheRule()
        {
            string suggestion = NamingRuleEvaluator.Suggest(_rule, "rock thing", NoSuffix);

            Assert.That(NamingRuleEvaluator.IsValid(_rule, suggestion, NoSuffix), Is.True, suggestion);
        }

        /// <summary>
        /// A prefix the name already carries is kept, so a name is not pushed onto the first entry of
        /// the list when it already used a valid one.
        /// </summary>
        [Test]
        public void APrefixTheNameAlreadyCarriesIsKept()
        {
            _rule.Prefixes.Add("SM_");

            Assert.That(NamingRuleEvaluator.Suggest(_rule, "SM_Kitchen", NoSuffix), Is.EqualTo("SM_Kitchen"));
        }

        /// <summary>
        /// A letter prefix only counts when the rest starts a new word, so an asset that merely begins
        /// with that letter is still reported.
        /// </summary>
        [Test]
        public void ALetterPrefixNeedsAWordBoundary()
        {
            NamingRule rule = new();

            rule.Prefixes.Add("T");

            Assert.That(NamingRuleEvaluator.IsValid(rule, "Trees", NoSuffix), Is.False);
            Assert.That(NamingRuleEvaluator.IsValid(rule, "TRock", NoSuffix), Is.True);
        }

        /// <summary>A space or a dash is never allowed in an asset name.</summary>
        [Test]
        public void ASpaceOrDashIsReported()
        {
            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_My Rock", NoSuffix), Is.False);
            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_My-Rock", NoSuffix), Is.False);
            Assert.That(NamingRuleEvaluator.Reason(_rule, "T_My Rock", NoSuffix), Does.Contain("space"));
        }

        /// <summary>Text on the strip list is reported wherever it sits.</summary>
        [Test]
        public void StrippedTextIsReported()
        {
            _rule.Stripped.Add("Copy");

            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_RockCopy", NoSuffix), Is.False);
            Assert.That(NamingRuleEvaluator.Reason(_rule, "T_RockCopy", NoSuffix), Does.Contain("Copy"));
        }

        /// <summary>The suggestion drops the stripped text.</summary>
        [Test]
        public void TheSuggestionDropsTheStrippedText()
        {
            _rule.Stripped.Add("Copy");

            Assert.That(NamingRuleEvaluator.Suggest(_rule, "T_RockCopy", NoSuffix), Is.EqualTo("T_Rock"));
        }

        /// <summary>An optional suffix is allowed but not demanded.</summary>
        [Test]
        public void AnOptionalSuffixIsNotDemanded()
        {
            _rule.Suffixes.Add("_A");

            Assert.That(_rule.SuffixOptional, Is.True);
            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_Rock", NoSuffix), Is.True);
            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_Rock_A", NoSuffix), Is.True);
        }

        /// <summary>A demanded suffix is reported when it is missing.</summary>
        [Test]
        public void ADemandedSuffixIsReported()
        {
            _rule.Suffixes.Add("_A");
            _rule.SuffixOptional = false;

            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_Rock", NoSuffix), Is.False);
            Assert.That(NamingRuleEvaluator.Suggest(_rule, "T_Rock", NoSuffix), Is.EqualTo("T_Rock_A"));
        }

        /// <summary>
        /// An optional suffix is only added back when the name already had one, so an asset without a
        /// sub type keeps its plain name.
        /// </summary>
        [Test]
        public void AnOptionalSuffixIsOnlyKeptWhenItWasThere()
        {
            _rule.Suffixes.Add("_A");

            Assert.That(NamingRuleEvaluator.Suggest(_rule, "T_rock", NoSuffix), Is.EqualTo("T_Rock"));
            Assert.That(NamingRuleEvaluator.Suggest(_rule, "T_rock_A", NoSuffix), Is.EqualTo("T_Rock_A"));
        }

        /// <summary>A suffix the asset's own sub type demands overrides the rule's list.</summary>
        [Test]
        public void ARequiredSuffixIsDemanded()
        {
            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_Rock_N", "_N"), Is.True);
            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_Rock", "_N"), Is.False);
            Assert.That(NamingRuleEvaluator.Reason(_rule, "T_Rock", "_N"), Does.Contain("_N"));
            Assert.That(NamingRuleEvaluator.Suggest(_rule, "T_Rock", "_N"), Is.EqualTo("T_Rock_N"));
        }

        /// <summary>The wrong casing is reported once everything else is in order.</summary>
        [Test]
        public void TheWrongCasingIsReported()
        {
            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_rock", NoSuffix), Is.False);
            Assert.That(NamingRuleEvaluator.Suggest(_rule, "T_rock", NoSuffix), Is.EqualTo("T_Rock"));
        }

        /// <summary>A name on the ignore list is skipped whatever it looks like.</summary>
        [Test]
        public void AnIgnoredNameIsSkipped()
        {
            _rule.IgnoredNames.Add("Temp*");

            Assert.That(NamingRuleEvaluator.IsValid(_rule, "TempThing", NoSuffix), Is.True);
            Assert.That(NamingRuleEvaluator.IsIgnored(_rule, "TempThing"), Is.True);
            Assert.That(NamingRuleEvaluator.IsIgnored(_rule, "OtherThing"), Is.False);
        }

        /// <summary>An ignore entry without a wildcard matches only that exact name.</summary>
        [Test]
        public void AnExactIgnoreEntryMatchesOnlyItself()
        {
            _rule.IgnoredNames.Add("Temp");

            Assert.That(NamingRuleEvaluator.IsIgnored(_rule, "Temp"), Is.True);
            Assert.That(NamingRuleEvaluator.IsIgnored(_rule, "TempThing"), Is.False);
        }

        /// <summary>A pattern replaces the casing, prefix and suffix checks entirely.</summary>
        [Test]
        public void APatternReplacesTheOtherChecks()
        {
            _rule.Pattern = "^X[0-9]+$";

            Assert.That(NamingRuleEvaluator.IsValid(_rule, "X12", NoSuffix), Is.True,
                "the missing prefix no longer matters");

            Assert.That(NamingRuleEvaluator.IsValid(_rule, "T_Rock", NoSuffix), Is.False);
            Assert.That(NamingRuleEvaluator.Reason(_rule, "T_Rock", NoSuffix), Does.Contain(_rule.Pattern));
        }

        /// <summary>
        /// A pattern that does not compile lets names through, so the window stays usable while the
        /// expression is still being typed.
        /// </summary>
        [Test]
        public void AnUnfinishedPatternLetsNamesThrough()
        {
            _rule.Pattern = "[";

            Assert.That(NamingRuleEvaluator.IsValid(_rule, "anything at all", NoSuffix), Is.True);
        }
    }
}