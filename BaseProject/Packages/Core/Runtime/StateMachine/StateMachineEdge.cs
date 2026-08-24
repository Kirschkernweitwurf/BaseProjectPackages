namespace Base.CorePackage.StateMachine
{
    /// <summary>
    /// One transition of a machine, described in names rather than in object references so tooling can
    /// read the shape of a machine without knowing its context type.
    /// </summary>
    public readonly struct StateMachineEdge
    {
        /// <summary>The state the transition leaves, or empty when it fires from any state.</summary>
        public string From { get; }

        /// <summary>The state the transition leads to.</summary>
        public string To { get; }

        /// <summary>The display name of the transition.</summary>
        public string Name { get; }

        /// <summary>Transitions with a higher priority are evaluated first.</summary>
        public int Priority { get; }

        /// <summary>True when this transition is evaluated no matter which state is active.</summary>
        public bool IsFromAnyState => string.IsNullOrEmpty(From);

        /// <summary>Describes a single transition.</summary>
        /// <param name="from">The state it leaves, or empty for an any state transition.</param>
        /// <param name="to">The state it leads to.</param>
        /// <param name="name">The display name of the transition.</param>
        /// <param name="priority">Higher values are evaluated first.</param>
        public StateMachineEdge(string from, string to, string name, int priority)
        {
            From = from;
            To = to;
            Name = name;
            Priority = priority;
        }
    }
}