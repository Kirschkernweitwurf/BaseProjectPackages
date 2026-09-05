using System.Collections.Generic;
using Base.CorePackage.Editor.StateMachine;
using NUnit.Framework;
using UnityEngine;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// Covers how a machine is arranged for the monitor window. The whole point of the drawing is that
    /// it reads left to right in the order the machine can run, so a state placed in the wrong column
    /// does not look broken, it just quietly tells the reader the wrong thing about their machine.
    /// </summary>
    public sealed class StateMachineLayoutTests
    {
        private const string Entry = "Idle";
        private const string Middle = "Walking";
        private const string Orphan = "Unreachable";
        private const string Second = "Running";
        private const float Tolerance = 0.01f;

        /// <summary>Nothing to lay out yields no placements rather than an empty drawing.</summary>
        [Test]
        public void AMachineWithNoStatesIsNotLaidOut()
            => Assert.That(Calculate(new StateMachineShape()), Is.Empty);

        /// <summary>Nothing at all is not laid out either.</summary>
        [Test]
        public void NothingIsNotLaidOut()
            => Assert.That(StateMachineLayout.Calculate(null, false), Is.Empty);

        /// <summary>Every state the machine knows gets a position, reachable or not.</summary>
        [Test]
        public void EveryStateGetsAPosition()
        {
            Dictionary<string, Vector2> placements = Calculate(new StateMachineShape()
                .WithStates(Entry, Middle, Orphan)
                .StartingAt(Entry)
                .WithEdge(Entry, Middle));

            Assert.That(placements, Has.Count.EqualTo(3));
        }

        /// <summary>
        /// A state one transition from the start sits in the next column along, which is what makes the
        /// drawing read as the order the machine runs in.
        /// </summary>
        [Test]
        public void AStateOneTransitionAwaySitsInTheNextColumn()
        {
            Dictionary<string, Vector2> placements = Calculate(new StateMachineShape()
                .WithStates(Entry, Middle)
                .StartingAt(Entry)
                .WithEdge(Entry, Middle));

            Assert.That(placements[Middle].x - placements[Entry].x,
                Is.GreaterThan(StateMachineLayout.NodeWidth));
        }

        /// <summary>Two states the same distance from the start share a column and so an x.</summary>
        [Test]
        public void TwoStatesTheSameDistanceAwayShareAColumn()
        {
            Dictionary<string, Vector2> placements = Calculate(new StateMachineShape()
                .WithStates(Entry, Middle, Second)
                .StartingAt(Entry)
                .WithEdge(Entry, Middle)
                .WithEdge(Entry, Second));

            Assert.That(placements[Middle].x, Is.EqualTo(placements[Second].x).Within(Tolerance));
        }

        /// <summary>
        /// A state nothing can reach is parked behind every reachable one instead of being left
        /// scattered, which is how it gets noticed.
        /// </summary>
        [Test]
        public void AnUnreachableStateIsParkedBehindEveryReachableOne()
        {
            Dictionary<string, Vector2> placements = Calculate(new StateMachineShape()
                .WithStates(Entry, Middle, Orphan)
                .StartingAt(Entry)
                .WithEdge(Entry, Middle));

            Assert.That(placements[Orphan].x, Is.GreaterThan(placements[Middle].x));
        }

        /// <summary>
        /// A target of an any state transition can be entered from anywhere but is not where the
        /// machine begins, so it starts one column in rather than beside the entry state.
        /// </summary>
        [Test]
        public void AnAnyStateTargetStartsOneColumnIn()
        {
            Dictionary<string, Vector2> placements = Calculate(new StateMachineShape()
                .WithStates(Entry, Middle)
                .StartingAt(Entry)
                .WithAnyStateEdge(Middle));

            Assert.That(placements[Middle].x, Is.GreaterThan(placements[Entry].x));
        }

        /// <summary>
        /// The any state node is parked above the first column, so when it is drawn everything else has
        /// to come down far enough to leave room for it.
        /// </summary>
        [Test]
        public void RoomIsLeftAboveWhenTheAnyStateNodeIsDrawn()
        {
            StateMachineShape shape = new StateMachineShape().WithStates(Entry).StartingAt(Entry);

            float without = StateMachineLayout.Calculate(shape, false)[Entry].y;
            float with = StateMachineLayout.Calculate(shape, true)[Entry].y;

            Assert.That(with - without, Is.GreaterThan(StateMachineLayout.NodeHeight));
        }

        /// <summary>
        /// A short column is centred against the tallest one rather than hung from the top, so a
        /// branch reads as a branch instead of as a list.
        /// </summary>
        [Test]
        public void AShorterColumnIsCentredAgainstTheTallest()
        {
            Dictionary<string, Vector2> placements = Calculate(new StateMachineShape()
                .WithStates(Entry, Middle, Second)
                .StartingAt(Entry)
                .WithEdge(Entry, Middle)
                .WithEdge(Entry, Second));

            float branchCentre = (placements[Middle].y + placements[Second].y) * 0.5f;

            Assert.That(placements[Entry].y, Is.EqualTo(branchCentre).Within(Tolerance));
        }

        /// <summary>
        /// A machine that has not run yet names no initial state, so the first state it was told about
        /// stands in and the drawing still reads from the left.
        /// </summary>
        [Test]
        public void AMachineThatNeverRanStartsFromItsFirstState()
        {
            Dictionary<string, Vector2> placements = Calculate(new StateMachineShape()
                .WithStates(Entry, Middle)
                .WithEdge(Entry, Middle));

            Assert.That(placements[Entry].x, Is.LessThan(placements[Middle].x));
        }

        /// <summary>Lays out the given shape without the any state node.</summary>
        private static Dictionary<string, Vector2> Calculate(StateMachineShape shape)
            => StateMachineLayout.Calculate(shape, false);
    }
}