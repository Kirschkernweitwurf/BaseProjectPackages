using UnityEngine;

namespace Base.CorePackage.Services
{
    /// <summary>
    /// Convenience base class for <see cref="MonoBehaviour"/>-based game services.
    /// Automatically handles registration and deregistration with the <see cref="ServiceLocator"/>.
    /// </summary>
    /// <remarks>
    /// Remember to call <c>base.Awake()</c> and <c>base.OnDestroy()</c>
    /// if you override these methods in derived classes.
    /// This can easily be checked by comparing the amount of usages and overrides of these methods in your IDE.
    /// </remarks>
    [DefaultExecutionOrder(ExecutionOrder)]
    public abstract class GameServiceBehaviour : MonoBehaviour, IGameService
    {
        private const int ExecutionOrder = -1;

#region Unity Callbacks
        protected virtual void Awake() => ServiceLocator.Register(GetType(), this);

        protected virtual void OnDestroy() => ServiceLocator.Deregister(GetType(), this);
#endregion
    }
}