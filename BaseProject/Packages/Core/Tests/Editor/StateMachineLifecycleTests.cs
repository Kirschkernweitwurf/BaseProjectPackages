using System.Collections.Generic;
using Base.CorePackage.StateMachine;
using NUnit.Framework;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// Covers what a machine does around a run rather than inside one: entering and leaving states, the
    /// clock it keeps, the shape it reports to tooling and whether it lets go of itself when it stops.
    /// </summary>
    public sealed class StateMachineLifecycleTests
    {
        private const string Left = "Left";
        private const string Right = "Right";
        private const string Start = "Start";

        private StateMachineProbe _probe;
        private StateMachine<StateMachineProbe> _machine;
        private IState<StateMachineProbe> _start;
        private IState<StateMachineProbe> _left;
        private IState<StateMachineProbe> _right;

        /// <summary>Builds a fresh three state machine for every test.</summary>
        [SetUp]
        public void Build()
        {
            _probe = new StateMachineProbe();

            _start = _probe.CreateState(Start);
            _left = _probe.CreateState(Left);
            _right = _probe.CreateState(Right);

            _machine = new StateMachine<StateMachineProbe>(_probe);
        }

        /// <summary>Releases the machine so it does not linger in the registry between tests.</summary>
        [TearDown]
        public void Release() => _machine.Dispose();

        /// <summary>Starting enters the initial state and nothing else.</summary>
        [Test]
        public void StartEntersTheInitialStateOnly()
        {
            _machine.Start(_start);

            Assert.That(_probe.Entered, Is.EqualTo(new[] { Start }), "only the initial state should be entered");
            Assert.That(_machine.InitialStateName, Is.EqualTo(Start), "the machine should report where it began");
        }

        /// <summary>A switch leaves the old state before it enters the new one.</summary>
        [Test]
        public void SwitchingExitsBeforeItEnters()
        {
            _machine.AddTransition(_start, _left, null);
            _machine.Start(_start);

            _machine.Tick(1f);

            Assert.That(_probe.Exited, Is.EqualTo(new[] { Start }), "the old state should be left");
            Assert.That(_probe.LastEntered(), Is.EqualTo(Left), "the new state should be entered afterwards");
        }

        /// <summary>The clock counts up while a state holds and restarts when it changes.</summary>
        [Test]
        public void TimeInStateResetsOnASwitch()
        {
            _machine.AddTransition(_start, _left, static probe => probe.GoLeft);
            _machine.Start(_start);

            _machine.Tick(0.5f);
            _machine.Tick(0.5f);

            Assert.That(_machine.TimeInState, Is.EqualTo(1f).Within(0.001f), "the clock should accumulate");

            _probe.GoLeft = true;
            _machine.Tick(0.5f);

            Assert.That(_machine.TimeInState, Is.EqualTo(0f).Within(0.001f), "a switch should restart the clock");
        }

        /// <summary>Forcing a state ignores every condition and reports the reason it was given.</summary>
        [Test]
        public void ForceStateSwitchesWithoutAskingAnyCondition()
        {
            const string reason = "Died";

            _machine.Start(_start);
            _machine.ForceState(_right, reason);

            Assert.That(_machine.CurrentStateName, Is.EqualTo(Right), "the forced state should be active");
            Assert.That(_machine.LastTransitionName, Is.EqualTo(reason), "the reason should be reported as is");
        }

        /// <summary>Stopping leaves the active state and stops evaluating anything.</summary>
        [Test]
        public void StopExitsTheActiveStateAndStaysStopped()
        {
            _machine.AddTransition(_start, _left, null);
            _machine.Start(_start);

            _machine.Stop();
            _machine.Tick(1f);

            Assert.That(_probe.Exited, Is.EqualTo(new[] { Start }), "the active state should be left on stop");
            Assert.That(_machine.IsRunning, Is.False, "a stopped machine should stay stopped");
            Assert.That(_probe.LastEntered(), Is.EqualTo(Start), "no transition should fire after a stop");
        }

        /// <summary>A running machine is visible to tooling, and a stopped one is not.</summary>
        [Test]
        public void RegistryFollowsTheRun()
        {
            _machine.Start(_start);

            Assert.That(StateMachineRegistry.GetRunning(), Contains.Item(_machine),
                "a running machine should be listed");

            _machine.Stop();

            Assert.That(StateMachineRegistry.GetRunning(), Has.No.Member(_machine),
                "a stopped machine should drop out of the list");
        }

        /// <summary>The reported shape covers every state and transition the machine was told about.</summary>
        [Test]
        public void ShapeReportsEveryStateAndTransition()
        {
            _machine.AddTransition(_start, _left, null, Left);
            _machine.AddAnyTransition(_right, null, Right);

            _machine.Start(_start);

            Assert.That(_machine.StateNames, Is.EquivalentTo(new[] { Start, Left, Right }),
                "every state the machine was told about should be reported");

            IReadOnlyList<StateMachineEdge> edges = _machine.Edges;

            Assert.That(edges.Count, Is.EqualTo(2), "both transitions should be reported");
            Assert.That(edges[0].IsFromAnyState, Is.True, "any state transitions come first, as they are asked");
            Assert.That(edges[1].From, Is.EqualTo(Start), "the second edge should leave the state that owns it");
        }
    }
}