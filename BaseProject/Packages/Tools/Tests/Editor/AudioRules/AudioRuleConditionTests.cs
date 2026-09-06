using Base.ToolsPackage.Editor.AudioRules.Data;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests.AudioRules
{
    /// <summary>
    /// Whether one test a rule carries holds for a clip. This is the whole of what decides which rule
    /// touches which file, so a wrong answer here reimports the wrong clips and says nothing about it.
    /// </summary>
    public sealed class AudioRuleConditionTests
    {
        private const string LoopPath = "Assets/Audio/Music/Theme_loop.wav";
        private const float Threshold = 2f;

        /// <summary>
        /// The numeric fields are the ones compared against a number.
        /// </summary>
        /// <param name="field">
        /// The field to test, as its underlying byte. The enum is internal to the package and a public
        /// test method cannot name it in its signature.
        /// </param>
        [TestCase((byte)EConditionField.Channels)]
        [TestCase((byte)EConditionField.DurationSeconds)]
        [TestCase((byte)EConditionField.FileSizeKilobytes)]
        [TestCase((byte)EConditionField.SampleRate)]
        public void ANumericFieldIsRecognized(byte field)
        {
            AudioRuleCondition condition = new((EConditionField)field, EConditionOperator.LessThan, 0f);

            Assert.That(condition.IsNumeric, Is.True);
        }

        /// <summary>Looping is the one field that is only ever on or off.</summary>
        [Test]
        public void TheLoopingFieldIsAFlag()
        {
            AudioRuleCondition condition = new(EConditionField.IsLooping, EConditionOperator.Equals, 1f);

            Assert.That(condition.IsFlag, Is.True);
            Assert.That(condition.IsNumeric, Is.False);
        }

        /// <summary>Below means below, so the boundary itself does not match.</summary>
        [Test]
        public void LessThanExcludesTheBoundary()
        {
            AudioRuleCondition condition = new(EConditionField.DurationSeconds,
                EConditionOperator.LessThan, Threshold);

            Assert.That(condition.MatchesNumber(1.9f), Is.True);
            Assert.That(condition.MatchesNumber(Threshold), Is.False);
        }

        /// <summary>Or equal includes it, which is the difference between the two operators.</summary>
        [Test]
        public void LessOrEqualIncludesTheBoundary()
        {
            AudioRuleCondition condition = new(EConditionField.DurationSeconds,
                EConditionOperator.LessOrEqual, Threshold);

            Assert.That(condition.MatchesNumber(Threshold), Is.True);
            Assert.That(condition.MatchesNumber(2.1f), Is.False);
        }

        /// <summary>Above means above, and or equal includes the boundary.</summary>
        [Test]
        public void GreaterThanAndGreaterOrEqualDifferOnTheBoundary()
        {
            AudioRuleCondition above = new(EConditionField.DurationSeconds,
                EConditionOperator.GreaterThan, Threshold);

            AudioRuleCondition atLeast = new(EConditionField.DurationSeconds,
                EConditionOperator.GreaterOrEqual, Threshold);

            Assert.That(above.MatchesNumber(Threshold), Is.False);
            Assert.That(atLeast.MatchesNumber(Threshold), Is.True);
        }

        /// <summary>
        /// Equality on a float is compared with a tolerance. A duration read back from an imported
        /// file is never exactly the number somebody typed into the rule.
        /// </summary>
        [Test]
        public void EqualityAllowsForFloatingPointDrift()
        {
            AudioRuleCondition condition = new(EConditionField.DurationSeconds,
                EConditionOperator.Equals, Threshold);

            Assert.That(condition.MatchesNumber(2.00001f), Is.True);
            Assert.That(condition.MatchesNumber(2.01f), Is.False);
        }

        /// <summary>Not equal is the exact opposite of equal, tolerance included.</summary>
        [Test]
        public void NotEqualIsTheOppositeOfEqual()
        {
            AudioRuleCondition condition = new(EConditionField.DurationSeconds,
                EConditionOperator.NotEquals, Threshold);

            Assert.That(condition.MatchesNumber(2.00001f), Is.False);
            Assert.That(condition.MatchesNumber(2.01f), Is.True);
        }

        /// <summary>
        /// Text is compared without regard to case. An asset called Music and one called music are the
        /// same folder on Windows, so a rule that told them apart would behave differently per machine.
        /// </summary>
        [Test]
        public void TextIsComparedWithoutRegardToCase()
        {
            AudioRuleCondition condition = new(EConditionField.Path, EConditionOperator.Contains, "/MUSIC/");

            Assert.That(condition.MatchesText(LoopPath), Is.True);
        }

        /// <summary>An empty value matches nothing, rather than matching everything.</summary>
        [Test]
        public void AnEmptyValueMatchesNothing()
        {
            AudioRuleCondition condition = new(EConditionField.Path, EConditionOperator.Contains, string.Empty);

            Assert.That(condition.MatchesText(LoopPath), Is.False);
        }

        /// <summary>Not contains is the opposite, so an empty value there matches everything.</summary>
        [Test]
        public void NotContainsIsTheOpposite()
        {
            AudioRuleCondition condition = new(EConditionField.Path, EConditionOperator.NotContains, "/Sfx/");

            Assert.That(condition.MatchesText(LoopPath), Is.True);
        }

        /// <summary>A pattern with no wildcard has to match the whole value, not a part of it.</summary>
        [Test]
        public void APatternWithoutAWildcardMatchesTheWholeValue()
        {
            AudioRuleCondition condition = new(EConditionField.Name, EConditionOperator.Matches, "Theme_loop");

            Assert.That(condition.MatchesText("Theme_loop"), Is.True);
            Assert.That(condition.MatchesText("MainTheme_loop"), Is.False);
        }

        /// <summary>A wildcard stands for any run of characters, including none at all.</summary>
        /// <param name="pattern">The pattern the rule carries.</param>
        /// <param name="expected">Whether it should match the looping theme.</param>
        [TestCase("Theme*", true)]
        [TestCase("*loop", true)]
        [TestCase("Theme*loop", true)]
        [TestCase("*", true)]
        [TestCase("Sfx*", false)]
        [TestCase("*wav", false)]
        public void AWildcardStandsForAnyRunOfCharacters(string pattern, bool expected)
        {
            AudioRuleCondition condition = new(EConditionField.Name, EConditionOperator.Matches, pattern);

            Assert.That(condition.MatchesText("Theme_loop"), Is.EqualTo(expected));
        }

        /// <summary>
        /// The rest of a pattern is taken literally. A dot is a character in a file name, not the
        /// regular expression one that would match anything.
        /// </summary>
        [Test]
        public void ThePatternIsOtherwiseTakenLiterally()
        {
            AudioRuleCondition condition = new(EConditionField.Name, EConditionOperator.Matches, "Theme.*");

            Assert.That(condition.MatchesText("Theme.wav"), Is.True);
            Assert.That(condition.MatchesText("ThemeXwav"), Is.False);
        }

        /// <summary>A flag condition reads its expected value from the number, on or off.</summary>
        /// <param name="number">The value stored on the condition.</param>
        /// <param name="looping">Whether the clip loops.</param>
        /// <param name="expected">Whether the condition should hold.</param>
        [TestCase(1f, true, true)]
        [TestCase(1f, false, false)]
        [TestCase(0f, false, true)]
        [TestCase(0f, true, false)]
        public void AFlagConditionReadsItsValueFromTheNumber(float number, bool looping, bool expected)
        {
            AudioRuleCondition condition = new(EConditionField.IsLooping, EConditionOperator.Equals, number);

            Assert.That(condition.MatchesFlag(looping), Is.EqualTo(expected));
        }

        /// <summary>An operator that makes no sense for a flag holds for nothing rather than everything.</summary>
        [Test]
        public void AnOperatorAFlagCannotUseMatchesNothing()
        {
            AudioRuleCondition condition = new(EConditionField.IsLooping, EConditionOperator.LessThan, 1f);

            Assert.That(condition.MatchesFlag(true), Is.False);
            Assert.That(condition.MatchesFlag(false), Is.False);
        }

        /// <summary>The same holds for text, so a numeric operator on a path never matches.</summary>
        [Test]
        public void AnOperatorTextCannotUseMatchesNothing()
        {
            AudioRuleCondition condition = new(EConditionField.Path, EConditionOperator.LessThan, LoopPath);

            Assert.That(condition.MatchesText(LoopPath), Is.False);
        }

        /// <summary>A value that was never read is treated as an empty one rather than throwing.</summary>
        [Test]
        public void AMissingValueIsTreatedAsEmpty()
        {
            AudioRuleCondition condition = new(EConditionField.Name, EConditionOperator.NotEquals, "Theme");

            Assert.That(condition.MatchesText(null), Is.True);
        }

        /// <summary>The one line form spaces the enum out, which is what the rule list shows.</summary>
        [Test]
        public void TheOneLineFormSpacesTheFieldOut()
        {
            AudioRuleCondition condition = new(EConditionField.DurationSeconds,
                EConditionOperator.LessThan, Threshold);

            Assert.That(condition.ToString(), Does.Contain("duration seconds"));
        }
    }
}