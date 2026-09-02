using System;
using System.Collections.Generic;
using Base.UtilityPackage.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the two halves the dictionary has to keep in step: the runtime lookup and the serialized
    /// entry list behind it. Every write has to reach both, and data authored with duplicate keys has
    /// to arrive as a usable dictionary rather than throwing during deserialization.
    /// </summary>
    public sealed class SerializableDictionaryTests
    {
        private const string Alpha = "Alpha";
        private const string Beta = "Beta";
        private const int FirstValue = 1;
        private const int SecondValue = 2;

        private SerializableDictionary<string, int> _dictionary;

        /// <summary>Every test starts from an empty dictionary.</summary>
        [SetUp]
        public void Build() => _dictionary = new SerializableDictionary<string, int>();

        /// <summary>What was added comes back under the key it was added with.</summary>
        [Test]
        public void AnAddedPairComesBackByKey()
        {
            _dictionary.Add(Alpha, FirstValue);

            Assert.That(_dictionary.ContainsKey(Alpha), Is.True);
            Assert.That(_dictionary[Alpha], Is.EqualTo(FirstValue));
            Assert.That(_dictionary.Count, Is.EqualTo(1));
        }

        /// <summary>Adding a key twice is a mistake, and has to be reported as one.</summary>
        [Test]
        public void AddingTheSameKeyTwiceIsRefused()
        {
            _dictionary.Add(Alpha, FirstValue);

            Assert.Throws<ArgumentException>(() => _dictionary.Add(Alpha, SecondValue));
            Assert.That(_dictionary.Count, Is.EqualTo(1), "the failed add must not leave anything behind");
        }

        /// <summary>The indexer overwrites an existing key instead of adding a second entry.</summary>
        [Test]
        public void TheIndexerOverwritesInsteadOfAdding()
        {
            _dictionary[Alpha] = FirstValue;
            _dictionary[Alpha] = SecondValue;

            Assert.That(_dictionary.Count, Is.EqualTo(1));
            Assert.That(_dictionary[Alpha], Is.EqualTo(SecondValue));
        }

        /// <summary>A removed key is gone from the lookup and from the serialized entries.</summary>
        [Test]
        public void RemoveDropsThePairFromBothHalves()
        {
            _dictionary.Add(Alpha, FirstValue);

            Assert.That(_dictionary.Remove(Alpha), Is.True);
            Assert.That(_dictionary.Count, Is.EqualTo(0));
            Assert.That(ToJson(_dictionary), Does.Not.Contain(Alpha), "the entry list has to shrink as well");
        }

        /// <summary>Removing something that was never there reports it instead of throwing.</summary>
        [Test]
        public void RemovingAMissingKeyReportsFalse()
            => Assert.That(_dictionary.Remove(Alpha), Is.False);

        /// <summary>Clearing has to leave the dictionary as empty as a fresh one.</summary>
        [Test]
        public void ClearEmptiesTheDictionary()
        {
            _dictionary.Add(Alpha, FirstValue);
            _dictionary.Clear();

            Assert.That(_dictionary.Count, Is.EqualTo(0));
            Assert.That(_dictionary.ContainsKey(Alpha), Is.False);
        }

        /// <summary>A pair only counts as contained when the value matches too.</summary>
        [Test]
        public void ContainsMatchesTheValueAsWellAsTheKey()
        {
            _dictionary.Add(Alpha, FirstValue);

            Assert.That(_dictionary.Contains(new KeyValuePair<string, int>(Alpha, FirstValue)), Is.True);
            Assert.That(_dictionary.Contains(new KeyValuePair<string, int>(Alpha, SecondValue)), Is.False);
        }

        /// <summary>Writing to disk and reading back has to hand back the same pairs.</summary>
        [Test]
        public void TheDictionarySurvivesASerializationRoundTrip()
        {
            _dictionary.Add(Alpha, FirstValue);
            _dictionary.Add(Beta, SecondValue);

            SerializableDictionary<string, int> restored = FromJson(ToJson(_dictionary));

            Assert.That(restored.Count, Is.EqualTo(2));
            Assert.That(restored[Alpha], Is.EqualTo(FirstValue));
            Assert.That(restored[Beta], Is.EqualTo(SecondValue));
        }

        /// <summary>
        /// Duplicate keys are an authoring mistake the drawer reports. The runtime dictionary keeps the
        /// first of them and stays usable rather than throwing while the asset loads.
        /// </summary>
        [Test]
        public void DuplicateKeysFromSerializedDataKeepTheFirstOne()
        {
            SerializableDictionary<string, int> restored = FromJson(BuildDuplicateJson());

            Assert.That(restored.Count, Is.EqualTo(1));
            Assert.That(restored[Alpha], Is.EqualTo(FirstValue));
        }

        // The JSON is built from the field name constants the drawers already use, so a rename of the
        // serialized fields breaks the build here instead of silently invalidating the test data.
        private static string BuildDuplicateJson()
        {
            string entries = $"{{\"{SerializableDictionaryEntry<string, int>.KeyField}\":\"{Alpha}\","
                + $"\"{SerializableDictionaryEntry<string, int>.ValueField}\":{FirstValue}}},"
                + $"{{\"{SerializableDictionaryEntry<string, int>.KeyField}\":\"{Alpha}\","
                + $"\"{SerializableDictionaryEntry<string, int>.ValueField}\":{SecondValue}}}";

            return $"{{\"{Host.MapField}\":"
                + $"{{\"{SerializableDictionary<string, int>.EntriesField}\":[{entries}]}}}}";
        }

        private static string ToJson(SerializableDictionary<string, int> dictionary)
        {
            Host host = new()
            {
                Map = dictionary
            };

            return JsonUtility.ToJson(host);
        }

        private static SerializableDictionary<string, int> FromJson(string json)
            => JsonUtility.FromJson<Host>(json).Map;

        // Unity serializes fields, not bare objects, so the dictionary needs something to sit in
        // before it can be written out and read back.
        [Serializable]
        private sealed class Host
        {
            /// <summary>Name of the serialized field, so the test data can name it without a literal.</summary>
            internal const string MapField = nameof(map);

            [SerializeField] private SerializableDictionary<string, int> map = new();

            /// <summary>The dictionary that was written out or read back.</summary>
            public SerializableDictionary<string, int> Map
            {
                get => map;
                set => map = value;
            }
        }
    }
}