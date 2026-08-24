namespace Base.CorePackage.StateMachine
{
    /// <summary>
    /// Describes a switch between two states, raised by <see cref="StateMachine{TContext}.StateChanged"/>.
    /// </summary>
    /// <typeparam name="TContext">The object the states operate on.</typeparam>
    public readonly struct StateChange<TContext>
    {
        /// <summary>The state that was left, or null when the machine just started.</summary>
        public IState<TContext> From { get; }

        /// <summary>The state that is now active.</summary>
        public IState<TContext> To { get; }

        /// <summary>The name of the transition that fired, or the reason a state was forced.</summary>
        public string Reason { get; }

        /// <summary>Creates the description of a single state switch.</summary>
        /// <param name="from">The state that was left.</param>
        /// <param name="to">The state that is now active.</param>
        /// <param name="reason">The transition name or the reason the state was forced.</param>
        public StateChange(IState<TContext> from, IState<TContext> to, string reason)
        {
            From = from;
            To = to;
            Reason = reason;
        }
    }
}