using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers how duplicates are found in a list. Both the rule and the inspector handler read this,
    /// so a difference between them would mean the overview window and the field itself disagree about
    /// the same asset.
    /// </summary>
    public sealed class DuplicateFinderTests
    {
        private readonly List<Object> _created = new();

        private List<string> _groups;
        private List<int> _indices;

        /// <summary>Every test starts with empty result buffers.</summary>
        [SetUp]
        public void Build()
        {
            _groups = new List<string>();
            _indices = new List<int>();
        }

        /// <summary>Objects built in a test are destroyed here.</summary>
        [TearDown]
        public void Release()
        {
            foreach (Object created in _created)
            {
                if (created != null)
                    Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        /// <summary>A list where everything differs has no groups.</summary>
        [Test]
        public void AListWithoutRepeatsHasNoGroups()
        {
            DuplicateFinder.Collect(new List<string> { "Alpha", "Beta" }, _groups);

            Assert.That(_groups, Is.Empty);
        }

        /// <summary>A repeated value forms one group naming every index it sits at.</summary>
        [Test]
        public void ARepeatedValueFormsOneGroup()
        {
            DuplicateFinder.Collect(new List<string> { "Alpha", "Beta", "Alpha" }, _groups);

            Assert.That(_groups, Is.EqualTo(new[] { "0, 2" }));
        }

        /// <summary>A value repeated three times still forms a single group.</summary>
        [Test]
        public void AValueRepeatedThreeTimesStaysOneGroup()
        {
            DuplicateFinder.Collect(new List<string> { "Alpha", "Beta", "Alpha", "Alpha" }, _groups);

            Assert.That(_groups, Is.EqualTo(new[] { "0, 2, 3" }));
        }

        /// <summary>Two different repeated values form two groups.</summary>
        [Test]
        public void TwoRepeatedValuesFormTwoGroups()
        {
            DuplicateFinder.Collect(new List<string> { "Alpha", "Beta", "Alpha", "Beta" }, _groups);

            Assert.That(_groups, Is.EqualTo(new[] { "0, 2", "1, 3" }));
        }

        /// <summary>
        /// Empty entries do not count as repeats, so a list that is still being filled stays quiet
        /// instead of flagging every freshly added slot.
        /// </summary>
        [Test]
        public void EmptyEntriesAreNotRepeats()
        {
            DuplicateFinder.Collect(new List<string> { null, string.Empty, null, string.Empty }, _groups);

            Assert.That(_groups, Is.Empty);
        }

        /// <summary>Empty entries do not break the indices reported around them.</summary>
        [Test]
        public void EmptyEntriesDoNotShiftTheIndices()
        {
            DuplicateFinder.Collect(new List<string> { null, "Alpha", null, "Alpha" }, _groups);

            Assert.That(_groups, Is.EqualTo(new[] { "1, 3" }));
        }

        /// <summary>Nothing to look at leaves the result empty rather than throwing.</summary>
        [Test]
        public void NothingToLookAtLeavesNoGroups()
        {
            DuplicateFinder.Collect(null, _groups);

            Assert.That(_groups, Is.Empty);
        }

        /// <summary>The result buffer is cleared, so a previous run cannot leak into this one.</summary>
        [Test]
        public void ThePreviousResultIsCleared()
        {
            _groups.Add("stale");

            DuplicateFinder.Collect(new List<string> { "Alpha" }, _groups);

            Assert.That(_groups, Is.Empty);
        }

        /// <summary>Object references count as the same entry only when they are the same object.</summary>
        [Test]
        public void ObjectsRepeatOnlyWhenTheyAreTheSameObject()
        {
            GameObject shared = Created();

            DuplicateFinder.Collect(new List<GameObject> { shared, Created(), shared }, _groups);

            Assert.That(_groups, Is.EqualTo(new[] { "0, 2" }));
        }

        /// <summary>
        /// A destroyed object counts as empty rather than as a repeat, since Unity reports it as null
        /// and two of them are not the same entry.
        /// </summary>
        [Test]
        public void DestroyedObjectsAreNotRepeats()
        {
            GameObject first = Created();
            GameObject second = Created();

            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);

            DuplicateFinder.Collect(new List<GameObject> { first, second }, _groups);

            Assert.That(_groups, Is.Empty);
        }

        /// <summary>The repeats are every index that echoes an earlier entry, first ones excluded.</summary>
        [Test]
        public void TheRepeatsExcludeTheFirstOccurrence()
        {
            DuplicateFinder.CollectRepeats(new List<string> { "Alpha", "Beta", "Alpha", "Alpha" }, _indices);

            Assert.That(_indices, Is.EqualTo(new[] { 2, 3 }));
        }

        /// <summary>Removing the reported repeats leaves one entry per value.</summary>
        [Test]
        public void RemovingTheRepeatsLeavesOneEntryPerValue()
        {
            List<string> entries = new() { "Alpha", "Beta", "Alpha", "Beta", "Alpha" };

            DuplicateFinder.CollectRepeats(entries, _indices);

            for (int i = _indices.Count - 1; i >= 0; i--)
                entries.RemoveAt(_indices[i]);

            Assert.That(entries, Is.EqualTo(new[] { "Alpha", "Beta" }));
        }

        /// <summary>A list without repeats reports no indices.</summary>
        [Test]
        public void AListWithoutRepeatsReportsNoIndices()
        {
            DuplicateFinder.CollectRepeats(new List<string> { "Alpha", "Beta" }, _indices);

            Assert.That(_indices, Is.Empty);
        }

        /// <summary>The message names the indices a group sits at.</summary>
        [Test]
        public void TheMessageNamesTheIndices()
        {
            string message = DuplicateFinder.Describe("0, 2");

            Assert.That(message, Does.Contain("0, 2"));
        }

        /// <summary>The combined message covers every group.</summary>
        [Test]
        public void TheCombinedMessageCoversEveryGroup()
        {
            string message = DuplicateFinder.Describe(new[] { "0, 2", "1, 3" });

            Assert.That(message, Does.Contain("0, 2"));
            Assert.That(message, Does.Contain("1, 3"));
        }

        private GameObject Created()
        {
            GameObject created = new(nameof(DuplicateFinderTests));

            _created.Add(created);

            return created;
        }
    }
}