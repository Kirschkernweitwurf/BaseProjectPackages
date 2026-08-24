using System.Collections.Generic;

namespace Base.CorePackage.StateMachine
{
    /// <summary>
    /// The non generic, read only face of a <see cref="StateMachine{TContext}"/>.
    /// </summary>
    /// <remarks>
    /// Tooling cannot name the context type of an arbitrary machine, so this is what
    /// <see cref="StateMachineRegistry"/> hands out and what the monitor window draws from. It reports the
    /// shape of the machine and where it currently is, and nothing that could change it.
    /// </remarks>
    public interface IStateMachineInfo
    {
        /// <summary>The name the machine was created with.</summary>
        string Name { get; }

        /// <summary>Every state the machine knows, in the order it was told about them.</summary>
        IReadOnlyList<string> StateNames { get; }

        /// <summary>Every transition the machine knows, any state transitions first.</summary>
        IReadOnlyList<StateMachineEdge> Edges { get; }

        /// <summary>The name of the state the machine was started in, or empty before it ran.</summary>
        string InitialStateName { get; }

        /// <summary>The name of the active state, or empty while the machine is not running.</summary>
        string CurrentStateName { get; }

        /// <summary>The name of the state left last, or empty before the first switch.</summary>
        string PreviousStateName { get; }

        /// <summary>The name of the transition that caused the last switch, or the reason a state was forced.</summary>
        string LastTransitionName { get; }

        /// <summary>Seconds the machine has spent in the active state.</summary>
        float TimeInState { get; }

        /// <summary>True between <see cref="StateMachine{TContext}.Start"/> and the matching stop.</summary>
        bool IsRunning { get; }
    }
}