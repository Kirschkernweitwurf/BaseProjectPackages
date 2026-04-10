namespace Systems.Services
{
    /// <summary>
    /// Interface for game services that can be registered with the <see cref="ServiceLocator"/>.
    /// Implement this interface to define a service that can be accessed globally.
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Called when the service is initialized or registered.
        /// The default implementation automatically registers the service by type.
        /// </summary>
        void Register() => ServiceLocator.Register(GetType(), this);

        /// <summary>
        /// Called when the service is being destroyed or deregistered.
        /// The default implementation automatically deregisters the service.
        /// </summary>
        void Deregister() => ServiceLocator.Deregister(GetType());
    }
}