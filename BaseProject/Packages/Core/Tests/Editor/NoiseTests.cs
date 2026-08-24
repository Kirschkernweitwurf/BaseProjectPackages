using Base.CorePackage.Noise;
using NUnit.Framework;
using UnityEngine;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// Covers the two things layered noise is easy to get wrong: the output leaving the range it
    /// promises once octaves and shaping are stacked on, and a seed that does not actually move the
    /// pattern because Perlin noise has no seed of its own.
    /// </summary>
    public sealed class NoiseTests
    {
        private const float Frequency = 0.05f;
        private const int MapHeight = 12;
        private const int MapWidth = 8;
        private const int Octaves = 5;
        private const int OtherSeed = 4321;
        private const int SampleCount = 200;
        private const int Seed = 1234;
        private const float Step = 0.37f;
        private const float Tolerance = 0.0001f;

        /// <summary>Stacked octaves must not push a sample past the amplitude in any shaping mode.</summary>
        [TestCase(ENoiseType.Perlin)]
        [TestCase(ENoiseType.Ridged)]
        [TestCase(ENoiseType.Turbulence)]
        public void SamplesStayInsideTheOutputRange(ENoiseType noiseType)
        {
            NoiseSettings settings = Build(noiseType);

            for (int index = 0; index < SampleCount; index++)
            {
                float position = index * Step;

                Assert.That(settings.Evaluate(position), Is.InRange(-Tolerance, 1f + Tolerance), "one axis");
                Assert.That(settings.Evaluate(new Vector2(position, position)), Is.InRange(-Tolerance, 1f + Tolerance),
                    "plane");
                Assert.That(settings.Evaluate(new Vector3(position, position, position)),
                    Is.InRange(-Tolerance, 1f + Tolerance), "space");
            }
        }

        /// <summary>The same seed has to give the same pattern every time it is built.</summary>
        [Test]
        public void TheSameSeedProducesTheSamePattern()
        {
            NoiseSettings first = Build(ENoiseType.Perlin);
            NoiseSettings second = Build(ENoiseType.Perlin);

            for (int index = 0; index < SampleCount; index++)
            {
                float position = index * Step;

                Assert.That(second.Evaluate(position), Is.EqualTo(first.Evaluate(position)).Within(Tolerance));
            }
        }

        /// <summary>A different seed has to move the pattern, not just relabel it.</summary>
        [Test]
        public void ADifferentSeedProducesADifferentPattern()
        {
            NoiseSettings first = Build(ENoiseType.Perlin);
            NoiseSettings second = new(OtherSeed, Frequency, Octaves);

            int matches = 0;

            for (int index = 0; index < SampleCount; index++)
            {
                float position = index * Step;

                if (Mathf.Abs(first.Evaluate(position) - second.Evaluate(position)) < Tolerance)
                    matches++;
            }

            Assert.That(matches, Is.LessThan(SampleCount / 2), "the two patterns should not line up");
        }

        /// <summary>Changing the seed at runtime has to take effect on the next sample.</summary>
        [Test]
        public void ChangingTheSeedMovesThePattern()
        {
            NoiseSettings settings = Build(ENoiseType.Perlin);
            float[] before = new float[SampleCount];

            for (int index = 0; index < SampleCount; index++)
                before[index] = settings.Evaluate(index * Step);

            settings.SetSeed(OtherSeed);

            int matches = 0;

            for (int index = 0; index < SampleCount; index++)
            {
                if (Mathf.Abs(settings.Evaluate(index * Step) - before[index]) < Tolerance)
                    matches++;
            }

            Assert.That(matches, Is.LessThan(SampleCount / 2), "the offset has to be rebuilt for the new seed");
        }

        /// <summary>A map has to come back at the size that was asked for.</summary>
        [Test]
        public void MapMatchesTheRequestedSize()
        {
            float[,] map = NoiseUtility.CreateMap(MapWidth, MapHeight, Build(ENoiseType.Perlin));

            Assert.That(map.GetLength(0), Is.EqualTo(MapWidth));
            Assert.That(map.GetLength(1), Is.EqualTo(MapHeight));
        }

        /// <summary>An unusable size returns an empty map rather than throwing.</summary>
        [Test]
        public void UnusableSizeReturnsAnEmptyMap()
        {
            float[,] map = NoiseUtility.CreateMap(0, MapHeight, Build(ENoiseType.Perlin));

            Assert.That(map.Length, Is.EqualTo(0));
        }

        private static NoiseSettings Build(ENoiseType noiseType) => new(Seed, Frequency, Octaves, noiseType);
    }
}