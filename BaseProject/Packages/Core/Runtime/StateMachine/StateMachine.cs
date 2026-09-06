using System;
using System.Collections.Generic;
using Base.UtilityPackage;
using Base.UtilityPackage.Logging;

namespace Base.CorePackage.StateMachine
{
    /// <summary>
    /// A finite state machine over an arbitrary context object.
    /// <para>
    /// The machine does not tick itself. Drive it from wherever the owning object updates, so its rate and
    /// its time scale stay under the caller's control.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">The object the states and conditions operate on.</typeparam>
    /// <example>
    /// <code>
    /// StateMachine&lt;Enemy&gt; machine = new(this, "Enemy");
    /// 
    /// machine.AddTransition(patrol, chase, static enemy =&gt; enemy.CanSeePlayer, "SeesPlayer");
    /// machine.AddAnyTransition(dead, static enemy =&gt; enemy.Health &lt;= 0, "Died", priority: 10);
    /// machine.Start(patrol);
    /// </code>
    /// </example>
    public sealed class StateMachine<TContext> : IStateMachineInfo, IDisposable
    {
        private const string ForcedReason = "Forced";

        /// <summary>How many switches a single tick may chain before the machine assumes a condition loop.</summary>
        private const int MaxChainedSwitches = 8;

        private const string StartedReason = "Started";

        /// <summary>
        /// Raised after a switch completed, so a listener already sees the new state as the active one.
        /// </summary>
        public event Action<StateChange<TContext>> StateChanged;

