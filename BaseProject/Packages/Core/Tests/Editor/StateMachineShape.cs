using System.Collections.Generic;
using Base.CorePackage.StateMachine;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// A machine described by nothing but its states and transitions. The layout reads only those two,
    /// so a test can write the shape it wants without standing up a real machine and running it.
    /// </summary>
    internal sealed class StateMachineShape : IStateMachineInfo
    {
        private readonly List<StateMachineEdge> _edges = new();
        private readonly List<string> _stateNames = new();

        /// <inheritdoc/>
        public string Name => nameof(StateMachineShape);

        /// <inheritdoc/>
        public IReadOnlyList<string> StateNames => _stateNames;

        /// <inheritdoc/>
        public IReadOnlyList<StateMachineEdge> Edges => _edges;

        /// <inheritdoc/>
        public string InitialStateName { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public string CurrentStateName => string.Empty;

        /// <inheritdoc/>
        public string PreviousStateName => string.Empty;

        /// <inheritdoc/>
        public string LastTransitionName => string.Empty;

        /// <inheritdoc/>
        public float TimeInState => 0f;

        /// <inheritdoc/>
        public bool IsRunning => false;

        /// <summary>Adds states in the order the machine was told about them.</summary>
        /// <param name="names">The state names to add.</param>
        /// <returns>The same shape, so a machine reads as one statement.</returns>
        internal StateMachineShape WithStates(params string[] names)
        {
            _stateNames.AddRange(names);

            return this;
        }

        /// <summary>Names the state the machine started in.</summary>
        /// <param name="name">The initial state name.</param>
        /// <returns>The same shape, so a machine reads as one statement.</returns>
        internal StateMachineShape StartingAt(string name)
        {
            InitialStateName = name;

            return this;
        }

        /// <summary>Adds a transition between two states.</summary>
        /// <param name="from">The state it leaves.</param>
        /// <param name="to">The state it leads to.</param>
        /// <returns>The same shape, so a machine reads as one statement.</returns>
        internal StateMachineShape WithEdge(string from, string to)
        {
            _edges.Add(new StateMachineEdge(from, to, from + to, 0));

            return this;
        }

        /// <summary>Adds a transition that fires whichever state is active.</summary>
        /// <param name="to">The state it leads to.</param>
        /// <returns>The same shape, so a machine reads as one statement.</returns>
        internal StateMachineShape WithAnyStateEdge(string to) => WithEdge(string.Empty, to);
    }
}