using System;
using System.Collections.Generic;
using Base.UtilityPackage.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the two halves the set has to keep in step: the runtime set and the serialized item list
    /// behind it. Bulk operations have to rewrite the list, and duplicates authored in the inspector
    /// must not leave an item that can never be added again.
    /// </summary>
    public sealed class SerializableHashSetTests
    {
        private const string Alpha = "Alpha";
        private const string Beta = "Beta";
        private const string Gamma = "Gamma";

        private SerializableHashSet<string> _set;

        /// <summary>Every test starts from an empty set.</summary>
        [SetUp]
        public void Build() => _set = new SerializableHashSet<string>();

        /// <summary>What was added is contained afterwards.</summary>
        [Test]
        public void AnAddedItemIsContained()
        {
            Assert.That(_set.Add(Alpha), Is.True);
            Assert.That(_set.Contains(Alpha), Is.True);
            Assert.That(_set.Count, Is.EqualTo(1));
        }

        /// <summary>A second add of the same item changes nothing and says so.</summary>
        [Test]
        public void AddingTheSameItemTwiceReportsFalse()
        {
            _set.Add(Alpha);

            Assert.That(_set.Add(Alpha), Is.False);
            Assert.That(_set.Count, Is.EqualTo(1));
        }

        /// <summary>A removed item is gone from the set and from the serialized items.</summary>
        [Test]
        public void RemoveDropsTheItemFromBothHalves()
        {
            _set.Add(Alpha);

            Assert.That(_set.Remove(Alpha), Is.True);
            Assert.That(_set.Count, Is.EqualTo(0));
            Assert.That(ToJson(_set), Does.Not.Contain(Alpha), "the item list has to shrink as well");
        }

        /// <summary>Removing something that was never there reports it instead of throwing.</summary>
        [Test]
        public void RemovingAMissingItemReportsFalse()
            => Assert.That(_set.Remove(Alpha), Is.False);

        /// <summary>Clearing has to leave the set as empty as a fresh one.</summary>
        [Test]
        public void ClearEmptiesTheSet()
        {
            _set.Add(Alpha);
            _set.Clear();

            Assert.That(_set.Count, Is.EqualTo(0));
            Assert.That(_set.Contains(Alpha), Is.False);
        }

        /// <summary>A union adds what is missing and skips what is already there.</summary>
        [Test]
        public void UnionAddsOnlyWhatIsMissing()
        {
            _set.Add(Alpha);
            _set.UnionWith(new List<string> { Alpha, Beta });

            Assert.That(_set.Count, Is.EqualTo(2));
            Assert.That(_set.SetEquals(new List<string> { Alpha, Beta }), Is.True);
        }

        /// <summary>An intersection keeps only what both sides hold.</summary>
        [Test]
        public void IntersectKeepsOnlyTheSharedItems()
        {
            _set.UnionWith(new List<string> { Alpha, Beta, Gamma });
            _set.IntersectWith(new List<string> { Beta, Gamma });

            Assert.That(_set.SetEquals(new List<string> { Beta, Gamma }), Is.True);
        }

        /// <summary>An exception removes exactly the given items.</summary>
        [Test]
        public void ExceptRemovesTheGivenItems()
        {
            _set.UnionWith(new List<string> { Alpha, Beta });
            _set.ExceptWith(new List<string> { Alpha });

            Assert.That(_set.SetEquals(new List<string> { Beta }), Is.True);
        }

        /// <summary>A bulk operation has to reach the serialized items, not only the runtime set.</summary>
        [Test]
        public void ABulkOperationRewritesTheSerializedItems()
        {
            _set.UnionWith(new List<string> { Alpha, Beta });
            _set.ExceptWith(new List<string> { Alpha });

            Assert.That(ToJson(_set), Does.Not.Contain(Alpha));
            Assert.That(ToJson(_set), Does.Contain(Beta));
        }

        /// <summary>Writing to disk and reading back has to hand back the same items.</summary>
        [Test]
        public void TheSetSurvivesASerializationRoundTrip()
        {
            _set.UnionWith(new List<string> { Alpha, Beta });

            SerializableHashSet<string> restored = FromJson(ToJson(_set));

            Assert.That(restored.SetEquals(new List<string> { Alpha, Beta }), Is.True);
        }

        /// <summary>Duplicates authored in the inspector collapse into a single item.</summary>
        [Test]
        public void DuplicatesFromSerializedDataCollapseIntoOne()
        {
            SerializableHashSet<string> restored = FromJson(BuildDuplicateJson());

            Assert.That(restored.Count, Is.EqualTo(1));
            Assert.That(restored.Contains(Alpha), Is.True);
        }

        /// <summary>
        /// Removing an item that was authored twice has to clear both occurrences, otherwise the item
        /// stays in the serialized list and can never be added back.
        /// </summary>
        [Test]
        public void RemovingAnItemAlsoClearsItsDuplicates()
        {
            SerializableHashSet<string> restored = FromJson(BuildDuplicateJson());

            restored.Remove(Alpha);

            Assert.That(ToJson(restored), Does.Not.Contain(Alpha));
            Assert.That(restored.Add(Alpha), Is.True, "the item has to be addable again");
        }

        // The JSON is built from the field name constant the drawer already uses, so a rename of the
        // serialized field breaks the build here instead of silently invalidating the test data.
        private static string BuildDuplicateJson()
            => $"{{\"{Host.ItemsField}\":"
                + $"{{\"{SerializableHashSet<string>.ItemsField}\":[\"{Alpha}\",\"{Alpha}\"]}}}}";

        private static string ToJson(SerializableHashSet<string> set)
        {
            Host host = new()
            {
                Items = set
            };

            return JsonUtility.ToJson(host);
        }

        private static SerializableHashSet<string> FromJson(string json) => JsonUtility.FromJson<Host>(json).Items;

        // Unity serializes fields, not bare objects, so the set needs something to sit in before it can
        // be written out and read back.
        [Serializable]
        private sealed class Host
        {
            /// <summary>Name of the serialized field, so the test data can name it without a literal.</summary>
            internal const string ItemsField = nameof(items);

            [SerializeField] private SerializableHashSet<string> items = new();

            /// <summary>The set that was written out or read back.</summary>
            public SerializableHashSet<string> Items
            {
                get => items;
                set => items = value;
            }
        }
    }
}