using System;
using System.Collections.Generic;
using Base.TweeningPackage.Core.Data;
using NUnit.Framework;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers the curves every tween is remapped through. Whatever shape a curve has in between, it
    /// has to start at nothing and end at everything, or a tween lands somewhere other than the value
    /// it was told to reach.
    /// </summary>
    /// <remarks>
    /// The per curve cases are driven from the enum itself, so a curve added later is covered without
    /// anyone remembering to list it here, and each one shows up as its own result in the runner.
    /// </remarks>
    public sealed class EasingsTests
    {
        private const float EndpointTolerance = 0.0001f;
        private const int SampleCount = 64;

        /// <summary>Every easing the enum offers. One test case is generated per entry.</summary>
        private static IEnumerable<EEasingType> AllTypes => (EEasingType[])Enum.GetValues(typeof(EEasingType));

        /// <summary>The curves that only ever climb, with no overshoot and no wobble.</summary>
        private static IEnumerable<EEasingType> MonotonicTypes => new[]
        {
            EEasingType.Linear,
            EEasingType.EaseInQuad,
            EEasingType.EaseOutQuad,
            EEasingType.EaseInOutQuad,
            EEasingType.EaseInOut,
            EEasingType.EaseInOutCubic,
            EEasingType.EaseInExpo,
            EEasingType.EaseOutExpo,
            EEasingType.EaseInOutExpo
        };

        /// <summary>The curves that pass their target before settling back onto it.</summary>
        private static IEnumerable<EEasingType> OvershootingTypes => new[]
        {
            EEasingType.EaseOutBack,
            EEasingType.EaseOutElastic
        };

        /// <summary>The curves that ease symmetrically and sit halfway at the midpoint.</summary>
        private static IEnumerable<EEasingType> SymmetricTypes => new[]
        {
            EEasingType.EaseInOut,
            EEasingType.EaseInOutQuad,
            EEasingType.EaseInOutCubic
        };

        /// <summary>Every easing type resolves to a curve, so a tween is never left without one.</summary>
        /// <param name="type">The easing under test.</param>
        [TestCaseSource(nameof(AllTypes))]
        public void EveryTypeResolvesToACurve(EEasingType type) => Assert.That(Easings.Get(type), Is.Not.Null);

        /// <summary>Every curve starts at nothing, so a tween begins at its start value.</summary>
        /// <param name="type">The easing under test.</param>
        [TestCaseSource(nameof(AllTypes))]
        public void EveryCurveStartsAtZero(EEasingType type)
            => Assert.That(Easings.Get(type)(0f), Is.EqualTo(0f).Within(EndpointTolerance));

        /// <summary>Every curve ends at one, so a tween lands exactly on its target value.</summary>
        /// <param name="type">The easing under test.</param>
        [TestCaseSource(nameof(AllTypes))]
        public void EveryCurveEndsAtOne(EEasingType type)
            => Assert.That(Easings.Get(type)(1f), Is.EqualTo(1f).Within(EndpointTolerance));

        /// <summary>Every curve stays a real number in between, so no tween lands on nothing at all.</summary>
        /// <param name="type">The easing under test.</param>
        [TestCaseSource(nameof(AllTypes))]
        public void EveryCurveStaysFinite(EEasingType type) => Assert.That(Sample(type), Is.All.Matches<float>(IsReal));

        /// <summary>A straight line hands its input straight back.</summary>
        [Test]
        public void ALinearCurveIsTheIdentity()
            => Assert.That(Sample(EEasingType.Linear), Is.EqualTo(Inputs()).Within(EndpointTolerance));

        /// <summary>A type this build does not know falls back to a straight line.</summary>
        [Test]
        public void AnUnknownTypeFallsBackToLinear() => Assert.That(Easings.Get((EEasingType)byte.MaxValue)(0.25f),
            Is.EqualTo(0.25f).Within(EndpointTolerance));

        /// <summary>An ease in starts slowly, so it sits below a straight line early on.</summary>
        /// <param name="type">The easing under test.</param>
        [TestCase(EEasingType.EaseInQuad)]
        [TestCase(EEasingType.EaseInExpo)]
        public void AnEaseInStartsSlowly(EEasingType type) => Assert.That(Easings.Get(type)(0.25f), Is.LessThan(0.25f));

        /// <summary>An ease out starts quickly, so it sits above a straight line early on.</summary>
        /// <param name="type">The easing under test.</param>
        [TestCase(EEasingType.EaseOutQuad)]
        [TestCase(EEasingType.EaseOutExpo)]
        public void AnEaseOutStartsQuickly(EEasingType type)
            => Assert.That(Easings.Get(type)(0.25f), Is.GreaterThan(0.25f));

        /// <summary>An ease in and out passes through the middle at the halfway point.</summary>
        /// <param name="type">The easing under test.</param>
        [TestCaseSource(nameof(SymmetricTypes))]
        public void AnEaseInOutIsHalfwayAtTheMiddle(EEasingType type)
            => Assert.That(Easings.Get(type)(0.5f), Is.EqualTo(0.5f).Within(EndpointTolerance));

        /// <summary>The plain curves never go backwards, so a tween cannot appear to reverse.</summary>
        /// <param name="type">The easing under test.</param>
        [TestCaseSource(nameof(MonotonicTypes))]
        public void ThePlainCurvesNeverGoBackwards(EEasingType type)
            => Assert.That(LargestDrop(Sample(type)), Is.LessThanOrEqualTo(EndpointTolerance));

        /// <summary>
        /// The back and elastic curves are supposed to pass their target, which is the whole reason
        /// the interpolation is left unclamped.
        /// </summary>
        /// <param name="type">The easing under test.</param>
        [TestCaseSource(nameof(OvershootingTypes))]
        public void AnOvershootingCurvePassesItsTarget(EEasingType type)
            => Assert.That(Sample(type), Has.Some.GreaterThan(1f));

        /// <summary>The bounce curves are mirror images of each other.</summary>
        [Test]
        public void TheBounceCurvesMirrorEachOther()
        {
            float[] mirrored = Array.ConvertAll(Sample(EEasingType.EaseOutBounce), converter: value => 1f - value);

            Array.Reverse(mirrored);

            Assert.That(Sample(EEasingType.EaseInBounce), Is.EqualTo(mirrored).Within(EndpointTolerance));
        }

        /// <summary>A bounce lands rather than overshoots, so it stays inside its range.</summary>
        [Test]
        public void TheBounceCurveStaysInRange() => Assert.That(Sample(EEasingType.EaseOutBounce),
            Is.All.InRange(-EndpointTolerance, 1f + EndpointTolerance));

        // A fixed sweep across the curve. Sampling into an array lets each test state one property of
        // the whole shape instead of asserting once per point.
        private static float[] Sample(EEasingType type)
        {
            Func<float, float> curve = Easings.Get(type);
            float[] values = new float[SampleCount + 1];

            for (int step = 0; step < values.Length; step++)
                values[step] = curve(step / (float)SampleCount);

            return values;
        }

        private static float[] Inputs()
        {
            float[] values = new float[SampleCount + 1];

            for (int step = 0; step < values.Length; step++)
                values[step] = step / (float)SampleCount;

            return values;
        }

        // The single number that decides whether a curve ever reversed, so the assertion states one
        // fact instead of firing once per sample.
        private static float LargestDrop(float[] values)
        {
            float largest = 0f;

            for (int index = 1; index < values.Length; index++)
                largest = Math.Max(largest, values[index - 1] - values[index]);

            return largest;
        }

        private static bool IsReal(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}