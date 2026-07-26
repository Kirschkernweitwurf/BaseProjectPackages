namespace Base.CorePackage.Services
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
        /// The default implementation passes the instance along, so an old service dying after a scene reload
        /// cannot remove the replacement that already registered itself.
        /// </summary>
        void Deregister() => ServiceLocator.Deregister(GetType(), this);
    }
}