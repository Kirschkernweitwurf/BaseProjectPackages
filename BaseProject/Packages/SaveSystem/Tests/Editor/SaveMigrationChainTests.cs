using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.SaveSystemPackage.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers how an old save is walked up to the current version, and the reporting that is supposed
    /// to catch a broken chain at startup rather than on the first old save a player happens to load.
    /// </summary>
    public sealed class SaveMigrationChainTests
    {
        private const int MissingVersion = 1;
        private const string SlotId = "slot_0";
        private const int TargetVersion = 3;

        private List<int> _log;
        private Dictionary<string, string> _states;

        /// <summary>Every test starts with an empty run log and an empty save.</summary>
        [SetUp]
        public void Build()
        {
            _log = new List<int>();
            _states = new Dictionary<string, string>();
        }

        /// <summary>Every step between the stored version and the current one runs, in order.</summary>
        [Test]
        public void EveryStepRunsInOrder()
        {
            SaveMigrationChain chain = Chain(Step(0), Step(1), Step(2));

            Assert.That(chain.TryMigrate(SlotId, _states, 0, TargetVersion), Is.True);
            Assert.That(_log, Is.EqualTo(new[] { 0, 1, 2 }));
        }

        /// <summary>A save already at the current version is left alone.</summary>
        [Test]
        public void ACurrentSaveIsNotTouched()
        {
            SaveMigrationChain chain = Chain(Step(0), Step(1), Step(2));

            Assert.That(chain.TryMigrate(SlotId, _states, TargetVersion, TargetVersion), Is.True);
            Assert.That(_log, Is.Empty);
        }

        /// <summary>Only the steps above the stored version run.</summary>
        [Test]
        public void OnlyTheStepsAboveTheStoredVersionRun()
        {
            SaveMigrationChain chain = Chain(Step(0), Step(1), Step(2));

            Assert.That(chain.TryMigrate(SlotId, _states, 2, TargetVersion), Is.True);
            Assert.That(_log, Is.EqualTo(new[] { 2 }));
        }

        /// <summary>Steps rewrite the save in place, so what they wrote is still there afterwards.</summary>
        [Test]
        public void TheStepsRewriteTheSaveInPlace()
        {
            SaveMigrationChain chain = Chain(Step(0), Step(1), Step(2));

            chain.TryMigrate(SlotId, _states, 0, TargetVersion);

            Assert.That(_states.Count, Is.EqualTo(TargetVersion));
        }

        /// <summary>A gap in the chain stops the upgrade rather than skipping the missing step.</summary>
        [Test]
        public void AMissingStepStopsTheUpgrade()
        {
            SaveMigrationChain chain = Chain(Step(0), Step(2));

            LogAssert.Expect(LogType.Error, new Regex(SlotId));

            Assert.That(chain.TryMigrate(SlotId, _states, 0, TargetVersion), Is.False);
            Assert.That(_log, Is.EqualTo(new[] { 0 }), "the steps before the gap still ran");
        }

        /// <summary>A step that fails is reported and the save is not treated as upgraded.</summary>
        [Test]
        public void AFailingStepIsReported()
        {
            SaveMigrationChain chain = Chain(Step(0), FailingStep(1), Step(2));

            LogAssert.Expect(LogType.Error, new Regex(MigrationProbe.FailureMessage));

            Assert.That(chain.TryMigrate(SlotId, _states, 0, TargetVersion), Is.False);
        }

        /// <summary>
        /// Two steps starting at the same version mean one of them is dead code, so it is reported at
        /// construction and the first one keeps the slot.
        /// </summary>
        [Test]
        public void TwoStepsFromTheSameVersionAreReported()
        {
            List<int> otherLog = new();

            LogAssert.Expect(LogType.Error, new Regex(nameof(MigrationProbe)));

            SaveMigrationChain chain = Chain(Step(0), new MigrationProbe(0, otherLog));

            chain.TryMigrate(SlotId, _states, 0, 1);

            Assert.That(_log, Is.EqualTo(new[] { 0 }));
            Assert.That(otherLog, Is.Empty, "the second step never runs");
        }

        /// <summary>A project with no migrations at all is a normal state.</summary>
        [Test]
        public void AChainWithoutStepsIsFine()
        {
            SaveMigrationChain chain = new(null);

            Assert.That(chain.TryMigrate(SlotId, _states, TargetVersion, TargetVersion), Is.True);
            Assert.DoesNotThrow(() => chain.Validate(TargetVersion));
        }

        /// <summary>Gaps in the list of steps are skipped instead of walked into.</summary>
        [Test]
        public void MissingEntriesInTheListAreSkipped()
        {
            SaveMigrationChain chain = new(new List<ISaveMigration> { null, Step(0), null });

            Assert.That(chain.TryMigrate(SlotId, _states, 0, 1), Is.True);
            Assert.That(_log, Is.EqualTo(new[] { 0 }));
        }

        /// <summary>A complete chain has nothing to report.</summary>
        [Test]
        public void ACompleteChainIsSilent()
        {
            SaveMigrationChain chain = Chain(Step(0), Step(1), Step(2));

            chain.Validate(TargetVersion);

            LogAssert.NoUnexpectedReceived();
        }

        /// <summary>A step that starts at or above the current version can never run.</summary>
        [Test]
        public void AStepThatCanNeverRunIsReported()
        {
            SaveMigrationChain chain = Chain(Step(0), Step(1), Step(2), Step(TargetVersion));

            LogAssert.Expect(LogType.Warning, new Regex($"version {TargetVersion}"));

            chain.Validate(TargetVersion);
        }

        /// <summary>A gap means saves at that version cannot be loaded, so it is reported up front.</summary>
        [Test]
        public void AGapInTheChainIsReported()
        {
            SaveMigrationChain chain = Chain(Step(0), Step(2));

            LogAssert.Expect(LogType.Warning, new Regex($"version {MissingVersion} to"));

            chain.Validate(TargetVersion);
        }

        private static SaveMigrationChain Chain(params ISaveMigration[] steps) => new(steps);

        private MigrationProbe Step(int fromVersion) => new(fromVersion, _log);

        private MigrationProbe FailingStep(int fromVersion) => new(fromVersion, _log, shouldThrow: true);
    }
}