using Base.ControllerSupportPackage.Haptics;
using NUnit.Framework;
using UnityEngine;

namespace Base.ControllerSupportPackage.Tests
{
    /// <summary>
    /// Covers a single live playback. Its own intensity is applied on top of the pattern, and a
    /// looping playback never reports itself as finished, which is what keeps it alive until something
    /// stops it explicitly.
    /// </summary>
    /// <remarks>
    /// The clock is driven by Unity's frame time, so advancing it is not covered here. What a request
    /// answers before any time has passed is.
    /// </remarks>
    public sealed class RumbleRequestTests
    {
        private const float CurveEnd = 1f;
        private const float CurveStart = 0f;
        private const float FullIntensity = 1f;
        private const float Tolerance = 0.0001f;

        private object _caller;

        /// <summary>Every test uses its own caller object.</summary>
        [SetUp]
        public void Build() => _caller = new object();

        /// <summary>A request holds on to what it was asked to play and who asked for it.</summary>
        [Test]
        public void ARequestKnowsItsPatternAndItsCaller()
        {
            RumblePatternData pattern = Burst(1f);
            RumbleRequest request = new(pattern, _caller, FullIntensity);

            Assert.That(request.Pattern, Is.SameAs(pattern));
            Assert.That(request.Caller, Is.SameAs(_caller));
        }

        /// <summary>A request applies its own intensity on top of the pattern.</summary>
        [Test]
        public void TheRequestIntensityScalesTheSample()
        {
            RumbleRequest request = new(Burst(1f), _caller, 0.5f);

            request.Sample(out float low, out float high);

            Assert.That(low, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(high, Is.EqualTo(0.5f).Within(Tolerance));
        }

        /// <summary>An intensity outside the range a motor accepts is brought back into it.</summary>
        [Test]
        public void TheRequestIntensityIsClamped()
        {
            Assert.That(new RumbleRequest(Burst(1f), _caller, 5f).Intensity, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(new RumbleRequest(Burst(1f), _caller, -5f).Intensity, Is.EqualTo(0f).Within(Tolerance));
        }

        /// <summary>An intensity of nothing silences the request without stopping it.</summary>
        [Test]
        public void AnIntensityOfNothingSilencesTheRequest()
        {
            RumbleRequest request = new(Burst(1f), _caller, 0f);

            request.Sample(out float low, out float high);

            Assert.That(low, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(high, Is.EqualTo(0f).Within(Tolerance));
        }

        /// <summary>A fresh request has not run out yet.</summary>
        [Test]
        public void AFreshRequestHasNotFinished()
            => Assert.That(new RumbleRequest(Burst(1f), _caller, FullIntensity).IsFinished, Is.False);

        /// <summary>
        /// A looping request never finishes on its own, which is what keeps it playing until something
        /// stops it.
        /// </summary>
        [Test]
        public void ALoopingRequestNeverFinishesOnItsOwn()
        {
            RumblePatternData looping = new(1f, Flat(1f), Flat(1f), loop: true);
            RumbleRequest request = new(looping, _caller, FullIntensity);

            request.Advance();

            Assert.That(request.IsFinished, Is.False);
        }

        private static AnimationCurve Flat(float value) => AnimationCurve.Constant(CurveStart, CurveEnd, value);

        private static RumblePatternData Burst(float strength) => new(1f, Flat(strength), Flat(strength));
    }
}