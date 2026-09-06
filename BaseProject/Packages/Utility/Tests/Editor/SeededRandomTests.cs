using System.Collections.Generic;
using Base.UtilityPackage.Randomization;
using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the promise the whole seeded generator exists for: the same seed replays the same
    /// run, and every helper built on top of it stays inside the range it advertises.
    /// </summary>
    public sealed class SeededRandomTests
    {
        private const int DrawCount = 1000;
        private const int MaxBound = 10;
        private const int MinBound = -5;
        private const int OtherSeed = 4321;
        private const int Seed = 1234;

        /// <summary>Two generators on one seed have to walk the same sequence.</summary>
        [Test]
        public void SameSeedProducesTheSameSequence()
        {
            SeededRandom first = new(Seed);
            SeededRandom second = new(Seed);

            for (int index = 0; index < DrawCount; index++)
                Assert.That(second.NextUInt(), Is.EqualTo(first.NextUInt()), "a seed has to replay exactly");
        }

        /// <summary>Neighboring seeds must not start out in neighboring parts of the sequence.</summary>
        [Test]
        public void NeighboringSeedsProduceDifferentSequences()
        {
            SeededRandom first = new(Seed);
            SeededRandom second = new(Seed + 1);

            int matches = 0;

            for (int index = 0; index < DrawCount; index++)
            {
                if (first.NextUInt() == second.NextUInt())
                    matches++;
            }

            Assert.That(matches, Is.LessThan(DrawCount / 2), "the two streams should not track each other");
        }

        /// <summary>A separate stream decorrelates two generators that share a seed.</summary>
        [Test]
        public void SeparateStreamsProduceDifferentSequences()
        {
            SeededRandom first = new(Seed, 0);
            SeededRandom second = new(Seed, 1);

            Assert.That(second.NextUInt(), Is.Not.EqualTo(first.NextUInt()), "streams should not overlap");
        }

        /// <summary>Resetting has to hand back the same draws the generator started with.</summary>
        [Test]
        public void ResetReplaysTheSequence()
        {
            SeededRandom random = new(Seed);

            uint first = random.NextUInt();

            random.NextUInt();
            random.Reset();

            Assert.That(random.NextUInt(), Is.EqualTo(first), "a reset generator is a fresh one");
        }

        /// <summary>A captured state has to continue where it was taken, not where the seed starts.</summary>
        [Test]
        public void RestoreContinuesFromTheCapturedState()
        {
            SeededRandom random = new(Seed);

            random.NextUInt();

            ulong captured = random.State;
            uint expected = random.NextUInt();

            random.Restore(captured);

            Assert.That(random.NextUInt(), Is.EqualTo(expected), "the run should pick up where it was saved");
        }

        /// <summary>The integer range has to include its lower bound and stay below its upper one.</summary>
        [Test]
        public void IntegerRangeStaysInsideItsBounds()
        {
            SeededRandom random = new(Seed);

            for (int index = 0; index < DrawCount; index++)
            {
                int value = random.Range(MinBound, MaxBound);

                Assert.That(value, Is.InRange(MinBound, MaxBound - 1), "the upper bound is excluded");
            }
        }

        /// <summary>An empty range has nothing to draw from and returns its own bound.</summary>
        [Test]
        public void EmptyRangeReturnsTheLowerBound()
        {
            SeededRandom random = new(Seed);

            Assert.That(random.Range(MaxBound, MaxBound), Is.EqualTo(MaxBound), "there is nothing to pick");
        }

        /// <summary>A certain and an impossible chance must not depend on a draw at all.</summary>
        [Test]
        public void CertainAndImpossibleChancesAreDecidedWithoutDrawing()
        {
            SeededRandom random = new(Seed);

            Assert.That(random.Chance(0f), Is.False, "zero can never hit");
            Assert.That(random.Chance(1f), Is.True, "one can never miss");
        }

        /// <summary>Shuffling reorders a list without losing or duplicating anything in it.</summary>
        [Test]
        public void ShuffleKeepsEveryElement()
        {
            SeededRandom random = new(Seed);
            List<int> items = new();

            for (int index = 0; index < MaxBound; index++)
                items.Add(index);

            random.Shuffle(items);

            items.Sort();

            for (int index = 0; index < MaxBound; index++)
                Assert.That(items[index], Is.EqualTo(index), "a shuffle only moves elements");
        }

        /// <summary>A seed built at runtime has to differ from the next one, or replays collide.</summary>
        [Test]
        public void CreatedSeedsDiffer()
            => Assert.That(SeededRandom.CreateSeed(), Is.Not.EqualTo(SeededRandom.CreateSeed()));

        /// <summary>Two generators on unrelated seeds must not agree from the first draw on.</summary>
        [Test]
        public void UnrelatedSeedsStartApart()
        {
            SeededRandom first = new(Seed);
            SeededRandom second = new(OtherSeed);

            Assert.That(second.NextUInt(), Is.Not.EqualTo(first.NextUInt()));
        }
    }
}