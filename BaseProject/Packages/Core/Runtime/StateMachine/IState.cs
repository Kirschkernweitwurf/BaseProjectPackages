namespace Base.CorePackage.StateMachine
{
    /// <summary>
    /// One state of a <see cref="StateMachine{TContext}"/>.
    /// </summary>
    /// <remarks>
    /// The context is handed to every call instead of being captured in the state, so one state instance
    /// can be shared between several machines running over different contexts.
    /// </remarks>
    /// <typeparam name="TContext">The object the state operates on.</typeparam>
    public interface IState<in TContext>
    {
        /// <summary>Display name used by the graph window, the logs and <see cref="IStateMachineInfo"/>.</summary>
        string Name { get; }

        /// <summary>Called once when the machine switches into this state.</summary>
        /// <param name="context">The object the state operates on.</param>
        void Enter(TContext context);

        /// <summary>Called once per machine tick while this state is the active one.</summary>
        /// <param name="context">The object the state operates on.</param>
        /// <param name="deltaTime">Seconds since the previous tick.</param>
        void Tick(TContext context, float deltaTime);

        /// <summary>Called once when the machine switches out of this state.</summary>
        /// <param name="context">The object the state operates on.</param>
        void Exit(TContext context);
    }
}