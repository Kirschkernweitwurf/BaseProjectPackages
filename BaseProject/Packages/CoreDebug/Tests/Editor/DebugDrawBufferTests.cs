using Base.CoreDebugPackage.DebugDrawing;
using NUnit.Framework;
using UnityEngine;

namespace Base.CoreDebugPackage.Tests
{
    /// <summary>
    /// What the debug draw buffer keeps and what it throws away.
    /// <para>
    /// Nothing here throws when it goes wrong. A segment that outlives its duration is a line that
    /// never leaves the screen, and one that expires a frame early is a line nobody ever sees, which
    /// is exactly the kind of thing a developer blames on their own maths for an afternoon.
    /// </para>
    /// </summary>
    public sealed class DebugDrawBufferTests
    {
        private const float LongDuration = 10f;
        private const float OneFrame = 0f;

        /// <summary>Leaves the buffer empty and switched on, since it is shared static state.</summary>
        [SetUp]
        public void Prepare()
        {
            DebugDrawBuffer.SetEnabled(true);
            DebugDrawBuffer.Clear();
        }

        /// <summary>And again afterwards, so a failing test cannot leak into the next one.</summary>
        [TearDown]
        public void Cleanup()
        {
            DebugDrawBuffer.SetEnabled(true);
            DebugDrawBuffer.Clear();
        }

        /// <summary>
        /// Depth tested and overlay segments are sorted as they arrive, because those are the two
        /// passes they are drawn in. Landing in the wrong list draws the line through walls or behind
        /// them.
        /// </summary>
        [Test]
        public void ASegmentGoesToTheListItsPassDrawsFrom()
        {
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, LongDuration, true);
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, LongDuration, false);

            Assert.That(DebugDrawBuffer.DepthTestedLineCommands, Has.Count.EqualTo(1));
            Assert.That(DebugDrawBuffer.OverlayLineCommands, Has.Count.EqualTo(1));
        }

        /// <summary>Labels are held apart from segments, since a different pass draws them.</summary>
        [Test]
        public void ALabelIsHeldApartFromTheSegments()
        {
            DebugDrawBuffer.AddLabel(Vector3.zero, "here", Color.white, LongDuration);

            Assert.That(DebugDrawBuffer.LabelCommands, Has.Count.EqualTo(1));
            Assert.That(DebugDrawBuffer.DepthTestedLineCommands, Is.Empty);
        }

        /// <summary>
        /// A duration of zero means one frame, and the frame it was queued in counts however late in
        /// that frame the call happened. Pruning inside the same frame has to keep it.
        /// </summary>
        [Test]
        public void ASingleFrameSegmentSurvivesTheFrameItWasQueuedIn()
        {
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, OneFrame, true);
            DebugDrawBuffer.Prune();

            Assert.That(DebugDrawBuffer.DepthTestedLineCommands, Has.Count.EqualTo(1));
        }

        /// <summary>A segment with time left on it is kept.</summary>
        [Test]
        public void ASegmentWithTimeLeftIsKept()
        {
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, LongDuration, false);
            DebugDrawBuffer.Prune();

            Assert.That(DebugDrawBuffer.OverlayLineCommands, Has.Count.EqualTo(1));
        }

        /// <summary>
        /// Switching drawing off drops what is queued as well as refusing what comes next. Leaving the
        /// queue standing would put the old lines back the moment it is switched on again.
        /// </summary>
        [Test]
        public void SwitchingOffDropsWhatWasAlreadyQueued()
        {
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, LongDuration, true);
            DebugDrawBuffer.SetEnabled(false);

            Assert.That(DebugDrawBuffer.IsEnabled, Is.False);
            Assert.That(DebugDrawBuffer.DepthTestedLineCommands, Is.Empty);
        }

        /// <summary>While it is off, adding is a no-op rather than an error.</summary>
        [Test]
        public void NothingIsQueuedWhileDrawingIsOff()
        {
            DebugDrawBuffer.SetEnabled(false);
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, LongDuration, true);
            DebugDrawBuffer.AddLabel(Vector3.zero, "here", Color.white, LongDuration);

            Assert.That(DebugDrawBuffer.DepthTestedLineCommands, Is.Empty);
            Assert.That(DebugDrawBuffer.LabelCommands, Is.Empty);
        }

        /// <summary>Switching it back on accepts commands again, and starts from empty.</summary>
        [Test]
        public void SwitchingBackOnStartsFromEmpty()
        {
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, LongDuration, true);
            DebugDrawBuffer.SetEnabled(false);
            DebugDrawBuffer.SetEnabled(true);
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, LongDuration, true);

            Assert.That(DebugDrawBuffer.DepthTestedLineCommands, Has.Count.EqualTo(1));
        }

        /// <summary>Clearing empties every list at once, not just the one that overflowed.</summary>
        [Test]
        public void ClearingEmptiesEveryList()
        {
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, LongDuration, true);
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.one, Color.red, LongDuration, false);
            DebugDrawBuffer.AddLabel(Vector3.zero, "here", Color.white, LongDuration);

            DebugDrawBuffer.Clear();

            Assert.That(DebugDrawBuffer.DepthTestedLineCommands, Is.Empty);
            Assert.That(DebugDrawBuffer.OverlayLineCommands, Is.Empty);
            Assert.That(DebugDrawBuffer.LabelCommands, Is.Empty);
        }

        /// <summary>Clearing an empty buffer is allowed, so a shutdown path never has to check first.</summary>
        [Test]
        public void ClearingAnEmptyBufferIsAllowed()
        {
            DebugDrawBuffer.Clear();

            Assert.That(DebugDrawBuffer.LabelCommands, Is.Empty);
        }

        /// <summary>
        /// Pruning keeps the order the commands were queued in. The renderer walks the list straight
        /// through, so a reordered list draws the same picture in a different order and any blending
        /// between overlapping labels changes with it.
        /// </summary>
        [Test]
        public void PruningKeepsTheOrderThingsWereQueuedIn()
        {
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.right, Color.red, LongDuration, true);
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.up, Color.green, LongDuration, true);
            DebugDrawBuffer.AddLine(Vector3.zero, Vector3.forward, Color.blue, LongDuration, true);

            DebugDrawBuffer.Prune();

            Assert.That(DebugDrawBuffer.DepthTestedLineCommands[0].To, Is.EqualTo(Vector3.right));
            Assert.That(DebugDrawBuffer.DepthTestedLineCommands[1].To, Is.EqualTo(Vector3.up));
            Assert.That(DebugDrawBuffer.DepthTestedLineCommands[2].To, Is.EqualTo(Vector3.forward));
        }

        /// <summary>A segment keeps the two ends and the color it was queued with.</summary>
        [Test]
        public void ASegmentKeepsWhatItWasQueuedWith()
        {
            DebugDrawBuffer.AddLine(Vector3.one, Vector3.up, Color.green, LongDuration, true);

            DebugLineCommand command = DebugDrawBuffer.DepthTestedLineCommands[0];

            Assert.That(command.From, Is.EqualTo(Vector3.one));
            Assert.That(command.To, Is.EqualTo(Vector3.up));
            Assert.That(command.Color, Is.EqualTo(Color.green));
        }
    }
}