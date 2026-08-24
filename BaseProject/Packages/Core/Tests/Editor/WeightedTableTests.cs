using System.Collections.Generic;
using Base.CorePackage.Randomization;
using NUnit.Framework;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// Covers what a weighted draw is supposed to guarantee: weight decides how often an entry
    /// comes up, a weight of zero takes it out entirely, and an empty table reports that instead of
    /// handing back a value that looks like a real draw.
    /// </summary>
    public sealed class WeightedTableTests
    {
        private const string Common = "Common";
        private const int DrawCount = 10000;
        private const float HeavyWeight = 9f;
        private const float LightWeight = 1f;
        private const float LowerShare = 0.85f;
        private const string Rare = "Rare";
        private const int Seed = 1234;
        private const float UpperShare = 0.95f;

        private SeededRandom _random;

        /// <summary>Every test draws from the same seed, so a failure is reproducible.</summary>
        [SetUp]
        public void Build() => _random = new SeededRandom(Seed);

        /// <summary>Nine to one weights have to show up as roughly nine to one draws.</summary>
        [Test]
        public void WeightDecidesHowOftenAnEntryComesUp()
        {
            WeightedTable<string> table = new();

            table.Add(Rare, LightWeight);
            table.Add(Common, HeavyWeight);

            int commonCount = 0;

            for (int index = 0; index < DrawCount; index++)
            {
                if (table.Draw(_random) == Common)
                    commonCount++;
            }

            float share = commonCount / (float)DrawCount;

            Assert.That(share, Is.InRange(LowerShare, UpperShare), "nine of ten draws should be the heavy entry");
        }

        /// <summary>An entry without weight must never be drawn, and must not be counted either.</summary>
        [Test]
        public void EntriesWithoutWeightAreNeverDrawn()
        {
            WeightedTable<string> table = new();

            table.Add(Rare, 0f);
            table.Add(Common, LightWeight);

            Assert.That(table.Count, Is.EqualTo(1), "the weightless entry is dropped, not stored");

            for (int index = 0; index < DrawCount; index++)
                Assert.That(table.Draw(_random), Is.EqualTo(Common), "only the weighted entry can come up");
        }

        /// <summary>The reported total has to match what was put in.</summary>
        [Test]
        public void TotalWeightSumsTheStoredEntries()
        {
            WeightedTable<string> table = new();

            table.Add(Rare, LightWeight);
            table.Add(Common, HeavyWeight);

            Assert.That(table.TotalWeight, Is.EqualTo(LightWeight + HeavyWeight));
        }

        /// <summary>An empty table has nothing to draw and has to say so.</summary>
        [Test]
        public void EmptyTableReportsThatItCannotDraw()
        {
            WeightedTable<string> table = new();

            Assert.That(table.TryDraw(_random, out string _), Is.False, "there is nothing in the table");
        }

        /// <summary>Clearing has to leave the table as empty as a fresh one.</summary>
        [Test]
        public void ClearEmptiesTheTable()
        {
            WeightedTable<string> table = new();

            table.Add(Common, LightWeight);
            table.Clear();

            Assert.That(table.Count, Is.EqualTo(0));
            Assert.That(table.TotalWeight, Is.EqualTo(0f));
        }

        /// <summary>The list based draw has to weight entries the same way the table does.</summary>
        [Test]
        public void DrawingStraightFromAListRespectsWeight()
        {
            List<WeightedEntry<string>> entries = new()
            {
                new WeightedEntry<string>(Rare, LightWeight),
                new WeightedEntry<string>(Common, HeavyWeight)
            };

            int commonCount = 0;

            for (int index = 0; index < DrawCount; index++)
            {
                WeightedTable<string>.TryDrawFrom(entries, _random, out string drawn);

                if (drawn == Common)
                    commonCount++;
            }

            float share = commonCount / (float)DrawCount;

            Assert.That(share, Is.InRange(LowerShare, UpperShare), "the shortcut has to match the table");
        }

        /// <summary>A table built from entries has to hold the ones that carry weight.</summary>
        [Test]
        public void EntriesFromAListAreTakenOver()
        {
            List<WeightedEntry<string>> entries = new()
            {
                new WeightedEntry<string>(Rare, 0f),
                new WeightedEntry<string>(Common, HeavyWeight),
                null
            };

            WeightedTable<string> table = new(entries);

            Assert.That(table.Count, Is.EqualTo(1), "only the weighted entry survives");
            Assert.That(table.Draw(_random), Is.EqualTo(Common));
        }
    }
}