        /// <summary>The object handed to every state and condition.</summary>
        public TContext Context { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>The active state, or null while the machine is not running.</summary>
        public IState<TContext> CurrentState { get; private set; }

        /// <summary>The state left last, or null before the first switch.</summary>
        public IState<TContext> PreviousState { get; private set; }

        /// <inheritdoc/>
        public string InitialStateName { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public string LastTransitionName { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public float TimeInState { get; private set; }

        /// <inheritdoc/>
        public bool IsRunning { get; private set; }

        /// <inheritdoc/>
        public string CurrentStateName => NameOf(CurrentState);

        /// <inheritdoc/>
        public string PreviousStateName => NameOf(PreviousState);

        /// <inheritdoc/>
        public IReadOnlyList<string> StateNames
        {
            get
            {
                RebuildShape();

                return _stateNames;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<StateMachineEdge> Edges
        {
            get
            {
                RebuildShape();

                return _edges;
            }
        }

        private readonly Dictionary<IState<TContext>, List<StateTransition<TContext>>> _transitions = new();
        private readonly List<StateTransition<TContext>> _anyTransitions = new();
        private readonly List<IState<TContext>> _states = new();
        private readonly List<StateMachineEdge> _edges = new();
        private readonly List<string> _stateNames = new();

        private bool _isShapeStale = true;

        /// <summary>Creates an empty machine.</summary>
        /// <param name="context">The object handed to every state and condition.</param>
        /// <param name="name">The name shown in the logs and in the monitor window.</param>
        public StateMachine(TContext context, string name = null)
        {
            Context = context;

            Name = string.IsNullOrEmpty(name)
                ? nameof(StateMachine<TContext>)
                : name;
        }

        /// <summary>
        /// Stops the machine and drops every listener, so a machine owned by a destroyed object leaves
        /// nothing behind.
        /// </summary>
        public void Dispose()
        {
            Stop();

            StateChanged = null;
        }

        /// <summary>
        /// Adds a transition that is only evaluated while <paramref name="from"/> is the active state.
        /// </summary>
        /// <param name="from">The state the transition leaves.</param>
        /// <param name="to">The state the transition leads to.</param>
        /// <param name="condition">The condition to evaluate. Null always holds.</param>
        /// <param name="name">The display name of the transition.</param>
        /// <param name="priority">Higher values are evaluated first.</param>
        public void AddTransition(IState<TContext> from, IState<TContext> to, Func<TContext, bool> condition,
            string name = null, int priority = 0)
        {
            if (!UnityObjectUtility.IsAlive(from))
            {
                CustomLogger.LogError($"{Name} cannot add a transition without a source state.", null);
                return;
            }

            Track(from);
            Track(to);

            if (!_transitions.TryGetValue(from, out List<StateTransition<TContext>> outgoing))
            {
                outgoing = new List<StateTransition<TContext>>();
                _transitions[from] = outgoing;
            }

            InsertByPriority(outgoing, new StateTransition<TContext>(to, condition, name, priority));

            _isShapeStale = true;
        }

        /// <summary>
        /// Adds a transition that is evaluated no matter which state is active. Any state transitions are
        /// checked before the ones leaving the active state, and never fire into the active state itself.
        /// </summary>
        /// <param name="to">The state the transition leads to.</param>
        /// <param name="condition">The condition to evaluate. Null always holds.</param>
        /// <param name="name">The display name of the transition.</param>
        /// <param name="priority">Higher values are evaluated first.</param>
        public void AddAnyTransition(IState<TContext> to, Func<TContext, bool> condition, string name = null,
            int priority = 0)
        {
            Track(to);

            InsertByPriority(_anyTransitions, new StateTransition<TContext>(to, condition, name, priority));

            _isShapeStale = true;
        }

        /// <summary>
        /// Enters the given state and starts evaluating transitions. Restarts the machine if it was already
        /// running.
        /// </summary>
        /// <param name="initialState">The state to enter.</param>
        public void Start(IState<TContext> initialState)
        {
            if (!UnityObjectUtility.IsAlive(initialState))
            {
                CustomLogger.LogError($"{Name} cannot start without an initial state.", null);
                return;
            }

            if (IsRunning)
                Stop();

            Track(initialState);

            IsRunning = true;
            PreviousState = null;
            CurrentState = initialState;
            InitialStateName = initialState.Name;
            LastTransitionName = StartedReason;
            TimeInState = 0f;

            StateMachineRegistry.Register(this);

            initialState.Enter(Context);

            StateChanged?.Invoke(new StateChange<TContext>(null, initialState, StartedReason));
        }

        /// <summary>
        /// Evaluates the transitions and ticks the resulting active state. Does nothing while the machine is
        /// not running.
        /// </summary>
        /// <param name="deltaTime">Seconds since the previous tick.</param>
        public void Tick(float deltaTime)
        {
            if (!IsRunning)
                return;

            TimeInState += deltaTime;

            // A pass through state can hand off within the same tick, so switching repeats until the machine
            // settles. The cap is what keeps a ring of conditions that all hold from spinning forever.
            for (int i = 0; i < MaxChainedSwitches; i++)
            {
                StateTransition<TContext> transition = FindTransition();

                if (transition == null)
                {
                    TickCurrent(deltaTime);
                    return;
                }

                SwitchTo(transition.Target, transition.Name);

                // A state may stop the machine from inside Enter.
                if (!IsRunning)
                    return;
            }

            CustomLogger.LogWarning($"{Name} switched {MaxChainedSwitches} times in one tick and stopped at "
                + $"\"{CurrentStateName}\". Check the conditions leaving it for a loop.", null);

            TickCurrent(deltaTime);
        }

        /// <summary>
        /// Switches to the given state without asking any condition. Use for external events the transition
        /// table does not model, such as a death or a scene change.
        /// </summary>
        /// <param name="state">The state to enter.</param>
        /// <param name="reason">The reason reported as the last transition name.</param>
        public void ForceState(IState<TContext> state, string reason = null)
        {
            if (!IsRunning)
            {
                CustomLogger.LogWarning($"{Name} is not running, so no state can be forced.", null);
                return;
            }

            if (!UnityObjectUtility.IsAlive(state))
            {
                CustomLogger.LogError($"{Name} cannot switch to a missing state.", null);
                return;
            }

            Track(state);

            SwitchTo(state, string.IsNullOrEmpty(reason)
                ? ForcedReason
                : reason);
        }

        /// <summary>Exits the active state and stops evaluating transitions.</summary>
        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;

            StateMachineRegistry.Unregister(this);

            if (UnityObjectUtility.IsAlive(CurrentState))
                CurrentState.Exit(Context);

            PreviousState = CurrentState;
            CurrentState = null;
        }

        // Keeps the list ordered by descending priority while leaving equal priorities in the order they
        // were added, so the table reads the same way it runs.
        private static void InsertByPriority(List<StateTransition<TContext>> transitions,
            StateTransition<TContext> transition)
        {
            int index = transitions.Count;

            while (index > 0 && transitions[index - 1].Priority < transition.Priority)
                index--;

            transitions.Insert(index, transition);
        }

        // A state can be a Unity object behind the interface, so a destroyed one has to be caught here
        // rather than trusted to compare equal to null.
        private static string NameOf(IState<TContext> state) => UnityObjectUtility.IsAlive(state)
            ? state.Name
            : string.Empty;

        // Every state the machine is told about is remembered in first seen order, which is the only way
        // the monitor can list a state that nothing happens to point at yet.
        private void Track(IState<TContext> state)
        {
            if (!UnityObjectUtility.IsAlive(state) || _states.Contains(state))
                return;

            _states.Add(state);

            _isShapeStale = true;
        }

        // The shape only changes while a machine is being wired up, so it is folded into name lists once
        // and then handed out untouched for the rest of the run.
        private void RebuildShape()
        {
            if (!_isShapeStale)
                return;

            _isShapeStale = false;

            _stateNames.Clear();
            _edges.Clear();

            foreach (IState<TContext> state in _states)
                _stateNames.Add(NameOf(state));

            foreach (StateTransition<TContext> transition in _anyTransitions)
            {
                _edges.Add(new StateMachineEdge(string.Empty, NameOf(transition.Target), transition.Name,
                    transition.Priority));
            }

            foreach (IState<TContext> state in _states)
            {
                if (!_transitions.TryGetValue(state, out List<StateTransition<TContext>> outgoing))
                    continue;

                foreach (StateTransition<TContext> transition in outgoing)
                {
                    _edges.Add(new StateMachineEdge(NameOf(state), NameOf(transition.Target), transition.Name,
                        transition.Priority));
                }
            }
        }

        private void TickCurrent(float deltaTime)
        {
            if (!UnityObjectUtility.IsAlive(CurrentState))
                return;

            CurrentState.Tick(Context, deltaTime);
        }

        private StateTransition<TContext> FindTransition()
        {
            foreach (StateTransition<TContext> transition in _anyTransitions)
            {
                if (ReferenceEquals(transition.Target, CurrentState))
                    continue;

                if (transition.IsMet(Context))
                    return transition;
            }

            if (!_transitions.TryGetValue(CurrentState, out List<StateTransition<TContext>> outgoing))
                return null;

            foreach (StateTransition<TContext> transition in outgoing)
            {
                if (transition.IsMet(Context))
                    return transition;
            }

            return null;
        }

        private void SwitchTo(IState<TContext> next, string reason)
        {
            IState<TContext> left = CurrentState;

            if (UnityObjectUtility.IsAlive(left))
                left.Exit(Context);

            PreviousState = left;
            CurrentState = next;
            LastTransitionName = reason;
            TimeInState = 0f;

            next.Enter(Context);

            StateChanged?.Invoke(new StateChange<TContext>(left, next, reason));
        }
    }
}