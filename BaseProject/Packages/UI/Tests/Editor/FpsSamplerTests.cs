using System.Collections.Generic;
using Base.UIPackage.Utility;
using NUnit.Framework;

namespace Base.UIPackage.Tests
{
    /// <summary>
    /// What a frame rate counter shows, apart from the component that draws it. The two things worth
    /// covering are both invisible in a screenshot: the reading is smoothed so a single long frame
    /// does not swing it, and it is only offered when it changed so the text does not flicker.
    /// </summary>
    public sealed class FpsSamplerTests
    {
        private const float FastFrame = 0.01f;
        private const float LongFrame = 0.5f;
        private const float SixtyHertzFrame = 1f / 60f;
        private const float SlowFrame = 0.1f;
        private const float SteadyFrame = 0.05f;

        private FpsSampler _sampler;

        /// <summary>A fresh sampler per test, since every reading depends on the ones before it.</summary>
        [SetUp]
        public void Prepare() => _sampler = new FpsSampler();

        /// <summary>
        /// A counter that updated every frame would be unreadable, so nothing is offered until half a
        /// second of frames has gone by.
        /// </summary>
        [Test]
        public void NothingIsOfferedBeforeTheIntervalElapses()
            => Assert.That(Feed(SteadyFrame, 9), Is.Empty);

        /// <summary>Once the interval is over, there is a number to show.</summary>
        [Test]
        public void AReadingArrivesOnceTheIntervalIsOver()
            => Assert.That(Feed(SteadyFrame, 20), Has.Count.EqualTo(1));

        /// <summary>
        /// One frame long enough to fill the interval on its own still produces a reading, so a game
        /// stuttering badly enough to drop under two frames a second still updates its counter.
        /// </summary>
        [Test]
        public void ASingleLongFrameProducesAReading()
        {
            Assert.That(_sampler.TryRead(LongFrame, out int fps), Is.True);
            Assert.That(fps, Is.GreaterThan(0));
        }

        /// <summary>
        /// The reading settles and then stops being offered. A hundred intervals of the same frame time
        /// produce a handful of readings rather than a hundred, which is what keeps the text still.
        /// </summary>
        [Test]
        public void ASteadyFrameRateStopsProducingReadings()
        {
            List<int> readings = Feed(SteadyFrame, 1000);

            Assert.That(readings, Is.Not.Empty);
            Assert.That(readings, Has.Count.LessThan(10));
        }

        /// <summary>A faster frame rate reads as a higher number, which is the whole contract.</summary>
        [Test]
        public void AFasterRateReadsAsAHigherNumber()
        {
            int fast = Last(Feed(FastFrame, 200));

            _sampler = new FpsSampler();

            int slow = Last(Feed(SlowFrame, 40));

            Assert.That(fast, Is.GreaterThan(slow));
        }

        /// <summary>
        /// The point of the smoothing. A run of fast frames interrupted by one that took half a second
        /// reads well above the two frames a second that one frame ran at.
        /// </summary>
        [Test]
        public void OneLongFrameDoesNotSwingTheReadingToItsOwnRate()
        {
            Feed(SixtyHertzFrame, 30);

            Assert.That(_sampler.TryRead(LongFrame, out int fps), Is.True);
            Assert.That(fps, Is.GreaterThan(10));
        }

        /// <summary>
        /// The first reading is always shown, whatever it turns out to be, because a counter that
        /// started on a number nobody set would stay blank until the rate happened to change.
        /// </summary>
        [Test]
        public void TheFirstReadingIsAlwaysShown()
            => Assert.That(Feed(SixtyHertzFrame, 40), Is.Not.Empty);

        /// <summary>The number a run of frames settled on, which is the one left on screen.</summary>
        /// <param name="readings">The readings the run produced.</param>
        /// <returns>The last reading.</returns>
        private static int Last(List<int> readings) => readings[^1];

        /// <summary>Runs a number of equally long frames through the sampler.</summary>
        /// <param name="deltaTime">How long each frame takes.</param>
        /// <param name="frames">How many frames to run.</param>
        /// <returns>Every reading the run produced, in order.</returns>
        private List<int> Feed(float deltaTime, int frames)
        {
            List<int> readings = new();

            for (int i = 0; i < frames; i++)
            {
                if (_sampler.TryRead(deltaTime, out int fps))
                    readings.Add(fps);
            }

            return readings;
        }
    }
}