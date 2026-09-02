using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.UtilityPackage.Randomization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the bounds every draw advertises, the shapes the point helpers claim to fill, and what
    /// happens when a caller hands in nothing at all. A missing source reports itself and returns a
    /// harmless value rather than throwing in the middle of a frame.
    /// </summary>
    /// <remarks>
    /// The sweeps collect their draws into an array first, so each test states one property of the
    /// whole sample rather than firing an assertion per draw. The seed is fixed, so a failure is
    /// reproducible rather than something that happened once.
    /// </remarks>
    public sealed class RandomSourceExtensionsTests
    {
        private const int DrawCount = 1000;
        private const string FirstItem = "Alpha";
        private const int GaussianDrawCount = 5000;
        private const float GaussianTolerance = 0.1f;
        private const float Magnitude = 1f;
        private const int MaxBound = 10;
        private const float MeanValue = 5f;
        private const int MinBound = -5;
        private const string SecondItem = "Beta";
        private const int Seed = 1234;
        private const string ThirdItem = "Gamma";
        private const float Tolerance = 0.001f;

        private SeededRandom _random;

        /// <summary>Every test draws from the same seed, so a failure is reproducible.</summary>
        [SetUp]
        public void Build() => _random = new SeededRandom(Seed);

        /// <summary>A float draw stays inside the unit range it advertises.</summary>
        [Test]
        public void AFloatDrawStaysInTheUnitRange()
            => Assert.That(Draw(() => _random.NextFloat()), Is.All.GreaterThanOrEqualTo(0f).And.LessThan(1f));

        /// <summary>An integer draw stays inside the range, upper bound excluded.</summary>
        [Test]
        public void AnIntegerDrawStaysInTheGivenRange()
            => Assert.That(DrawIntegers(), Is.All.InRange(MinBound, MaxBound - 1));

        /// <summary>A float range stays inside its bounds.</summary>
        [Test]
        public void AFloatRangeStaysInsideItsBounds()
            => Assert.That(Draw(() => _random.Range(MinBound, (float)MaxBound)),
                Is.All.GreaterThanOrEqualTo((float)MinBound).And.LessThan((float)MaxBound));

        /// <summary>A range that holds nothing hands back its lower bound instead of throwing.</summary>
        [Test]
        public void AnEmptyRangeAnswersItsLowerBound()
        {
            Assert.That(_random.Range(MaxBound, MaxBound), Is.EqualTo(MaxBound));
            Assert.That(_random.Range(MaxBound, MinBound), Is.EqualTo(MaxBound));
            Assert.That(_random.Range(1f, 1f), Is.EqualTo(1f));
        }

        /// <summary>A certainty at either end is answered without consulting the source.</summary>
        /// <param name="probability">The chance handed in.</param>
        /// <param name="expected">Whether that chance always hits.</param>
        [TestCase(0f, false)]
        [TestCase(-1f, false)]
        [TestCase(1f, true)]
        [TestCase(2f, true)]
        public void ACertainChanceSkipsTheDraw(float probability, bool expected)
            => Assert.That(_random.Chance(probability), Is.EqualTo(expected));

        /// <summary>A sign is one of two values and never anything else.</summary>
        [Test]
        public void ASignIsEitherUpOrDown()
            => Assert.That(DrawSigns(), Is.All.Matches<int>(IsUpOrDown));

        /// <summary>Both outcomes of a boolean draw have to actually come up.</summary>
        [Test]
        public void ABooleanDrawProducesBothOutcomes()
        {
            List<bool> draws = new();

            for (int index = 0; index < DrawCount; index++)
                draws.Add(_random.NextBool());

            Assert.That(draws, Does.Contain(true));
            Assert.That(draws, Does.Contain(false));
        }

        /// <summary>A normal draw centers on the mean it was given.</summary>
        [Test]
        public void ANormalDrawCentersOnItsMean()
        {
            float total = 0f;

            for (int index = 0; index < GaussianDrawCount; index++)
                total += _random.NextGaussian(MeanValue);

            Assert.That(total / GaussianDrawCount, Is.EqualTo(MeanValue).Within(GaussianTolerance));
        }

        /// <summary>A pick comes out of the list it was handed.</summary>
        [Test]
        public void APickComesFromTheGivenList()
        {
            List<string> items = new() { FirstItem, SecondItem, ThirdItem };
            List<string> picks = new();

            for (int index = 0; index < DrawCount; index++)
                picks.Add(_random.Pick(items));

            Assert.That(picks, Is.All.Matches<string>(items.Contains));
        }

        /// <summary>A shuffle reorders the list without gaining or losing anything.</summary>
        [Test]
        public void AShuffleKeepsEveryItem()
        {
            List<int> items = new() { 1, 2, 3, 4, 5, 6, 7, 8 };

            _random.Shuffle(items);

            Assert.That(items, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
        }

        /// <summary>A shuffle has to actually change the order at some point.</summary>
        [Test]
        public void AShuffleChangesTheOrder()
        {
            List<int> items = new() { 1, 2, 3, 4, 5, 6, 7, 8 };

            _random.Shuffle(items);

            Assert.That(items, Is.Not.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
        }

        /// <summary>A point on the circle sits exactly one unit from the origin.</summary>
        [Test]
        public void APointOnTheCircleHasUnitLength()
            => Assert.That(Draw(() => _random.OnUnitCircle().magnitude),
                Is.All.EqualTo(Magnitude).Within(Tolerance));

        /// <summary>A point inside the circle never leaves it.</summary>
        [Test]
        public void APointInsideTheCircleStaysInside()
            => Assert.That(Draw(() => _random.InsideUnitCircle().magnitude),
                Is.All.LessThanOrEqualTo(Magnitude + Tolerance));

        /// <summary>A point on the sphere sits exactly one unit from the origin.</summary>
        [Test]
        public void APointOnTheSphereHasUnitLength()
            => Assert.That(Draw(() => _random.OnUnitSphere().magnitude),
                Is.All.EqualTo(Magnitude).Within(Tolerance));

        /// <summary>A point inside the sphere never leaves it.</summary>
        [Test]
        public void APointInsideTheSphereStaysInside()
            => Assert.That(Draw(() => _random.InsideUnitSphere().magnitude),
                Is.All.LessThanOrEqualTo(Magnitude + Tolerance));

        /// <summary>A missing source reports itself and hands back a harmless value.</summary>
        [Test]
        public void AMissingSourceIsReported()
        {
            IRandomSource source = null;

            LogAssert.Expect(LogType.Error, new Regex(nameof(RandomSourceExtensions.NextFloat)));

            Assert.That(source.NextFloat(), Is.EqualTo(0f));
        }

        /// <summary>A missing source falls back to the lower bound of the range.</summary>
        [Test]
        public void AMissingSourceAnswersTheLowerBound()
        {
            IRandomSource source = null;

            LogAssert.Expect(LogType.Error, new Regex(nameof(RandomSourceExtensions.Range)));

            Assert.That(source.Range(MinBound, MaxBound), Is.EqualTo(MinBound));
        }

        /// <summary>A missing list is reported rather than picked from.</summary>
        [Test]
        public void AMissingListIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex($"{nameof(RandomSourceExtensions.Pick)}.*null"));

            Assert.That(_random.Pick<string>(null), Is.Null);
        }

        /// <summary>An empty list has nothing to pick from and is reported.</summary>
        [Test]
        public void AnEmptyListIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex($"{nameof(RandomSourceExtensions.Pick)}.*empty"));

            Assert.That(_random.Pick(new List<string>()), Is.Null);
        }

        private static bool IsUpOrDown(int sign) => sign == 1 || sign == -1;

        // Collects a sweep of draws so a test can state one property of the whole sample.
        private static float[] Draw(Func<float> draw)
        {
            float[] values = new float[DrawCount];

            for (int index = 0; index < DrawCount; index++)
                values[index] = draw();

            return values;
        }

        private int[] DrawIntegers()
        {
            int[] values = new int[DrawCount];

            for (int index = 0; index < DrawCount; index++)
                values[index] = _random.Range(MinBound, MaxBound);

            return values;
        }

        private int[] DrawSigns()
        {
            int[] values = new int[DrawCount];

            for (int index = 0; index < DrawCount; index++)
                values[index] = _random.NextSign();

            return values;
        }
    }
}