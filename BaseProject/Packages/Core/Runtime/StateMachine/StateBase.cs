namespace Base.CorePackage.StateMachine
{
    /// <summary>
    /// Convenience base for <see cref="IState{TContext}"/> implementations. Every hook is optional, so a
    /// state only overrides what it actually uses.
    /// </summary>
    /// <typeparam name="TContext">The object the state operates on.</typeparam>
    public abstract class StateBase<TContext> : IState<TContext>
    {
        /// <inheritdoc/>
        public virtual string Name => _name ??= GetType().Name;

        // The type name never changes for an instance, so it is resolved once instead of on every read.
        private string _name;

        /// <inheritdoc/>
        public virtual void Enter(TContext context) { }

        /// <inheritdoc/>
        public virtual void Tick(TContext context, float deltaTime) { }

        /// <inheritdoc/>
        public virtual void Exit(TContext context) { }
    }
}