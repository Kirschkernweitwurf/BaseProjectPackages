using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the conversion a volume slider depends on: full volume is the zero point, half volume is
    /// the expected drop, and silence lands on a floor instead of running off to negative infinity.
    /// </summary>
    public sealed class AudioMathUtilityTests
    {
        private const float DecibelTolerance = 0.05f;
        private const float HalfVolume = 0.5f;
        private const float HalfVolumeDecibels = -6.02f;
        private const float LinearTolerance = 0.0001f;
        private const float QuarterVolume = 0.25f;
        private const float SilenceDecibels = -80f;

        /// <summary>Full volume is the point the scale is measured from.</summary>
        [Test]
        public void FullVolumeIsZeroDecibels() => Assert.That(AudioMathUtility.ConvertLinearToDecibel(1f),
            Is.EqualTo(0f).Within(LinearTolerance));

        /// <summary>Halving the linear value drops the volume by about six decibels.</summary>
        [Test]
        public void HalfVolumeIsAboutSixDecibelsDown() => Assert.That(
            AudioMathUtility.ConvertLinearToDecibel(HalfVolume),
            Is.EqualTo(HalfVolumeDecibels).Within(DecibelTolerance));

        /// <summary>Silence has to clamp to the floor, since the logarithm has no value at zero.</summary>
        [Test]
        public void SilenceClampsToTheFloor() => Assert.That(AudioMathUtility.ConvertLinearToDecibel(0f),
            Is.EqualTo(SilenceDecibels).Within(LinearTolerance));

        /// <summary>A negative value is below silence, so it clamps to the same floor.</summary>
        [Test]
        public void ANegativeVolumeClampsToTheFloor() => Assert.That(AudioMathUtility.ConvertLinearToDecibel(-1f),
            Is.EqualTo(SilenceDecibels).Within(LinearTolerance));

        /// <summary>Converting out and back has to land on the value it started from.</summary>
        [Test]
        public void TheConversionsUndoEachOther()
        {
            float decibels = AudioMathUtility.ConvertLinearToDecibel(QuarterVolume);

            Assert.That(AudioMathUtility.ConvertDecibelToLinear(decibels),
                Is.EqualTo(QuarterVolume).Within(LinearTolerance));
        }
    }
}