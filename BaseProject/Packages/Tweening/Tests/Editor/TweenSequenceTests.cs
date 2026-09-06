using System.Collections.Generic;
using System.Text.RegularExpressions;
using Base.TweeningPackage.Core;
using Base.TweeningPackage.Core.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers how a sequence chains its children. A sequential one may only ever have one child
    /// running, a parallel one has to wait for the slowest, and both have to finish exactly once
    /// however they end.
    /// </summary>
    public sealed class TweenSequenceTests
    {
        private List<string> _events;

        /// <summary>Every test starts with an empty event log.</summary>
        [SetUp]
        public void Build() => _events = new List<string>();

        /// <summary>A sequence with nothing in it finishes the moment it is started.</summary>
        [Test]
        public void AnEmptySequenceFinishesImmediately()
        {
            TweenSequence sequence = new(ESequenceMode.Sequential);

            Listen(sequence);
            sequence.Start();

            Assert.That(sequence.IsCompleted, Is.True);
            Assert.That(_events, Is.EqualTo(new[]
            {
                "Complete",
                "Kill"
            }));
        }

        /// <summary>A sequential run starts only the first child.</summary>
        [Test]
        public void ASequentialRunStartsOneChildAtATime()
        {
            TweenProbe first = new();
            TweenProbe second = new();
            TweenSequence sequence = Sequence(ESequenceMode.Sequential, first, second);

            sequence.Start();

            Assert.That(first.StartCount, Is.EqualTo(1));
            Assert.That(second.StartCount, Is.EqualTo(0));
        }

        /// <summary>A finished child hands over to the next one.</summary>
        [Test]
        public void AFinishedChildHandsOverToTheNext()
        {
            TweenProbe first = new();
            TweenProbe second = new();
            TweenSequence sequence = Sequence(ESequenceMode.Sequential, first, second);

            sequence.Start();
            first.Finish();

            Assert.That(second.StartCount, Is.EqualTo(1));
            Assert.That(sequence.IsCompleted, Is.False);
        }

        /// <summary>The sequence finishes once its last child does.</summary>
        [Test]
        public void ASequentialRunFinishesWithItsLastChild()
        {
            TweenProbe first = new();
            TweenProbe second = new();
            TweenSequence sequence = Sequence(ESequenceMode.Sequential, first, second);

            Listen(sequence);
            sequence.Start();
            first.Finish();
            second.Finish();

            Assert.That(sequence.IsCompleted, Is.True);
            Assert.That(sequence.IsRunning, Is.False);
            Assert.That(_events, Is.EqualTo(new[]
            {
                "Complete",
                "Kill"
            }));
        }

        /// <summary>A parallel run starts every child at once.</summary>
        [Test]
        public void AParallelRunStartsEveryChildAtOnce()
        {
            TweenProbe first = new();
            TweenProbe second = new();

            Sequence(ESequenceMode.Parallel, first, second).Start();

            Assert.That(first.StartCount, Is.EqualTo(1));
            Assert.That(second.StartCount, Is.EqualTo(1));
        }

        /// <summary>A parallel run waits for the slowest child rather than the first.</summary>
        [Test]
        public void AParallelRunWaitsForTheSlowestChild()
        {
            TweenProbe first = new();
            TweenProbe second = new();
            TweenSequence sequence = Sequence(ESequenceMode.Parallel, first, second);

            Listen(sequence);
            sequence.Start();
            first.Finish();

            Assert.That(sequence.IsCompleted, Is.False);
            Assert.That(_events, Is.Empty);

            second.Finish();

            Assert.That(sequence.IsCompleted, Is.True);
            Assert.That(_events, Is.EqualTo(new[]
            {
                "Complete",
                "Kill"
            }));
        }

        /// <summary>Cancelling stops every child without completing any of them.</summary>
        [Test]
        public void CancellingStopsEveryChild()
        {
            TweenProbe first = new();
            TweenProbe second = new();
            TweenSequence sequence = Sequence(ESequenceMode.Parallel, first, second);

            Listen(sequence);
            sequence.Start();
            sequence.Stop();

            Assert.That(first.StopCount, Is.EqualTo(1));
            Assert.That(second.StopCount, Is.EqualTo(1));
            Assert.That(first.WasStoppedWithComplete, Is.False);
            Assert.That(_events, Is.EqualTo(new[]
            {
                "Kill"
            }));
        }

        /// <summary>Stopping with a snap carries that through to every child.</summary>
        [Test]
        public void StoppingWithASnapCarriesThroughToTheChildren()
        {
            TweenProbe first = new();
            TweenProbe second = new();
            TweenSequence sequence = Sequence(ESequenceMode.Parallel, first, second);

            Listen(sequence);
            sequence.Start();
            sequence.Stop(complete: true);

            Assert.That(first.WasStoppedWithComplete, Is.True);
            Assert.That(second.WasStoppedWithComplete, Is.True);
            Assert.That(_events, Is.EqualTo(new[]
            {
                "Complete",
                "Kill"
            }));
        }

        /// <summary>Stopping a finished sequence changes nothing.</summary>
        [Test]
        public void StoppingAFinishedSequenceChangesNothing()
        {
            TweenProbe only = new();
            TweenSequence sequence = Sequence(ESequenceMode.Sequential, only);

            Listen(sequence);
            sequence.Start();
            only.Finish();
            sequence.Stop();

            Assert.That(_events, Is.EqualTo(new[]
            {
                "Complete",
                "Kill"
            }));

            Assert.That(only.StopCount, Is.EqualTo(0));
        }

        /// <summary>Nothing to add is reported rather than stored as a gap in the chain.</summary>
        [Test]
        public void NothingToAddIsReported()
        {
            TweenSequence sequence = new(ESequenceMode.Sequential);

            LogAssert.Expect(LogType.Warning, new Regex("null tween"));

            sequence.Add(null);
            Listen(sequence);
            sequence.Start();

            Assert.That(_events, Is.EqualTo(new[]
            {
                "Complete",
                "Kill"
            }), "the sequence stayed empty");
        }

        /// <summary>A sequence is driven by its children, so ticking it does nothing itself.</summary>
        [Test]
        public void TickingASequenceDoesNothing()
        {
            TweenProbe only = new();
            TweenSequence sequence = Sequence(ESequenceMode.Sequential, only);

            sequence.Start();
            sequence.Tick(1f);

            Assert.That(sequence.IsCompleted, Is.False);
        }

        private static TweenSequence Sequence(ESequenceMode mode, params TweenBase[] tweens)
        {
            TweenSequence sequence = new(mode);

            foreach (TweenBase tween in tweens)
                sequence.Add(tween);

            return sequence;
        }

        private void Listen(TweenBase sequence)
        {
            sequence.OnComplete += _ => _events.Add("Complete");
            sequence.OnKill += _ => _events.Add("Kill");
        }
    }
}