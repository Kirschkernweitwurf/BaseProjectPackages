using UnityEngine;

namespace Base.ServicesPackage
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

    // Load bearing on purpose. Every service in every package derives from this, and the whole class
    // is two lines of registration that have not changed since they were written. An interface in
    // front of it would only add a layer between a service and the locator it registers with.
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