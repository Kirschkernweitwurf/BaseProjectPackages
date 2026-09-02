using Base.ToolsPackage.Editor.NamingConventions.Data;
using Base.ToolsPackage.Editor.NamingConventions.Scanning;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers how a name's casing is recognized and rewritten. A wrong answer here either reports an
    /// asset that was fine or suggests a rename that changes nothing, and both make the whole tool
    /// look untrustworthy.
    /// </summary>
    public sealed class NameStyleUtilityTests
    {
        private const string MixedName = "kitchen_lamp";

        /// <summary>Pascal case is one word starting upper, with no separators.</summary>
        [Test]
        public void PascalCaseIsRecognized()
        {
            Assert.That(NameStyleUtility.Matches("MyName", ENamingStyle.PascalCase), Is.True);
            Assert.That(NameStyleUtility.Matches("myName", ENamingStyle.PascalCase), Is.False);
            Assert.That(NameStyleUtility.Matches("My_Name", ENamingStyle.PascalCase), Is.False);
        }

        /// <summary>Camel case is the same, starting lower.</summary>
        [Test]
        public void CamelCaseIsRecognized()
        {
            Assert.That(NameStyleUtility.Matches("myName", ENamingStyle.CamelCase), Is.True);
            Assert.That(NameStyleUtility.Matches("MyName", ENamingStyle.CamelCase), Is.False);
        }

        /// <summary>Upper snake case is all caps with underscores between.</summary>
        [Test]
        public void UpperSnakeCaseIsRecognized()
        {
            Assert.That(NameStyleUtility.Matches("MY_NAME", ENamingStyle.UpperSnakeCase), Is.True);
            Assert.That(NameStyleUtility.Matches("My_Name", ENamingStyle.UpperSnakeCase), Is.False);
        }

        /// <summary>Lower snake case is all lower with underscores between.</summary>
        [Test]
        public void LowerSnakeCaseIsRecognized()
        {
            Assert.That(NameStyleUtility.Matches("my_name", ENamingStyle.LowerSnakeCase), Is.True);
            Assert.That(NameStyleUtility.Matches("My_name", ENamingStyle.LowerSnakeCase), Is.False);
        }

        /// <summary>
        /// Pascal snake case keeps the underscores, so a category stays separate from the asset rather
        /// than collapsing into one word.
        /// </summary>
        [Test]
        public void PascalSnakeCaseKeepsItsSegments()
        {
            Assert.That(NameStyleUtility.Matches("Kitchen_Lamp", ENamingStyle.PascalSnakeCase), Is.True);
            Assert.That(NameStyleUtility.Matches("kitchen_lamp", ENamingStyle.PascalSnakeCase), Is.False);
        }

        /// <summary>
        /// A plain number counts as a segment, so a variant like a numbered counter is accepted instead
        /// of being reported with a fix that changes nothing.
        /// </summary>
        [Test]
        public void ANumberCountsAsASegment()
            => Assert.That(NameStyleUtility.Matches("Counter_01_MS", ENamingStyle.PascalSnakeCase), Is.True);

        /// <summary>The open style accepts anything that is a name at all.</summary>
        [Test]
        public void TheOpenStyleAcceptsAnything()
        {
            Assert.That(NameStyleUtility.Matches("whatever", ENamingStyle.Any), Is.True);
            Assert.That(NameStyleUtility.Matches("what ever", ENamingStyle.Any), Is.True);
        }

        /// <summary>No name at all matches nothing, not even the open style.</summary>
        [Test]
        public void NoNameMatchesNothing()
        {
            Assert.That(NameStyleUtility.Matches(null, ENamingStyle.Any), Is.False);
            Assert.That(NameStyleUtility.Matches(string.Empty, ENamingStyle.PascalCase), Is.False);
        }

        /// <summary>A name without separators is read as pascal or camel case.</summary>
        [Test]
        public void ANameWithoutSeparatorsIsPascalOrCamel()
        {
            Assert.That(NameStyleUtility.Detect("MyName"), Is.EqualTo(ENamingStyle.PascalCase));
            Assert.That(NameStyleUtility.Detect("myName"), Is.EqualTo(ENamingStyle.CamelCase));
        }

        /// <summary>The all upper and all lower styles win over the mixed one.</summary>
        [Test]
        public void TheUniformSnakeStylesWinOverTheMixedOne()
        {
            Assert.That(NameStyleUtility.Detect("MY_NAME"), Is.EqualTo(ENamingStyle.UpperSnakeCase));
            Assert.That(NameStyleUtility.Detect("my_name"), Is.EqualTo(ENamingStyle.LowerSnakeCase));
            Assert.That(NameStyleUtility.Detect("My_Name"), Is.EqualTo(ENamingStyle.PascalSnakeCase));
        }

        /// <summary>A name that fits nothing is reported as having no style rather than a wrong one.</summary>
        [Test]
        public void ANameThatFitsNothingHasNoStyle()
        {
            Assert.That(NameStyleUtility.Detect("my name"), Is.EqualTo(ENamingStyle.Any));
            Assert.That(NameStyleUtility.Detect(null), Is.EqualTo(ENamingStyle.Any));
            Assert.That(NameStyleUtility.Detect(string.Empty), Is.EqualTo(ENamingStyle.Any));
        }

        /// <summary>What was detected is what the name matches.</summary>
        /// <param name="name">The name under test.</param>
        [TestCase("MyName")]
        [TestCase("myName")]
        [TestCase("MY_NAME")]
        [TestCase("my_name")]
        [TestCase("My_Name")]
        public void WhatIsDetectedIsWhatMatches(string name)
            => Assert.That(NameStyleUtility.Matches(name, NameStyleUtility.Detect(name)), Is.True);

        /// <summary>Spaces and dashes are word breaks when a name is rewritten.</summary>
        [Test]
        public void SpacesAndDashesAreWordBreaks()
        {
            Assert.That(NameStyleUtility.Convert("my name", ENamingStyle.PascalCase), Is.EqualTo("MyName"));
            Assert.That(NameStyleUtility.Convert("my-name", ENamingStyle.UpperSnakeCase), Is.EqualTo("MY_NAME"));
        }

        /// <summary>A rewritten name comes out in the style it was asked for.</summary>
        [Test]
        public void ARewrittenNameMatchesTheStyleItWasAskedFor()
        {
            Assert.That(NameStyleUtility.Convert("MyName", ENamingStyle.LowerSnakeCase), Is.EqualTo("my_name"));
            Assert.That(NameStyleUtility.Convert("my_name", ENamingStyle.PascalCase), Is.EqualTo("MyName"));
            Assert.That(NameStyleUtility.Convert("MyName", ENamingStyle.CamelCase), Is.EqualTo("myName"));
            Assert.That(NameStyleUtility.Convert("my_name", ENamingStyle.UpperSnakeCase), Is.EqualTo("MY_NAME"));
        }

        /// <summary>
        /// A run of capitals is one word, so an acronym does not turn into one word per letter.
        /// </summary>
        [Test]
        public void ARunOfCapitalsStaysOneWord()
            => Assert.That(NameStyleUtility.Convert("HTTPServer", ENamingStyle.LowerSnakeCase),
                Is.EqualTo("http_server"));

        /// <summary>Rewriting into the mixed snake style rewrites each segment on its own.</summary>
        [Test]
        public void TheMixedSnakeStyleRewritesEachSegment()
        {
            Assert.That(NameStyleUtility.Convert("kitchen_lamp", ENamingStyle.PascalSnakeCase),
                Is.EqualTo("Kitchen_Lamp"));

            Assert.That(NameStyleUtility.Convert("KITCHEN_LAMP", ENamingStyle.PascalSnakeCase),
                Is.EqualTo("Kitchen_Lamp"));
        }

        /// <summary>A name with nothing to split comes back as it was rather than as an empty string.</summary>
        [Test]
        public void ANameWithNothingToSplitComesBackUnchanged()
        {
            Assert.That(NameStyleUtility.Convert(string.Empty, ENamingStyle.PascalCase), Is.Empty);
            Assert.That(NameStyleUtility.Convert("___", ENamingStyle.PascalSnakeCase), Is.EqualTo("___"));
        }

        /// <summary>A name rewritten into pascal case satisfies it.</summary>
        [Test]
        public void APascalRewriteSatisfiesItsStyle() => AssertRewriteSatisfies(ENamingStyle.PascalCase);

        /// <summary>A name rewritten into camel case satisfies it.</summary>
        [Test]
        public void ACamelRewriteSatisfiesItsStyle() => AssertRewriteSatisfies(ENamingStyle.CamelCase);

        /// <summary>A name rewritten into upper snake case satisfies it.</summary>
        [Test]
        public void AnUpperSnakeRewriteSatisfiesItsStyle() => AssertRewriteSatisfies(ENamingStyle.UpperSnakeCase);

        /// <summary>A name rewritten into lower snake case satisfies it.</summary>
        [Test]
        public void ALowerSnakeRewriteSatisfiesItsStyle() => AssertRewriteSatisfies(ENamingStyle.LowerSnakeCase);

        /// <summary>A name rewritten into the mixed snake case satisfies it.</summary>
        [Test]
        public void APascalSnakeRewriteSatisfiesItsStyle() => AssertRewriteSatisfies(ENamingStyle.PascalSnakeCase);

        // The style is internal, so it cannot sit in a public test signature and a test case per
        // style would not compile. One named test each calls through this instead, which gives the
        // same per style result in the runner.
        private static void AssertRewriteSatisfies(ENamingStyle style)
        {
            string converted = NameStyleUtility.Convert(MixedName, style);

            Assert.That(NameStyleUtility.Matches(converted, style), Is.True, converted);
        }
    }
}