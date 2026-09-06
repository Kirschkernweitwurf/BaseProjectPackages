using System.Collections.Generic;
using Base.CorePackage.StateMachine;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// The context the state machine tests run over. It records which states were entered, ticked and left
    /// so a test can state what the machine did rather than inspect what it holds, and it carries the flags
    /// the transition conditions read.
    /// </summary>
    internal sealed class StateMachineProbe
    {
        /// <summary>Names of the states that were entered, in order.</summary>
        internal IReadOnlyList<string> Entered => _entered;

        /// <summary>Names of the states that were left, in order.</summary>
        internal IReadOnlyList<string> Exited => _exited;

        /// <summary>How often the active state was ticked.</summary>
        internal int Ticks { get; private set; }

        /// <summary>Set by a test to open the first transition.</summary>
        internal bool GoLeft { get; set; }

        /// <summary>Set by a test to open the second transition.</summary>
        internal bool GoRight { get; set; }

        private readonly List<string> _entered = new();
        private readonly List<string> _exited = new();

        /// <summary>Builds a state that reports everything it is asked to do into this probe.</summary>
        /// <param name="name">The name of the state.</param>
        /// <returns>The state, ready to be added to a machine.</returns>
        internal IState<StateMachineProbe> CreateState(string name) => new DelegateState<StateMachineProbe>(name,
            onEnter: probe => probe._entered.Add(name),
            onTick: static (probe, _) => probe.Ticks++,
            onExit: probe => probe._exited.Add(name));

        /// <summary>The last state that was entered, or an empty string when none was.</summary>
        /// <returns>The name of the state entered last.</returns>
        internal string LastEntered() => _entered.Count == 0
            ? string.Empty
            : _entered[_entered.Count - 1];
    }
}