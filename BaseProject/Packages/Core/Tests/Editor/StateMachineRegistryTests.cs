using Base.CorePackage.StateMachine;
using NUnit.Framework;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// Covers the list the monitor window reads. A machine appears while it runs and disappears when
    /// it stops, without the object that owns it having to publish anything.
    /// </summary>
    /// <remarks>
    /// The registry is global, so the assertions ask whether a specific machine is listed rather than
    /// how many are, which keeps them independent of whatever else is running.
    /// </remarks>
    public sealed class StateMachineRegistryTests
    {
        private const string Idle = "Idle";

        private StateMachineProbe _probe;
        private StateMachine<StateMachineProbe> _machine;
        private IState<StateMachineProbe> _idle;

        /// <summary>Builds a machine that is not running yet.</summary>
        [SetUp]
        public void Build()
        {
            _probe = new StateMachineProbe();
            _idle = _probe.CreateState(Idle);
            _machine = new StateMachine<StateMachineProbe>(_probe);
        }

        /// <summary>Releases the machine so it cannot linger between tests.</summary>
        [TearDown]
        public void Release() => _machine.Dispose();

        /// <summary>A machine that was built but never started is not running.</summary>
        [Test]
        public void AMachineThatNeverStartedIsNotListed()
            => Assert.That(StateMachineRegistry.GetRunning(), Has.No.Member(_machine));

        /// <summary>Starting a machine puts it on the list without any registration call.</summary>
        [Test]
        public void AStartedMachineIsListed()
        {
            _machine.Start(_idle);

            Assert.That(StateMachineRegistry.GetRunning(), Contains.Item(_machine));
        }

        /// <summary>Stopping takes the machine off the list again.</summary>
        [Test]
        public void AStoppedMachineIsRemoved()
        {
            _machine.Start(_idle);
            _machine.Stop();

            Assert.That(StateMachineRegistry.GetRunning(), Has.No.Member(_machine));
        }

        /// <summary>Disposing takes the machine off the list, even while it was still running.</summary>
        [Test]
        public void ADisposedMachineIsRemoved()
        {
            _machine.Start(_idle);
            _machine.Dispose();

            Assert.That(StateMachineRegistry.GetRunning(), Has.No.Member(_machine));
        }

        /// <summary>Starting the same machine twice lists it once, not twice.</summary>
        [Test]
        public void RestartingDoesNotListTheMachineTwice()
        {
            _machine.Start(_idle);
            _machine.Start(_idle);

            Assert.That(TimesListed(_machine), Is.EqualTo(1));
        }

        /// <summary>Several machines running at once are all listed.</summary>
        [Test]
        public void SeveralRunningMachinesAreAllListed()
        {
            StateMachineProbe otherProbe = new();
            StateMachine<StateMachineProbe> other = new(otherProbe);

            _machine.Start(_idle);
            other.Start(otherProbe.CreateState(Idle));

            Assert.That(StateMachineRegistry.GetRunning(), Contains.Item(_machine));
            Assert.That(StateMachineRegistry.GetRunning(), Contains.Item(other));

            other.Dispose();
        }

        // The registry is global, so the count is taken for one machine rather than over the list.
        private static int TimesListed(IStateMachineInfo machine)
        {
            int listed = 0;

            foreach (IStateMachineInfo running in StateMachineRegistry.GetRunning())
            {
                if (ReferenceEquals(running, machine))
                    listed++;
            }

            return listed;
        }
    }
}