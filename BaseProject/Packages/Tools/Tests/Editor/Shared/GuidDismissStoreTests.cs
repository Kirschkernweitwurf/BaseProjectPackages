using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.Shared;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the list of assets a scan was told to leave alone. Three tools share it and the file is
    /// committed, so a dismissal that fails to persist puts a finding back in front of somebody who
    /// already decided about it, and one that fails to lift hides a real finding for good.
    /// </summary>
    public sealed class GuidDismissStoreTests
    {
        private const string First = "11111111111111111111111111111111";
        private const string LoadFailedMessage = "Could not read";
        private const string Second = "22222222222222222222222222222222";

        private string _filePath;

        /// <summary>
        /// A path in the temp folder rather than in the project, so a test never writes into
        /// ProjectSettings. The file itself is not created, since a missing one is a case too.
        /// </summary>
        [SetUp]
        public void Prepare() => _filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        /// <summary>Removes whatever the test wrote, so the next one starts from nothing.</summary>
        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            _filePath = null;
        }

        /// <summary>A project that never dismissed anything has no file, which is not an error.</summary>
        [Test]
        public void AStoreOverAMissingFileIsEmpty()
        {
            GuidDismissStore store = new(_filePath);

            Assert.That(store.Count, Is.EqualTo(0));
            Assert.That(store.IsDismissed(First), Is.False);
        }

        /// <summary>A dismissed entry reports itself as dismissed.</summary>
        [Test]
        public void ADismissedEntryIsRemembered()
        {
            GuidDismissStore store = new(_filePath);
            store.Dismiss(First);

            Assert.That(store.IsDismissed(First), Is.True);
            Assert.That(store.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// The point of writing a file at all: a dismissal outlives the store that made it, and so
        /// survives a rescan, a restart and a branch switch.
        /// </summary>
        [Test]
        public void ADismissalOutlivesTheStoreThatMadeIt()
        {
            new GuidDismissStore(_filePath).Dismiss(First);

            Assert.That(new GuidDismissStore(_filePath).IsDismissed(First), Is.True);
        }

        /// <summary>Dismissing twice is not two dismissals, so the file does not grow duplicates.</summary>
        [Test]
        public void DismissingTheSameEntryTwiceChangesNothing()
        {
            GuidDismissStore store = new(_filePath);
            store.Dismiss(First);
            store.Dismiss(First);

            Assert.That(store.Count, Is.EqualTo(1));
        }

        /// <summary>Restoring puts the entry back in front of the next scan, and that persists too.</summary>
        [Test]
        public void ARestoredEntryComesBack()
        {
            GuidDismissStore store = new(_filePath);
            store.Dismiss(First);
            store.Restore(First);

            Assert.That(store.IsDismissed(First), Is.False);
            Assert.That(new GuidDismissStore(_filePath).IsDismissed(First), Is.False);
        }

        /// <summary>Clearing drops every dismissal at once.</summary>
        [Test]
        public void ClearingDropsEveryDismissal()
        {
            GuidDismissStore store = new(_filePath);
            store.Dismiss(First);
            store.Dismiss(Second);
            store.Clear();

            Assert.That(store.Count, Is.EqualTo(0));
            Assert.That(new GuidDismissStore(_filePath).Count, Is.EqualTo(0));
        }

        /// <summary>Dismissing a whole selection adds every entry rather than only the first.</summary>
        [Test]
        public void ARangeAddsEveryEntry()
        {
            GuidDismissStore store = new(_filePath);
            store.DismissRange(new List<string>
            {
                First,
                Second
            });

            Assert.That(store.Count, Is.EqualTo(2));
        }

        /// <summary>Handing over nothing is a bug in the caller, so it is said out loud.</summary>
        [Test]
        public void ARangeOfNothingIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(GuidDismissStore.DismissRange)));

            GuidDismissStore store = new(_filePath);
            store.DismissRange(null);

            Assert.That(store.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// An empty GUID is not an asset, so it is passed over rather than written into the file where
        /// it would match nothing forever.
        /// </summary>
        [Test]
        public void AnEmptyGuidIsPassedOver()
        {
            GuidDismissStore store = new(_filePath);
            store.Dismiss(null);
            store.Dismiss(string.Empty);

            Assert.That(store.Count, Is.EqualTo(0));
            Assert.That(store.IsDismissed(null), Is.False);
        }

        /// <summary>
        /// The file is committed, so it is written in a stable order. Without that, one added entry
        /// reshuffles the whole list and the diff hides which one actually changed.
        /// </summary>
        [Test]
        public void TheFileIsWrittenInAStableOrder()
        {
            GuidDismissStore store = new(_filePath);
            store.Dismiss(Second);
            store.Dismiss(First);

            string text = File.ReadAllText(_filePath);

            Assert.That(text.IndexOf(First, StringComparison.Ordinal),
                Is.LessThan(text.IndexOf(Second, StringComparison.Ordinal)));
        }

        /// <summary>
        /// A file edited by hand or mangled by a merge must not stop the tool from running, so it is
        /// reported and the store starts empty.
        /// </summary>
        [Test]
        public void AnUnreadableFileIsReportedAndStartsEmpty()
        {
            File.WriteAllText(_filePath, "this is not json");

            LogAssert.Expect(LogType.Warning, new Regex(LoadFailedMessage));

            Assert.That(new GuidDismissStore(_filePath).Count, Is.EqualTo(0));
        }

        /// <summary>
        /// The listing is a copy, which is what lets a window walk it while dismissing or restoring
        /// the rows it is walking.
        /// </summary>
        [Test]
        public void TheListingIsASnapshot()
        {
            GuidDismissStore store = new(_filePath);
            store.Dismiss(First);

            IReadOnlyList<string> listing = store.GetAll();
            store.Dismiss(Second);

            Assert.That(listing, Has.Count.EqualTo(1));
        }
    }
}