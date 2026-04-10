using UnityEngine;

namespace Systems.Services
{
    /// <summary>
    /// Convenience base class for <see cref="MonoBehaviour"/>-based game services.
    /// Automatically handles registration and deregistration with the <see cref="ServiceLocator"/>.
    /// </summary>
    /// <remarks>
    /// Remember to call <c>base.Awake()</c> and <c>base.OnDestroy()</c> if you override these methods in derived classes.
    /// This can easily be checked by comparing the amount of usages and overrides of these methods in your IDE.
    /// </remarks>
    [DefaultExecutionOrder(-1)]
    public abstract class GameServiceBehaviour : MonoBehaviour, IGameService
    {
        protected virtual void Awake() => ((IGameService)this).Register();

        protected virtual void OnDestroy() => ((IGameService)this).Deregister();
    }
}