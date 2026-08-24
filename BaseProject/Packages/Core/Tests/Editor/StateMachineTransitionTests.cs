using Base.CorePackage.StateMachine;
using NUnit.Framework;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// Covers which transition a machine picks. Evaluation order is the part of a state machine that is
    /// invisible from the outside and the part a stuck machine almost always turns on, so it is the part
    /// worth pinning down.
    /// </summary>
    public sealed class StateMachineTransitionTests
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

        /// <summary>A condition that does not hold leaves the machine where it is.</summary>
        [Test]
        public void ClosedConditionKeepsTheMachineInPlace()
        {
            _machine.AddTransition(_start, _left, static probe => probe.GoLeft);
            _machine.Start(_start);

            _machine.Tick(1f);

            Assert.That(_machine.CurrentStateName, Is.EqualTo(Start), "nothing opened, so nothing should move");
        }

        /// <summary>The higher priority wins even when both conditions hold.</summary>
        [Test]
        public void HigherPriorityWinsOverDeclarationOrder()
        {
            _machine.AddTransition(_start, _left, static probe => probe.GoLeft);
            _machine.AddTransition(_start, _right, static probe => probe.GoRight, priority: 5);

            _machine.Start(_start);

            _probe.GoLeft = true;
            _probe.GoRight = true;

            _machine.Tick(1f);

            Assert.That(_machine.CurrentStateName, Is.EqualTo(Right), "the higher priority was declared second");
        }

        /// <summary>Equal priorities keep the order they were added in.</summary>
        [Test]
        public void EqualPrioritiesKeepDeclarationOrder()
        {
            _machine.AddTransition(_start, _left, static probe => probe.GoLeft);
            _machine.AddTransition(_start, _right, static probe => probe.GoRight);

            _machine.Start(_start);

            _probe.GoLeft = true;
            _probe.GoRight = true;

            _machine.Tick(1f);

            Assert.That(_machine.CurrentStateName, Is.EqualTo(Left), "the first one added should be asked first");
        }

        /// <summary>An any state transition is asked before the ones leaving the active state.</summary>
        [Test]
        public void AnyStateTransitionIsAskedFirst()
        {
            _machine.AddTransition(_start, _left, static probe => probe.GoLeft);
            _machine.AddAnyTransition(_right, static probe => probe.GoRight);

            _machine.Start(_start);

            _probe.GoLeft = true;
            _probe.GoRight = true;

            _machine.Tick(1f);

            Assert.That(_machine.CurrentStateName, Is.EqualTo(Right), "any state transitions come first");
        }

        /// <summary>An any state transition never fires into the state the machine is already in.</summary>
        [Test]
        public void AnyStateTransitionDoesNotReenterTheActiveState()
        {
            _machine.AddAnyTransition(_start, static _ => true);
            _machine.Start(_start);

            _machine.Tick(1f);

            Assert.That(_probe.Entered.Count, Is.EqualTo(1), "the machine should not re-enter where it already is");
        }

        /// <summary>A null condition always holds, which is how a pass through state hands off.</summary>
        [Test]
        public void NullConditionAlwaysHolds()
        {
            _machine.AddTransition(_start, _left, null);
            _machine.Start(_start);

            _machine.Tick(1f);

            Assert.That(_machine.CurrentStateName, Is.EqualTo(Left), "an unconditional transition should fire");
        }

        /// <summary>Several transitions can chain within one tick, and the active state is ticked once.</summary>
        [Test]
        public void ChainedTransitionsResolveWithinOneTick()
        {
            _machine.AddTransition(_start, _left, null);
            _machine.AddTransition(_left, _right, null);

            _machine.Start(_start);

            _machine.Tick(1f);

            Assert.That(_machine.CurrentStateName, Is.EqualTo(Right), "both transitions should resolve in one tick");
            Assert.That(_probe.Ticks, Is.EqualTo(1), "only the state the machine settled in should be ticked");
        }
    }
}