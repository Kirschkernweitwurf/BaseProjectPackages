using System;

namespace Base.CorePackage.StateMachine
{
    /// <summary>
    /// A state assembled from delegates, for the many states that are a few lines long and do not
    /// deserve a class of their own.
    /// </summary>
    /// <typeparam name="TContext">The object the state operates on.</typeparam>
    public sealed class DelegateState<TContext> : IState<TContext>
    {
        /// <inheritdoc/>
        public string Name { get; }

        private readonly Action<TContext> _onEnter;
        private readonly Action<TContext, float> _onTick;
        private readonly Action<TContext> _onExit;

        /// <summary>Creates a state from the hooks it needs. Every hook is optional.</summary>
        /// <param name="name">The display name of the state.</param>
        /// <param name="onEnter">Called when the machine switches into this state.</param>
        /// <param name="onTick">Called once per machine tick while this state is active.</param>
        /// <param name="onExit">Called when the machine switches out of this state.</param>
        public DelegateState(string name, Action<TContext> onEnter = null, Action<TContext, float> onTick = null,
            Action<TContext> onExit = null)
        {
            Name = string.IsNullOrEmpty(name)
                ? nameof(DelegateState<TContext>)
                : name;

            _onEnter = onEnter;
            _onTick = onTick;
            _onExit = onExit;
        }

        /// <inheritdoc/>
        public void Enter(TContext context) => _onEnter?.Invoke(context);

        /// <inheritdoc/>
        public void Tick(TContext context, float deltaTime) => _onTick?.Invoke(context, deltaTime);

        /// <inheritdoc/>
        public void Exit(TContext context) => _onExit?.Invoke(context);
    }
}