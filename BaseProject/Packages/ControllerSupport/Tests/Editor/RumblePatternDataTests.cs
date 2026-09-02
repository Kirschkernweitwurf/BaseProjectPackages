using Base.ControllerSupportPackage.Haptics;
using NUnit.Framework;
using UnityEngine;

namespace Base.ControllerSupportPackage.Tests
{
    /// <summary>
    /// Covers the curve-driven description of a haptic. Both motors are authored over normalized time,
    /// so what matters is that a sample never leaves the range a motor accepts, however the curve was
    /// drawn, and that a duration can never reach zero and divide the playback by nothing.
    /// </summary>
    public sealed class RumblePatternDataTests
    {
        private const float CurveEnd = 1f;
        private const float CurveStart = 0f;
        private const float Tolerance = 0.0001f;

        /// <summary>A pattern built for serialization holds both motors at full strength.</summary>
        [Test]
        public void AFreshPatternRunsBothMotors()
        {
            RumblePatternData pattern = new();

            pattern.Evaluate(0.5f, out float low, out float high);

            Assert.That(low, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(high, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(pattern.Duration, Is.GreaterThan(0f));
            Assert.That(pattern.Loop, Is.False);
            Assert.That(pattern.UseUnscaledTime, Is.True);
        }

        /// <summary>A pattern keeps what it was built with.</summary>
        [Test]
        public void APatternKeepsWhatItWasBuiltWith()
        {
            RumblePatternData pattern = new(2f, Flat(0.25f), Flat(0.75f), loop: true, useUnscaledTime: false);

            Assert.That(pattern.Duration, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(pattern.Loop, Is.True);
            Assert.That(pattern.UseUnscaledTime, Is.False);
        }

        /// <summary>
        /// A duration can never reach zero, since the playback divides its elapsed time by it.
        /// </summary>
        [Test]
        public void ADurationNeverReachesZero()
        {
            RumblePatternData pattern = new(0f, Flat(1f), Flat(1f));

            Assert.That(pattern.Duration, Is.GreaterThan(0f));
        }

        /// <summary>Each motor is sampled from its own curve.</summary>
        [Test]
        public void EachMotorIsSampledFromItsOwnCurve()
        {
            RumblePatternData pattern = new(1f, Flat(0.25f), Flat(0.75f));

            pattern.Evaluate(0.5f, out float low, out float high);

            Assert.That(low, Is.EqualTo(0.25f).Within(Tolerance));
            Assert.That(high, Is.EqualTo(0.75f).Within(Tolerance));
        }

        /// <summary>A curve that rises is sampled differently at different times.</summary>
        [Test]
        public void ARisingCurveIsSampledOverTime()
        {
            RumblePatternData pattern = new(1f, AnimationCurve.Linear(CurveStart, CurveStart, CurveEnd, CurveEnd),
                Flat(0f));

            pattern.Evaluate(0f, out float atStart, out float _);
            pattern.Evaluate(1f, out float atEnd, out float _);

            Assert.That(atStart, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(atEnd, Is.EqualTo(1f).Within(Tolerance));
        }

        /// <summary>
        /// A curve drawn outside the range a motor accepts is brought back into it, so an overshooting
        /// tangent cannot ask for a strength that does not exist.
        /// </summary>
        [Test]
        public void ASampleNeverLeavesTheRangeAMotorAccepts()
        {
            RumblePatternData pattern = new(1f, Flat(5f), Flat(-5f));

            pattern.Evaluate(0.5f, out float low, out float high);

            Assert.That(low, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(high, Is.EqualTo(0f).Within(Tolerance));
        }

        /// <summary>A flat burst holds both motors steady and does not repeat.</summary>
        [Test]
        public void AFlatBurstHoldsBothMotorsSteady()
        {
            RumblePatternData pattern = RumblePatternData.Constant(0.4f, 0.6f, 0.5f);

            pattern.Evaluate(0f, out float atStart, out float highAtStart);
            pattern.Evaluate(1f, out float atEnd, out float highAtEnd);

            Assert.That(atStart, Is.EqualTo(0.4f).Within(Tolerance));
            Assert.That(atEnd, Is.EqualTo(0.4f).Within(Tolerance));
            Assert.That(highAtStart, Is.EqualTo(0.6f).Within(Tolerance));
            Assert.That(highAtEnd, Is.EqualTo(0.6f).Within(Tolerance));
            Assert.That(pattern.Loop, Is.False);
            Assert.That(pattern.Duration, Is.EqualTo(0.5f).Within(Tolerance));
        }

        /// <summary>A flat burst asked for more than a motor can give is brought back into range.</summary>
        [Test]
        public void AFlatBurstClampsWhatItWasAskedFor()
        {
            RumblePatternData pattern = RumblePatternData.Constant(2f, -1f, 0.5f);

            pattern.Evaluate(0.5f, out float low, out float high);

            Assert.That(low, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(high, Is.EqualTo(0f).Within(Tolerance));
        }

        private static AnimationCurve Flat(float value) => AnimationCurve.Constant(CurveStart, CurveEnd, value);
    }
}