using System;
using Base.UtilityPackage;
using Base.UtilityPackage.Logging;

namespace Base.CorePackage.StateMachine
{
    /// <summary>
    /// One edge of a <see cref="StateMachine{TContext}"/>: a target state plus the condition that has to
    /// hold for the machine to move there.
    /// </summary>
    /// <typeparam name="TContext">The object the condition is evaluated against.</typeparam>
    public sealed class StateTransition<TContext>
    {
        private const string UnnamedLabel = "Unnamed";

        /// <summary>The state the machine switches to once the condition holds.</summary>
        public IState<TContext> Target { get; }

        /// <summary>Display name used by the logs and by <see cref="IStateMachineInfo.LastTransitionName"/>.</summary>
        public string Name { get; }

        /// <summary>Transitions with a higher priority are evaluated first.</summary>
        public int Priority { get; }

        private readonly Func<TContext, bool> _condition;

        /// <summary>Creates a transition to the given target.</summary>
        /// <param name="target">The state the machine switches to.</param>
        /// <param name="condition">The condition to evaluate. A null condition always holds.</param>
        /// <param name="name">The display name of the transition.</param>
        /// <param name="priority">Higher values are evaluated first.</param>
        public StateTransition(IState<TContext> target, Func<TContext, bool> condition, string name = null,
            int priority = 0)
        {
            Name = string.IsNullOrEmpty(name)
                ? UnnamedLabel
                : name;

            // A transition without a target would strand the machine, so it is caught here rather than
            // when it fires. Interfaces bypass Unity's null operator, hence the explicit check.
            if (!UnityObjectUtility.IsAlive(target))
                CustomLogger.LogError($"Transition \"{Name}\" has no target state and never fires.", null);

            Target = target;
            Priority = priority;

            _condition = condition;
        }

        /// <summary>Evaluates the condition.</summary>
        /// <param name="context">The object the condition is evaluated against.</param>
        /// <returns>True when the machine should move to <see cref="Target"/>.</returns>
        public bool IsMet(TContext context)
        {
            if (!UnityObjectUtility.IsAlive(Target))
                return false;

            return _condition == null || _condition.Invoke(context);
        }
    }
}