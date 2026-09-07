using System.Collections.Generic;
using Base.ToolsPackage.Editor.CodebaseGraph.Analysis;
using NUnit.Framework;

namespace Base.ToolsPackage.Editor.Tests.CodebaseGraph
{
    /// <summary>
    /// What the dismissal reader says about each line it was given.
    /// <para>
    /// The reason this matters is a report that said two lines were ignored without saying which. A
    /// line that changed nothing because the entry was already set aside is not a problem; a line that
    /// changed nothing because the id was mistyped is, and one count cannot tell them apart.
    /// </para>
    /// </summary>
    public sealed class DismissalTextFormatTests
    {
        private const string OtherId = "type:Probe.Other";
        private const string ProbeId = "member:Probe.Type#Member : Void|DeadMember";

        /// <summary>Leaves the store as it was found, since it is shared editor state.</summary>
        [TearDown]
        public void Cleanup() => DismissalStore.RestoreMany(new[]
        {
            ProbeId,
            OtherId
        });

        /// <summary>A first dismissal of an entry changes the stored set.</summary>
        [Test]
        public void ANewDismissalIsApplied()
            => Assert.That(Only($"dismiss {ProbeId}").Outcome, Is.EqualTo(EDismissalOutcome.Applied));

        /// <summary>
        /// Dismissing the same entry twice is reported as a no-op rather than as a change, so a block
        /// pasted a second time reads as nothing to do instead of as work done.
        /// </summary>
        [Test]
        public void DismissingTwiceSaysThereWasNothingToDo()
        {
            Apply($"dismiss {ProbeId}");

            Assert.That(Only($"dismiss {ProbeId}").Outcome, Is.EqualTo(EDismissalOutcome.AlreadyDismissed));
        }

        /// <summary>Restoring something that was dismissed changes the set.</summary>
        [Test]
        public void RestoringADismissedEntryIsApplied()
        {
            Apply($"dismiss {ProbeId}");

            Assert.That(Only($"restore {ProbeId}").Outcome, Is.EqualTo(EDismissalOutcome.Applied));
        }

        /// <summary>
        /// Restoring something that was never dismissed says so. This is the case that went unreported
        /// and left a stale id looking like a typo.
        /// </summary>
        [Test]
        public void RestoringSomethingUndismissedSaysSo()
            => Assert.That(Only($"restore {ProbeId}").Outcome, Is.EqualTo(EDismissalOutcome.NotDismissed));

        /// <summary>A word that is not one of the four verbs is named as the problem.</summary>
        [Test]
        public void AnUnknownVerbIsNamed()
            => Assert.That(Only($"forget {ProbeId}").Outcome, Is.EqualTo(EDismissalOutcome.UnknownVerb));

        /// <summary>A line with no space at all cannot carry a verb and an id.</summary>
        [Test]
        public void ALineWithoutASpaceIsAnUnknownVerb()
            => Assert.That(Only("dismiss").Outcome, Is.EqualTo(EDismissalOutcome.UnknownVerb));

        /// <summary>
        /// An id with no prefix is reported as the id being wrong rather than the verb, which is what
        /// points the reader at the half of the line that needs fixing.
        /// </summary>
        [Test]
        public void AnIdWithoutAPrefixIsNamedAsTheProblem()
            => Assert.That(Only("dismiss Probe.Type#Member").Outcome,
                Is.EqualTo(EDismissalOutcome.UnreadableId));

        /// <summary>
        /// A bad verb wins over a bad id, because fixing the verb is what makes the rest of the line
        /// worth reading at all.
        /// </summary>
        [Test]
        public void ABadVerbIsReportedBeforeABadId()
            => Assert.That(Only("forget nonsense").Outcome, Is.EqualTo(EDismissalOutcome.UnknownVerb));

        /// <summary>Blank lines and comments are not reported, or a commented block would read as failures.</summary>
        [Test]
        public void CommentsAndBlankLinesAreNotReported()
            => Assert.That(Apply($"# a note\n\n   \ndismiss {ProbeId}"), Has.Count.EqualTo(1));

        /// <summary>Each result carries the line it came from, so the reader can find it again.</summary>
        [Test]
        public void EachResultKnowsItsLineNumber()
        {
            IReadOnlyList<DismissalLineResult> results = Apply($"# a note\ndismiss {ProbeId}\nforget it");

            Assert.That(results[0].Number, Is.EqualTo(2));
            Assert.That(results[1].Number, Is.EqualTo(3));
        }

        /// <summary>And the text of the line, so the message can quote it back.</summary>
        [Test]
        public void EachResultKeepsTheLineText()
            => Assert.That(Only("forget it").Text, Is.EqualTo("forget it"));

        /// <summary>Every failure explains itself, which is the whole point of the outcome.</summary>
        /// <param name="line">A line that changes nothing.</param>
        [TestCase("forget it")]
        [TestCase("dismiss Probe.Type")]
        [TestCase("restore type:Probe.Other")]
        public void EveryFailureExplainsItself(string line)
            => Assert.That(Only(line).Describe(), Is.Not.Empty);

        /// <summary>An applied line has nothing to explain.</summary>
        [Test]
        public void AnAppliedLineExplainsNothing()
            => Assert.That(Only($"dismiss {ProbeId}").Describe(), Is.Empty);

        /// <summary>Reads one line and hands back its result.</summary>
        /// <param name="line">The instruction line.</param>
        /// <returns>What became of it.</returns>
        private static DismissalLineResult Only(string line) => Apply(line)[0];

        /// <summary>Reads instruction lines.</summary>
        /// <param name="text">The lines.</param>
        /// <returns>One result per instruction line.</returns>
        private static IReadOnlyList<DismissalLineResult> Apply(string text)
            => DismissalTextFormat.Apply(text);
    }
}